﻿using System.ComponentModel;
using System.Text;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AudioToText;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextToImage;

namespace AIWebApi.Core;

public interface IKernelService
{
    void ClearHistory();

    Task<MessageDto> Chat(AIModel model, IList<MessageDto> messages);

    Task<string> SimpleChat(AIModel model, string message);

    Task<MessageDto> ImageChat(AIModel model, string fileName, string prompt);

    Task<string> AudioTranscription(string fileName);

    Task<Uri> GenerateImage(string prompt);

    Task<float[]> CreateVector(string description);
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
        builder.AddOpenAITextToImage(openAIApiKey, serviceId: AIModel.DallE3.GetDescription());
        builder.AddOpenAITextEmbeddingGeneration(AIModel.TextEmbedding3Small.GetDescription(), openAIApiKey, serviceId: AIModel.TextEmbedding3Small.GetDescription());

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

    public async Task<MessageDto> ImageChat(AIModel model, string fileName, string prompt)
    {
        string? data = await LoadProcessedData(fileName);
        if (data is not null)
        {
            return new MessageDto(Role.Assistant, data);
        }

        BinaryData file = await _fileService.ReadBinaryFile(fileName);
        MessageDto message = new(Role.User, prompt, [new ImageDto(file, ImageType.Png)]);

        History.Add(message.ToKernelMessage());
        IChatCompletionService chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>(model.GetDescription());
        ChatMessageContent result = await chatCompletionService.GetChatMessageContentAsync(History);

        _logger.LogInformation("Image text: {text}", result.Content);
        await SaveProcessedData(fileName, result.Content);

        return new MessageDto(Role.Assistant, result.Content ?? string.Empty);
    }

    public async Task<string> AudioTranscription(string fileName)
    {
        string? data = await LoadProcessedData(fileName);
        if (data is not null)
        {
            return data;
        }

        FileStream audioFile = _fileService.ReadStream(fileName);
        AudioContent audioContent = new(await BinaryData.FromStreamAsync(audioFile), mimeType: null);

        OpenAIAudioToTextExecutionSettings executionSettings = new(fileName)
        {
            Language = "pl"
        };

        IAudioToTextService audioToTextService = _kernel.GetRequiredService<IAudioToTextService>(AIModel.Whisper1.GetDescription());
        TextContent transcription = await audioToTextService.GetTextContentAsync(audioContent, executionSettings);

        _logger.LogInformation("Transcription text: {text}", transcription.Text);
        await SaveProcessedData(fileName, transcription.Text);

        return transcription.Text ?? string.Empty;
    }

    public async Task<Uri> GenerateImage(string prompt)
    {
        OpenAITextToImageExecutionSettings executionSettings = new()
        {
            Size = (1024, 1024),
        };

        ITextToImageService textToImageService = _kernel.GetRequiredService<ITextToImageService>(AIModel.DallE3.GetDescription());
        IReadOnlyList<ImageContent> image = await textToImageService.GetImageContentsAsync(new TextContent(prompt), executionSettings);
        _logger.LogInformation("Generated image: {uri}", image[0].Uri);

        return image[0].Uri!;
    }

    public async Task<float[]> CreateVector(string description)
    {
        ITextEmbeddingGenerationService textEmbeddingService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>(AIModel.TextEmbedding3Small.GetDescription());
        ReadOnlyMemory<float> vector = await textEmbeddingService.GenerateEmbeddingAsync(description);

        _logger.LogInformation("Create vector: dimension: {dimension}", vector.Length);

        float[] vectorArray = vector.Span.ToArray();
        StringBuilder sp = new();
        sp.Append(vectorArray.Select((value, index) => $"  [{index,4}] = {value}"));
        _logger.LogInformation("Floats: {floats}", sp.ToString());

        return vectorArray;
    }

    public void ClearHistory() => History.Clear();

    private async Task<string?> LoadProcessedData(string fileName)
    {
        string textFileName = _fileService.ChangeExtension(fileName, TempDataExtension);
        return await _fileService.ReadTextFile(textFileName);
    }

    protected async Task SaveProcessedData(string fileName, string? content)
    {
        if (content != null)
        {
            string textFileName = _fileService.ChangeExtension(fileName, TempDataExtension);
            await _fileService.WriteTextFile(textFileName, content);
        }
    }

    //private static OpenAIPromptExecutionSettings KernelSettings()
    //{
    //    return new()
    //    {
    //        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
    //        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    //    };
    //}
}

public enum AIModel
{
    [Description("gpt-4o")]
    Gpt4o,
    [Description("gpt-4o-mini")]
    Gpt4oMini,
    [Description("whisper-1")]
    Whisper1,
    [Description("dall-e-3")]
    DallE3,
    [Description("text-embedding-3-small")]
    TextEmbedding3Small
}
