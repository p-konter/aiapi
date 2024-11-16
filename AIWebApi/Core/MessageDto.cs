using System.ComponentModel;

using OpenAI.Chat;

namespace AIWebApi.Core;

public enum Role
{
    User,
    Assistant,
    System
}

public enum ImageType
{
    [Description("image/png")]
    Png
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
