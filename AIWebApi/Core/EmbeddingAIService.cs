using System.Text;

using OpenAI.Embeddings;

namespace AIWebApi.Core;

public interface IEmbeddingAIService
{
    Task<float[]> CreateVector(string description);
}

public class EmbeddingAIService(IConfiguration configuration, ILogger<AudioAIService> logger) : IEmbeddingAIService
{
    private const string OpenAIApiKey = "OpenAIApiKey";
    private readonly EmbeddingClient _client = new("text-embedding-3-small", configuration.GetStrictValue<string>(OpenAIApiKey));

    private readonly ILogger<AudioAIService> _logger = logger;

    public async Task<float[]> CreateVector(string description)
    {
        OpenAIEmbedding embedding = await _client.GenerateEmbeddingAsync(description);
        ReadOnlyMemory<float> vector = embedding.ToFloats();
        _logger.LogInformation("Create vector: dimension: {dimension}", vector.Length);

        float[] vectorArray = vector.Span.ToArray();

        StringBuilder sp = new();
        sp.Append(vectorArray.Select((value, index) => $"  [{index,4}] = {value}"));
        _logger.LogInformation("Floats: {floats}", sp.ToString());

        return vectorArray;
    }
}
