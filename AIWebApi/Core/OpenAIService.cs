using System.ComponentModel;

using OpenAI.Chat;

namespace AIWebApi.Core;

public class OpenAIService(ChatModel model, IConfiguration configuration)
{
    private const string OpenAIApiKey = "OpenAIApiKey";

    private readonly ChatClient _client = new(model.GetDescription(), configuration.GetStrictValue<string>(OpenAIApiKey));

    public async Task<string> SimpleChat(string message)
    {
        ChatCompletion completion = await _client.CompleteChatAsync(message);
        return completion.Content[0].Text;
    }

    public async Task<MessageDto> ThreadChat(IList<MessageDto> messages)
    {
        IList<ChatMessage> listMessages = messages.ToChatMessages();
        ChatCompletion completion = await _client.CompleteChatAsync(listMessages);
        return new MessageDto(Role.Assistant, completion.Content[0].Text);
    }
}

public enum ChatModel
{
    [Description("gpt-4o")]
    GPT_40,

    [Description("gpt-4o-mini")]
    GPT_40_Mini
}
