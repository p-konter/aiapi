namespace AIWebApi.Core;

public interface IChatAIService
{
    Task<string> SimpleChat(string message);

    Task<MessageDto> Chat(IList<MessageDto> messages);

    Task<MessageDto> JsonChat(IList<MessageDto> messages);

    Task<MessageDto> ReadImageChat(string fileName, string prompt);
}

public interface IGPT4AIService : IChatAIService;

public interface IGPT4MiniAIService : IChatAIService;
