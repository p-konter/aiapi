using System.ComponentModel;

using Microsoft.SemanticKernel.ChatCompletion;

using OpenAI.Chat;

namespace AIWebApi.Core;

public enum Role
{
    User,
    Assistant,
    System,
    Tool
}

public enum ImageType
{
    [Description("image/png")]
    Png,
}

public class ImageDto(BinaryData binaryData, ImageType imageType)
{
    public BinaryData BinaryData { get; } = binaryData;
    public ImageType ImageType { get; } = imageType;

    public ChatMessageContentPart ToChatMessageContent() => ChatMessageContentPart.CreateImagePart(BinaryData, ImageType.GetDescription());
}

public class MessageDto(Role role, string message, List<ImageDto>? images = null)
{
    public Role Role { get; set; } = role;
    public string Message { get; set; } = message;
    public List<ImageDto>? Images { get; set; } = images;

    public virtual Microsoft.SemanticKernel.ChatMessageContent ToKernelMessage()
    {
        if (Images is not null)
        {
            ChatMessageContentItemCollection items = [new Microsoft.SemanticKernel.TextContent(Message)];
            foreach (ImageDto image in Images)
            {
                items.Add(new Microsoft.SemanticKernel.ImageContent(image.BinaryData, image.ImageType.GetDescription()));
            }

            return Role switch
            {
                Role.User => new Microsoft.SemanticKernel.ChatMessageContent(AuthorRole.User, items),
                Role.Assistant => new Microsoft.SemanticKernel.ChatMessageContent(AuthorRole.Assistant, items),
                Role.System => new Microsoft.SemanticKernel.ChatMessageContent(AuthorRole.System, items),
                Role.Tool => new Microsoft.SemanticKernel.ChatMessageContent(AuthorRole.Tool, items),
                _ => throw new NotImplementedException(),
            };
        }

        return Role switch
        {
            Role.User => new Microsoft.SemanticKernel.ChatMessageContent(AuthorRole.User, Message),
            Role.Assistant => new Microsoft.SemanticKernel.ChatMessageContent(AuthorRole.Assistant, Message),
            Role.System => new Microsoft.SemanticKernel.ChatMessageContent(AuthorRole.System, Message),
            Role.Tool => new Microsoft.SemanticKernel.ChatMessageContent(AuthorRole.Tool, Message),
            _ => throw new NotImplementedException(),
        };
    }

    public virtual ChatMessage ToChatMessage()
    {
        List<ChatMessageContentPart> parts = [ChatMessageContentPart.CreateTextPart(Message)];

        if (Images is not null)
        {
            parts.AddRange(Images.ToChatMessageContentParts());
        }

        return Role switch
        {
            Role.User => new UserChatMessage(parts),
            Role.Assistant => new AssistantChatMessage(parts),
            Role.System => new SystemChatMessage(parts),
            _ => throw new NotImplementedException(),
        };
    }
}

public static class ListMessageExtensions
{
    public static IList<Microsoft.SemanticKernel.ChatMessageContent> ToKernelMessages(this IList<MessageDto> list)
    {
        List<Microsoft.SemanticKernel.ChatMessageContent> listMessages = [];
        listMessages.AddRange(list.Select(m => m.ToKernelMessage()));
        return listMessages;
    }

    public static IList<ChatMessage> ToChatMessages(this IList<MessageDto> list)
    {
        List<ChatMessage> listMessages = [];
        listMessages.AddRange(list.Select(m => m.ToChatMessage()));
        return listMessages;
    }

    public static IList<ChatMessageContentPart> ToChatMessageContentParts(this IList<ImageDto> list)
    {
        List<ChatMessageContentPart> listMessages = [];
        listMessages.AddRange(list.Select(m => m.ToChatMessageContent()));
        return listMessages;
    }
}
