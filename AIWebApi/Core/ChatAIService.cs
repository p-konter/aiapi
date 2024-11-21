using OpenAI.Chat;

namespace AIWebApi.Core;

public class ChatAIService(string model, IConfiguration configuration, IFileService fileService, ILogger<ChatAIService> logger)
    : BaseFileAIService(fileService), IGPT4AIService, IGPT4MiniAIService
{
    private const string OpenAIApiKey = "OpenAIApiKey";
    private readonly ChatClient _client = new(model, configuration.GetStrictValue<string>(OpenAIApiKey));
    private readonly ILogger<ChatAIService> _logger = logger;

    public async Task<string> SimpleChat(string message)
    {
        ChatCompletion completion = await _client.CompleteChatAsync(message);
        return completion.Content[0].Text;
    }

    public async Task<MessageDto> Chat(IList<MessageDto> messages) => await ProcessChatAsync(messages.ToChatMessages());

    public async Task<MessageDto> JsonChat(IList<MessageDto> messages)
    {
        MessageDto dto = await ProcessChatAsync(messages.ToChatMessages());
        dto.Message = dto.Message.Replace("```json", "").Replace("```", "").Trim();
        return dto;
    }

    public async Task<MessageDto> ReadImageChat(string fileName, string prompt)
    {
        string? data = await LoadProcessedData(fileName);
        if (data is not null)
        {
            return new MessageDto(Role.Assistant, data);
        }

        BinaryData file = await ReadBinaryFile(fileName);
        List<MessageDto> messages = [new(Role.User, prompt, [new ImageDto(file, ImageType.Png)])];

        MessageDto response = await ProcessChatAsync(messages.ToChatMessages());
        await SaveProcessedData(fileName, response.Message);

        return response;
    }

    private async Task<MessageDto> ProcessChatAsync(IList<ChatMessage> listMessages)
    {
        ChatCompletion completion = await _client.CompleteChatAsync(listMessages);
        _logger.LogInformation("Chat completion: {completion}", completion.Content[0].Text);
        return new MessageDto(Role.Assistant, completion.Content[0].Text);
    }
}
