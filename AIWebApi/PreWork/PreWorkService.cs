using AIWebApi.Core;

namespace AIWebApi.PreWork;

public interface IPreWorkService
{
    Task<ResponseDto> RunPreWork();
}

public class PreWorkService(IConfiguration configuration, IHttpService httpService, ILogger<PreWorkService> logger) : IPreWorkService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IHttpService _httpService = httpService;
    private readonly ILogger<PreWorkService> _logger = logger;

    private const string TaskName = "POLIGON";
    private const string GetDataConfigName = "PreWorkData";
    private const string PostDataConfigName = "PreWorkPost";
    private const string ApiKeyConfigName = "ApiKey";

    private static readonly string[] separator = ["\r\n", "\n"];

    private async Task<IList<string>> FetchData()
    {
        string data = await _httpService.GetString(GetDataConfigName);
        return new List<string>(data.Split(separator, StringSplitOptions.RemoveEmptyEntries));
    }

    private async Task<ResponseDto> PostData(IList<string> values)
    {
        string apiKey = _configuration.GetStrictValue<string>(ApiKeyConfigName);
        RequestDto request = new(TaskName, apiKey, values);
        return await _httpService.PostJson<ResponseDto>(PostDataConfigName, request);
    }

    public async Task<ResponseDto> RunPreWork()
    {
        IList<string> values = await FetchData();

        _logger.LogInformation("Values {values}", values);

        return await PostData(values);
    }
}
