namespace AIWebApi.Core;

public abstract class BaseController(IConfiguration configuration, IHttpService httpService)
{
    protected readonly IConfiguration _configuration = configuration;
    protected readonly IHttpService _httpService = httpService;

    private const string ApiKeyConfigName = "ApiKey";

    protected async Task<ResponseDto> SendAnswer<T>(string taskName, string urlKey, T answer)
    {
        string apiKey = _configuration.GetStrictValue<string>(ApiKeyConfigName);
        Uri sendAnswerUrl = new(_configuration.GetSection("Urls").GetStrictValue<string>(urlKey));

        RequestDto<T> request = new(taskName, apiKey, answer);
        return await _httpService.PostJson<ResponseDto>(sendAnswerUrl, request);
    }
}
