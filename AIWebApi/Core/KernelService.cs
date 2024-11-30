using System.ComponentModel;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AIWebApi.Core;

public interface IKernelService
{
    Task<string> SimpleChat(AIModel model, string message);
}

public class KernelService : IKernelService
{
    private const string OpenAIApiKey = "OpenAIApiKey";
    private readonly Kernel _kernel;

    public KernelService(IConfiguration configuration)
    {
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
