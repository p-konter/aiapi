using OpenAI.Chat;

namespace AIWebApi.Core;

public interface IOpenAIService
{
    Task<string> SimpleChat(string message);
}

public class OpenAIService(IConfiguration configuration) : IOpenAIService
{
    private const string OpenAIApiKey = "OpenAIApiKey";
    private const string OpenAIModel = "gpt-4o";

    private readonly ChatClient _client = new(OpenAIModel, configuration.GetStrictValue<string>(OpenAIApiKey));

    public async Task<string> SimpleChat(string message)
    {
        ChatCompletion completion = await _client.CompleteChatAsync(message);
        return completion.Content[0].Text;
    }
}
