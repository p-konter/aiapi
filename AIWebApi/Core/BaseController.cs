using HtmlAgilityPack;

namespace AIWebApi.Core;

public class BaseController(IConfiguration configuration, IHttpService httpService)
{
    protected readonly IConfiguration _configuration = configuration;
    protected readonly IHttpService _httpService = httpService;

    protected const string ApiKeyConfigName = "ApiKey";
    protected readonly string ApiKey = configuration.GetStrictValue<string>(ApiKeyConfigName);

    protected Uri GetUrl(string key) => new(_configuration.GetSection("Urls").GetStrictValue<string>(key));

    protected async Task<ResponseDto> SendAnswer<T>(string taskName, string urlKey, T answer)
    {
        Uri sendAnswerUrl = GetUrl(urlKey);
        RequestDto<T> request = new(taskName, ApiKey, answer);
        return await _httpService.PostJson<ResponseDto>(sendAnswerUrl, request);
    }

    protected Uri GetUrlWithKey(string keyName)
    {
        Uri url = GetUrl(keyName);
        return new($"{url.ToString().Replace("{key}", ApiKey)}");
    }

    protected static HtmlDocument CreateHtmlDocument(string form)
    {
        HtmlDocument document = new();
        document.LoadHtml(form);
        return document;
    }

    protected async Task<SqlResponseDto> SendQuery(string query)
    {
        SqlRequestDto request = new("database", ApiKey, query);
        return await _httpService.PostJson<SqlResponseDto>(GetUrl("ApiDB"), request);
    }
}
