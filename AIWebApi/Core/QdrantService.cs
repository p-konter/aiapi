using System.Data;

using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace AIWebApi.Core;

public interface IQdrantService
{
    Task CreateCollectionAsync();

    Task<UpdateResult> AddPointsAsync(List<DataPointDto> data);

    Task<IList<SearchResultDto>> SearchAsync(ReadOnlyMemory<float> vector, ulong limit = 1);
}

public class QdrantService : IQdrantService
{
    private readonly QdrantClient Client = new("localhost");
    private const string CollectionName = "date_from_vector";
    private const ulong CollectionSize = 1536;

    public async Task CreateCollectionAsync()
    {
        IReadOnlyList<string> collections = await Client.ListCollectionsAsync();
        if (!collections.Contains(CollectionName))
        {
            await Client.CreateCollectionAsync(CollectionName, new VectorParams
            {
                Size = CollectionSize,
                Distance = Distance.Cosine
            });
        }
    }

    public async Task<UpdateResult> AddPointsAsync(List<DataPointDto> data)
    {
        List<PointStruct> points = data
            .Select((item, index) =>
            {
                PointStruct point = new()
                {
                    Id = (ulong)index,
                    Vectors = item.Vectors,
                };

                foreach ((string key, string value) in item.Payload)
                {
                    point.Payload.Add(key, value);
                }

                return point;
            })
            .ToList();

        return await Client.UpsertAsync(CollectionName, points);
    }

    public async Task<IList<SearchResultDto>> SearchAsync(ReadOnlyMemory<float> vector, ulong limit = 1)
    {
        IReadOnlyList<ScoredPoint> results = await Client.SearchAsync(CollectionName, vector, limit: limit);

        List<SearchResultDto> response = results.Select(result =>
        {
            Dictionary<string, string> payload = result.Payload?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.StringValue) ?? [];
            return new SearchResultDto(result.Score, (long)result.Id.Num, payload);
        }).ToList();

        return response;
    }
}

public record DataPointDto(float[] Vectors, IDictionary<string, string> Payload);

public record SearchResultDto(float Score, long Id, IDictionary<string, string> Data);
