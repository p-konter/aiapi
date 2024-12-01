using System.ComponentModel;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AIWebApi.Core;

public interface IKernelService
{
    void ClearHistory();

    Task<MessageDto> Chat(AIModel model, IList<MessageDto> messages);

    Task<string> SimpleChat(AIModel model, string message);

    Task<string> AudioTranscription(string fileName);
}

public class KernelService : IKernelService
{
    private readonly ILogger<KernelService> _logger;
    protected readonly IFileService _fileService;

    private const string OpenAIApiKey = "OpenAIApiKey";
    protected readonly string TempDataExtension = ".log";

    private readonly Kernel _kernel;
    private readonly ChatHistory History = [];

    public KernelService(IConfiguration configuration, IFileService fileService, ILogger<KernelService> logger)
    {
        _fileService = fileService;
        _logger = logger;

        string openAIApiKey = configuration.GetStrictValue<string>(OpenAIApiKey);
        IKernelBuilder builder = Kernel.CreateBuilder();

        // Add models
        builder.AddOpenAIChatCompletion(AIModel.Gpt4o.GetDescription(), openAIApiKey, serviceId: AIModel.Gpt4o.GetDescription());
        builder.AddOpenAIChatCompletion(AIModel.Gpt4oMini.GetDescription(), openAIApiKey, serviceId: AIModel.Gpt4oMini.GetDescription());
        builder.AddOpenAIAudioToText(AIModel.Whisper1.GetDescription(), openAIApiKey, serviceId: AIModel.Whisper1.GetDescription());

        builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
        _kernel = builder.Build();

        // Add plugins
        //_kernel.Plugins.AddFromType<LightsPlugin>("Lights");
    }

    public async Task<string> SimpleChat(AIModel model, string message)
    {
        IChatCompletionService chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>(model.GetDescription());
        ChatMessageContent result = await chatCompletionService.GetChatMessageContentAsync(message);
        return result.Content ?? string.Empty;
    }

    public async Task<MessageDto> Chat(AIModel model, IList<MessageDto> messages)
    {
        History.AddRange(messages.ToKernelMessages());

        IChatCompletionService chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>(model.GetDescription());
        ChatMessageContent result = await chatCompletionService.GetChatMessageContentAsync(History);

        _logger.LogInformation("Chat completion: {completion}", result.Content);
        return new MessageDto(Role.Assistant, result.Content ?? string.Empty);
    }

    public async Task<string> AudioTranscription(string fileName)
    {
        string textFileName = _fileService.ChangeExtension(fileName, TempDataExtension);
        string? data = await _fileService.ReadTextFile(textFileName);
        if (data is not null)
        {
            return data;
        }
        _logger.LogInformation("Filename: {name}", fileName);
        FileStream audioFile = _fileService.ReadStream(fileName);
        AudioContent audioContent = new(await BinaryData.FromStreamAsync(audioFile), mimeType: null);

        OpenAIAudioToTextExecutionSettings executionSettings = new(fileName)
        {
            Language = "pl"
        };

        IAudioToTextService audioToTextService = _kernel.GetRequiredService<IAudioToTextService>(AIModel.Whisper1.GetDescription());
        TextContent transcription = await audioToTextService.GetTextContentAsync(audioContent, executionSettings);

        _logger.LogInformation("Transcription text: {text}", transcription.Text);
        await _fileService.WriteTextFile(textFileName, transcription.Text ?? string.Empty);

        return transcription.Text ?? string.Empty;
    }

    public void ClearHistory() => History.Clear();

    private static OpenAIPromptExecutionSettings KernelSettings()
    {
        return new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
    }
}

public enum AIModel
{
    [Description("gpt-4o")]
    Gpt4o,
    [Description("gpt-4o-mini")]
    Gpt4oMini,
    [Description("whisper-1")]
    Whisper1,
}
