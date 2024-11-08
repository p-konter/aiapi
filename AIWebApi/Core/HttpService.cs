using System.Text;

namespace AIWebApi.Core;

public interface IHttpService
{
    Task<string> GetString(Uri url);

    Task<string> Post(Uri url, string request);

    Task<string> PostContent(Uri url, HttpContent content);

    Task<T> PostJson<T>(Uri url, object request);
}

public class HttpService(IJsonService jsonService) : IHttpService
{
    private readonly IJsonService _jsonService = jsonService;

    private static readonly HttpClient _httpClient = new();

    private const string JsonHeader = "application/json";

    public async Task<string> GetString(Uri url) => await _httpClient.GetStringAsync(url);

    public async Task<string> PostContent(Uri url, HttpContent content)
    {
        HttpResponseMessage response = await _httpClient.PostAsync(url, content);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> Post(Uri url, string request)
    {
        StringContent content = new(request, Encoding.UTF8, JsonHeader);
        return await PostContent(url, content);
    }

    public async Task<T> PostJson<T>(Uri url, object request)
    {
        string content = _jsonService.Serialize(request);
        string response = await Post(url, content);
        return _jsonService.Deserialize<T>(response);
    }
}
