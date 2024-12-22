using AIWebApi.Core;

using Neo4j.Driver;

namespace AIWebApi.Tasks._15_FindPath;

public interface IFindPathController
{
    Task<ResponseDto> FindPath();
}

public class FindPathController(IConfiguration configuration, IHttpService httpService, ILogger<FindPathController> logger) : BaseController(configuration, httpService), IFindPathController
{
    private readonly ILogger<FindPathController> _logger = logger;
    private readonly IDriver _driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", configuration.GetStrictValue<string>("Neo4jPassword")));

    public async Task<ResponseDto> FindPath()
    {
        await _driver.VerifyConnectivityAsync();

        List<DatabaseUser> users = await GetUsers();
        List<DatabaseConnection> connections = await GetConnections();
        await AddUsers(users);
        await AddConnections(connections);

        List<string> names = await FindShortestPath("Rafał", "Barbara");
        string path = string.Join(", ", names);
        _logger.LogInformation("Shortest path: {path}", path);

        await ClearDatabase();
        return await SendAnswer("connections", "Report", path);
    }

    private async Task<List<DatabaseUser>> GetUsers()
    {
        SqlResponseDto response = await SendQuery($"SELECT id, username FROM users");
        return response.Reply
            .Select(repl => new DatabaseUser(int.Parse(repl["id"]), repl["username"]))
            .ToList();
    }

    private async Task<List<DatabaseConnection>> GetConnections()
    {
        SqlResponseDto response = await SendQuery($"SELECT user1_id, user2_id FROM connections");
        return response.Reply
            .Select(repl => new DatabaseConnection(int.Parse(repl["user1_id"]), int.Parse(repl["user2_id"])))
            .ToList();
    }

    private async Task AddUsers(List<DatabaseUser> users)
    {
        using IAsyncSession session = _driver.AsyncSession();

        await session.ExecuteWriteAsync(async tx =>
        {
            foreach (DatabaseUser user in users)
            {
                string query = @"
                    MERGE (u:User {id: $id})
                    SET u.name = $name";
                await tx.RunAsync(query, new { id = user.Id, name = user.Username });
            }
        });
    }

    private async Task AddConnections(List<DatabaseConnection> connections)
    {
        using IAsyncSession session = _driver.AsyncSession();

        await session.ExecuteWriteAsync(async tx =>
        {
            foreach (DatabaseConnection connection in connections)
            {
                string query = @"
                    MATCH (u1:User {id: $user1Id}), (u2:User {id: $user2Id})
                    MERGE (u1)-[:CONNECTED_TO]->(u2)";
                await tx.RunAsync(query, new { user1Id = connection.Id1, user2Id = connection.Id2 });
            }
        });
    }

    private async Task<List<string>> FindShortestPath(string user1Name, string user2Name)
    {
        using IAsyncSession session = _driver.AsyncSession();

        return await session.ExecuteReadAsync(async tx =>
        {
            string query = @"
            MATCH p = shortestPath((u1:User {name: $user1Name})-[:CONNECTED_TO*]-(u2:User {name: $user2Name}))
            RETURN [node IN nodes(p) | node.name] as names";

            IResultCursor result = await tx.RunAsync(query, new { user1Name, user2Name });
            IRecord node = await result.SingleAsync();

            return node != null ? node["names"].As<List<string>>() : [string.Empty];
        });
    }

    private async Task ClearDatabase()
    {
        using IAsyncSession session = _driver.AsyncSession();

        await session.ExecuteWriteAsync(async tx =>
        {
            string query = "MATCH (n) DETACH DELETE n";
            await tx.RunAsync(query);
        });
    }
}

public record DatabaseUser(int Id, string Username);

public record DatabaseConnection(int Id1, int Id2);
