using System.ComponentModel;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AIWebApi.Core;

public interface IKernelService
{
    void ClearHistory();

    Task<MessageDto> Chat(AIModel model, IList<MessageDto> messages);

    Task<string> SimpleChat(AIModel model, string message);
}

public class KernelService : IKernelService
{
    private readonly ILogger<KernelService> _logger;
    private const string OpenAIApiKey = "OpenAIApiKey";
    private readonly Kernel _kernel;

    private readonly ChatHistory History = [];

    public KernelService(IConfiguration configuration, ILogger<KernelService> logger)
    {
        _logger = logger;

        string openAIApiKey = configuration.GetStrictValue<string>(OpenAIApiKey);
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(AIModel.Gpt4o.GetDescription(), openAIApiKey, serviceId: AIModel.Gpt4o.GetDescription());
        builder.AddOpenAIChatCompletion(AIModel.Gpt4oMini.GetDescription(), openAIApiKey, serviceId: AIModel.Gpt4oMini.GetDescription());
        builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
        _kernel = builder.Build();

        //kernel.Plugins.AddFromType<LightsPlugin>("Lights");

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
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
}
