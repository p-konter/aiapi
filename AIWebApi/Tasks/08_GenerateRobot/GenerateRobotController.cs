using AIWebApi.Core;

namespace AIWebApi.Tasks._08_GenerateRobot;

public interface IGenerateRobotController
{
    Task<ResponseDto> RunRobotGeneration();
}

public class GenerateRobotController(IConfiguration configuration, IHttpService httpService, IImageAIService imageAIService, ILogger<GenerateRobotController> logger)
    : IGenerateRobotController
{
    private readonly IHttpService _httpService = httpService;
    private readonly IImageAIService _imageAIService = imageAIService;
    private readonly ILogger<GenerateRobotController> _logger = logger;

    private const string FileUrl = "https://centrala.ag3nts.org/data/{key}/robotid.json";
    private readonly Uri PostDataUrl = new("https://centrala.ag3nts.org/report");
    private readonly string ApiKey = configuration.GetStrictValue<string>("ApiKey");

    public async Task<ResponseDto> RunRobotGeneration()
    {
        PromptData prompt = await GetPrompt();
        Uri robotUrl = await GenerateRobot(prompt.Description);
        return await SendResponse(robotUrl);
    }

    private async Task<PromptData> GetPrompt()
    {
        Uri url = new($"{FileUrl.Replace("{key}", ApiKey)}");
        return await _httpService.GetJson<PromptData>(url);
    }

    private async Task<Uri> GenerateRobot(string description)
    {
        string prompt = $"Generate a robot using the description: {description}";
        _logger.LogInformation("Full prompt: {prompt}", prompt);
        return await _imageAIService.GenerateImage(prompt);
    }

    private async Task<ResponseDto> SendResponse(Uri url)
    {
        RequestDto request = new("robotid", ApiKey, url.ToString());
        return await _httpService.PostJson<ResponseDto>(PostDataUrl, request);
    }
}

public record PromptData(string Description);
