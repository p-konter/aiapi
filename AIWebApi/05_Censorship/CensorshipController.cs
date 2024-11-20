using AIWebApi.Core;

namespace AIWebApi._05_Censorship;

public interface ICensorshipController
{
    Task<ResponseDto> RunCensorship();

    Task<ResponseDto> RunCensorshipLocal();
}

public class CensorshipController(IGPT4AIService chatService, IConfiguration configuration, IHttpService httpService, ILogger<CensorshipController> logger)
    : ICensorshipController
{
    private readonly IGPT4AIService _chatService = chatService;
    private readonly IHttpService _httpService = httpService;
    private readonly ILogger<CensorshipController> _logger = logger;

    private const string FileUrl = "https://centrala.ag3nts.org/data/{key}/cenzura.txt";
    private readonly Uri PostDataUrl = new("https://centrala.ag3nts.org/report");
    private readonly string ApiKey = configuration.GetStrictValue<string>("ApiKey");

    public async Task<ResponseDto> RunCensorship()
    {
        string file = await GetFile();
        _logger.LogInformation("File content: {file}", file);

        string censored = await CensorText(file);
        _logger.LogInformation("AI response: {response}", censored);

        ResponseDto response = await SendResponse(censored);
        _logger.LogInformation("System response: {response}", response.Message);

        return response;
    }

    public async Task<ResponseDto> RunCensorshipLocal()
    {
        string file = await GetFile();
        _logger.LogInformation("File content: {file}", file);

        // ToDo: use local model

        ResponseDto response = await SendResponse(file);
        _logger.LogInformation("System response: {response}", response.Message);

        return response;
    }

    private async Task<string> GetFile()
    {
        Uri url = new($"{FileUrl.Replace("{key}", ApiKey)}");
        return await _httpService.GetString(url);
    }

    private async Task<string> CensorText(string text)
    {
        MessageDto message = new(Role.User, text);
        MessageDto response = await _chatService.Chat([CreateSystemPrompt(), message]);

        return response.Message;
    }

    private async Task<ResponseDto> SendResponse(string text)
    {
        RequestDto request = new("CENZURA", ApiKey, text);
        return await _httpService.PostJson<ResponseDto>(PostDataUrl, request);
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
