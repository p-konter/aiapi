using System.Text.Json;

namespace AIWebApi.Core;

public interface IJsonService
{
    string Serialize(object data);

    T Deserialize<T>(string data);
}

public class JsonService : IJsonService
{
    public string Serialize(object data) => JsonSerializer.Serialize(data, GetJsonOptions());

    public T Deserialize<T>(string data)
    {
        T? obj = JsonSerializer.Deserialize<T>(data, GetJsonOptions());
        return obj is null ? throw new EmptyJsonException() : obj;
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
}
