namespace AIWebApi.Core;

public class BaseController(IConfiguration configuration, IHttpService httpService)
{
    protected readonly IConfiguration _configuration = configuration;
    protected readonly IHttpService _httpService = httpService;

    protected const string ApiKeyConfigName = "ApiKey";

    protected Uri GetUrl(string key) => new(_configuration.GetSection("Urls").GetStrictValue<string>(key));

    protected async Task<ResponseDto> SendAnswer<T>(string taskName, string urlKey, T answer)
    {
        string apiKey = _configuration.GetStrictValue<string>(ApiKeyConfigName);
        Uri sendAnswerUrl = GetUrl(urlKey);

        RequestDto<T> request = new(taskName, apiKey, answer);
        return await _httpService.PostJson<ResponseDto>(sendAnswerUrl, request);
    }
}
