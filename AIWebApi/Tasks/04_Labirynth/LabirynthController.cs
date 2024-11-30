using AIWebApi.Core;

namespace AIWebApi.Tasks._04_Labirynth;

public interface ILabirynthController
{
    Task<string> WriteLabirynthPromptEasy();

    Task<string> WriteLabirynthPromptHard();
}

public class LabirynthController(IKernelService kernelService, ILogger<LabirynthController> logger) : ILabirynthController
{
    private readonly IKernelService _kernelService = kernelService;
    private readonly ILogger<LabirynthController> _logger = logger;

    public async Task<string> WriteLabirynthPromptEasy()
    {
        try
        {
            MessageDto messageDto = new(Role.User, Prompts.EasyPrompt);
            MessageDto response = await _kernelService.Chat(AIModel.Gpt4oMini, [messageDto]);
            _logger.LogInformation("Response message: {response}", response.Message);
            return response.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WriteLabirynthPromptEasy");
            throw;
        }
    }

    public async Task<string> WriteLabirynthPromptHard()
    {
        try
        {
            MessageDto messageDto = new(Role.User, Prompts.HardPrompt);
            MessageDto response = await _kernelService.Chat(AIModel.Gpt4o, [messageDto]);
            _logger.LogInformation("Response message: {response}", response.Message);
            return response.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WriteLabirynthPromptEasy");
            throw;
        }
    }
}
