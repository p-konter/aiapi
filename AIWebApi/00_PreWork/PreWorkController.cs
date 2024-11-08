using AIWebApi.Core;

namespace AIWebApi._00_PreWork;

public interface IPreWorkController
{
    Task<ResponseDto> RunPreWork();
}

public class PreWorkController(IConfiguration configuration, IHttpService httpService, ILogger<PreWorkController> logger) : IPreWorkController
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IHttpService _httpService = httpService;
    private readonly ILogger<PreWorkController> _logger = logger;

    private readonly Uri GetDataUrl = new("https://poligon.aidevs.pl/dane.txt");
    private readonly Uri PostDataUrl = new("https://poligon.aidevs.pl/verify");

    private const string TaskName = "POLIGON";
    private const string ApiKeyConfigName = "ApiKey";

    private static readonly string[] separator = ["\r\n", "\n"];

    private async Task<IList<string>> FetchData()
    {
        string data = await _httpService.GetString(GetDataUrl);
        return new List<string>(data.Split(separator, StringSplitOptions.RemoveEmptyEntries));
    }

    private async Task<ResponseDto> PostData(IList<string> values)
    {
        string apiKey = _configuration.GetStrictValue<string>(ApiKeyConfigName);
        RequestDto request = new(TaskName, apiKey, values);
        return await _httpService.PostJson<ResponseDto>(PostDataUrl, request);
    }

    public async Task<ResponseDto> RunPreWork()
    {
        IList<string> values = await FetchData();
        _logger.LogInformation("Fetched values are: {values}", values);

        ResponseDto answer = await PostData(values);
        _logger.LogInformation("Final answer is {answer}", answer.Message);

        return answer;
    }
}
