using System.Text.Json;

namespace AIWebApi.Core;

public interface IJsonService
{
    string Serialize(object data);

    T Deserialize<T>(string data);

    Task<T> LoadFromFile<T>(string filePath);
}

public class JsonService(ILogger<JsonService> logger) : IJsonService
{
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

    public async Task<T> LoadFromFile<T>(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        using FileStream fileStream = File.OpenRead(filePath);
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
