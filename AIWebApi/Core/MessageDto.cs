using System.ComponentModel;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

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
}

public class MessageDto(Role role, string message, List<ImageDto>? images = null)
{
    public Role Role { get; set; } = role;
    public string Message { get; set; } = message;
    public List<ImageDto>? Images { get; set; } = images;

    public virtual ChatMessageContent ToKernelMessage()
    {
        if (Images is not null)
        {
            ChatMessageContentItemCollection items = [new TextContent(Message)];
            foreach (ImageDto image in Images)
            {
                items.Add(new ImageContent(image.BinaryData, image.ImageType.GetDescription()));
            }

            return Role switch
            {
                Role.User => new ChatMessageContent(AuthorRole.User, items),
                Role.Assistant => new ChatMessageContent(AuthorRole.Assistant, items),
                Role.System => new ChatMessageContent(AuthorRole.System, items),
                Role.Tool => new ChatMessageContent(AuthorRole.Tool, items),
                _ => throw new NotImplementedException(),
            };
        }

        return Role switch
        {
            Role.User => new ChatMessageContent(AuthorRole.User, Message),
            Role.Assistant => new ChatMessageContent(AuthorRole.Assistant, Message),
            Role.System => new ChatMessageContent(AuthorRole.System, Message),
            Role.Tool => new ChatMessageContent(AuthorRole.Tool, Message),
            _ => throw new NotImplementedException(),
        };
    }
}

public static class ListMessageExtensions
{
    public static IList<ChatMessageContent> ToKernelMessages(this IList<MessageDto> list)
    {
        List<ChatMessageContent> listMessages = [];
        listMessages.AddRange(list.Select(m => m.ToKernelMessage()));
        return listMessages;
    }
}
