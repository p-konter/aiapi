using System.Text.Json;

namespace AIWebApi.Core;

public interface IJsonService
{
    string Serialize(object data);

    T Deserialize<T>(string data);

    Task<T> LoadFromFile<T>(string filePath);
}

public class JsonService(IFileService fileService, ILogger<JsonService> logger) : IJsonService
{
    private readonly IFileService _fileService = fileService;
    private readonly ILogger<JsonService> _logger = logger;

    public string Serialize(object data)
    {
        try
        {
            return JsonSerializer.Serialize(data, GetJsonOptions());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to serialize JSON: {data}", data);
            throw;
        }
    }

    public T Deserialize<T>(string data)
    {
        try
        {
            T? obj = JsonSerializer.Deserialize<T>(data, GetJsonOptions());
            return obj is null ? throw new EmptyJsonException() : obj;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize JSON: {data}", data);
            throw;
        }
    }

    public async Task<T> LoadFromFile<T>(string fileName)
    {
        FileStream fileStream = _fileService.ReadStream(fileName);
        T? result = await JsonSerializer.DeserializeAsync<T>(fileStream, GetJsonOptions());
        return result ?? throw new EmptyJsonException();
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
}
