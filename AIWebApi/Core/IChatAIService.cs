namespace AIWebApi.Core;

public interface IChatAIService
{
    Task<string> SimpleChat(string message);

    Task<MessageDto> ThreadChat(IList<MessageDto> messages);
}

public interface IGPT4AIService : IChatAIService;

public interface IGPT4MiniAIService : IChatAIService;
