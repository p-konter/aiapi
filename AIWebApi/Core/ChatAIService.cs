using OpenAI.Chat;

namespace AIWebApi.Core;

public class ChatAIService(string model, IConfiguration configuration, ILogger<ChatAIService> logger) : IGPT4AIService, IGPT4MiniAIService
{
    private const string OpenAIApiKey = "OpenAIApiKey";
    private readonly ChatClient _client = new(model, configuration.GetStrictValue<string>(OpenAIApiKey));
    private readonly ILogger<ChatAIService> _logger = logger;

    public async Task<string> SimpleChat(string message)
    {
        ChatCompletion completion = await _client.CompleteChatAsync(message);
        return completion.Content[0].Text;
    }

    public async Task<MessageDto> ThreadChat(IList<MessageDto> messages)
    {
        IList<ChatMessage> listMessages = messages.ToChatMessages();
        ChatCompletion completion = await _client.CompleteChatAsync(listMessages);
        _logger.LogInformation("Chat completion: {completion}", completion.Content[0].Text);

        return new MessageDto(Role.Assistant, completion.Content[0].Text);
    }
}
