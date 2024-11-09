using OpenAI.Chat;

namespace AIWebApi.Core;

public enum Role
{
    User,
    Assistant,
    System
}

public class MessageDto(Role role, string message)
{
    public Role Role { get; set; } = role;
    public string Message { get; set; } = message;

    public ChatMessage ToChatMessage()
    {
        return Role switch
        {
            Role.User => new UserChatMessage(Message),
            Role.Assistant => new AssistantChatMessage(Message),
            Role.System => new SystemChatMessage(Message),
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
}
