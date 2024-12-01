using AIWebApi.Core;

namespace AIWebApi.Tasks._08_GenerateRobot;

public interface IGenerateRobotController
{
    Task<ResponseDto> RunRobotGeneration();
}

public class GenerateRobotController(IConfiguration configuration, IHttpService httpService, IKernelService kernelService, ILogger<GenerateRobotController> logger)
    : BaseController(configuration, httpService), IGenerateRobotController
{
    private readonly IKernelService _kernelService = kernelService;
    private readonly ILogger<GenerateRobotController> _logger = logger;

    public async Task<ResponseDto> RunRobotGeneration()
    {
        PromptData prompt = await GetPrompt();
        Uri robotUrl = await GenerateRobot(prompt.Description);
        return await SendAnswer("robotid", "Report", robotUrl.ToString());
    }

    private async Task<PromptData> GetPrompt()
    {
        Uri url = GetUrl("Robot");
        string apiKey = _configuration.GetStrictValue<string>("ApiKey");
        Uri robotUrl = new($"{url.ToString().Replace("{key}", apiKey)}");
        return await _httpService.GetJson<PromptData>(robotUrl);
    }

    private async Task<Uri> GenerateRobot(string description)
    {
        string prompt = $"Generate a robot using the description: {description}";
        _logger.LogInformation("Full prompt: {prompt}", prompt);
        return await _kernelService.GenerateImage(prompt);
    }
}

public record PromptData(string Description);
