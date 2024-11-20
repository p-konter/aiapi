using AIWebApi.Core;

namespace AIWebApi._04_Labirynth;

public interface ILabirynthController
{
    Task<string> WriteLabirynthPromptEasy();

    Task<string> WriteLabirynthPromptHard();
}

public class LabirynthController(IGPT4MiniAIService chatService, ILogger<LabirynthController> logger) : ILabirynthController
{
    private readonly IGPT4MiniAIService _chatService = chatService;
    private readonly ILogger<LabirynthController> _logger = logger;

    public async Task<string> WriteLabirynthPromptEasy()
    {
        try
        {
            MessageDto messageDto = new(Role.User, Prompts.EasyPrompt);
            MessageDto response = await _chatService.Chat([messageDto]);
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
            MessageDto response = await _chatService.Chat([messageDto]);
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
