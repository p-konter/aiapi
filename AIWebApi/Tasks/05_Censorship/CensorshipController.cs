using AIWebApi.Core;

namespace AIWebApi.Tasks._05_Censorship;

public interface ICensorshipController
{
    Task<ResponseDto> RunCensorship();

    Task<ResponseDto> RunCensorshipLocal();
}

public class CensorshipController(IConfiguration configuration,
    IHttpService httpService,
    IKernelService kernelService,
    ILogger<CensorshipController> logger) : BaseController(configuration, httpService), ICensorshipController
{
    private readonly IKernelService _kernelService = kernelService;
    private readonly ILogger<CensorshipController> _logger = logger;

    public async Task<ResponseDto> RunCensorship()
    {
        string file = await GetFile();
        _logger.LogInformation("File content: {file}", file);

        string censored = await CensorText(file);
        _logger.LogInformation("AI response: {response}", censored);

        ResponseDto response = await SendAnswer("CENZURA", "Report", censored);
        _logger.LogInformation("System response: {response}", response.Message);

        return response;
    }

    public async Task<ResponseDto> RunCensorshipLocal()
    {
        string file = await GetFile();
        _logger.LogInformation("File content: {file}", file);

        // ToDo: use local model

        ResponseDto response = await SendAnswer("CENZURA", "Report", file);
        _logger.LogInformation("System response: {response}", response.Message);

        return response;
    }

    private async Task<string> GetFile()
    {
        string apiKey = _configuration.GetStrictValue<string>("ApiKey");
        Uri fileUrl = GetUrl("Censorship");
        Uri url = new($"{fileUrl.ToString().Replace("{key}", apiKey)}");
        return await _httpService.GetString(url);
    }

    private async Task<string> CensorText(string text)
    {
        MessageDto message = new(Role.User, text);
        MessageDto response = await _kernelService.Chat(AIModel.Gpt4o, [CreateSystemPrompt(), message]);

        return response.Message;
    }

    private static MessageDto CreateSystemPrompt()
    {
        string prompt = """
        <objective>
        You are a personal data protection assistant. You work in Polish.
        </objective>

        <rules>
        - You must write the text you receive. Do not add any other messages to it.
        - You need to change name, street, city and age to the word "CENZURA"
        - You need to change age number to the word "CENZURA"
        - Write the text. Do not add anything else.
        </rules>
        """;

        return new MessageDto(Role.System, prompt);
    }
}
