using System.Text;

namespace AIWebApi.Core;

public interface IHttpService
{
    Task<string> GetString(string configUrlName);

    Task<string> Post(string configUrlName, string request);

    Task<T> PostJson<T>(string configUrlName, object request);
}

public class HttpService(IConfiguration configuration, IJsonService jsonService) : IHttpService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IJsonService _jsonService = jsonService;

    private static readonly HttpClient _httpClient = new();

    private const string ConfigUrlsSectionName = "Urls";
    private const string JsonHeader = "application/json";

    public async Task<string> GetString(string configUrlName)
    {
        Uri uri = GetUri(configUrlName);
        return await _httpClient.GetStringAsync(uri);
    }

    public async Task<string> Post(string configUrlName, string request)
    {
        Uri uri = GetUri(configUrlName);
        StringContent content = new(request, Encoding.UTF8, JsonHeader);

        HttpResponseMessage response = await _httpClient.PostAsync(uri, content);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<T> PostJson<T>(string configUrlName, object request)
    {
        string content = _jsonService.Serialize(request);
        string response = await Post(configUrlName, content);
        return _jsonService.Deserialize<T>(response);
    }

    private Uri GetUri(string configUrlName) => new(_configuration.GetSection(ConfigUrlsSectionName).GetStrictValue<string>(configUrlName));
}
