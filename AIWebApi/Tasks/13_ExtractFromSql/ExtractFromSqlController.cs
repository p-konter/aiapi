using AIWebApi.Core;

namespace AIWebApi.Tasks._13_ExtractFromSql;

public interface IExtractFromSqlController
{
    Task<ResponseDto> ExtractFromSql();
}

public class ExtractFromSqlController(
    IConfiguration configuration,
    IHttpService httpService,
    IJsonService jsonService,
    IKernelService kernelService,
    ILoggerFactory loggerFactory)
    : BaseController(configuration, httpService), IExtractFromSqlController
{
    private readonly IJsonService _jsonService = jsonService;
    private readonly IKernelService _kernelService = kernelService;
    private readonly ILogger<ExtractFromSqlController> _logger = loggerFactory.CreateLogger<ExtractFromSqlController>();

    private readonly string ApiKey = configuration.GetStrictValue<string>("ApiKey");

    public async Task<ResponseDto> ExtractFromSql()
    {
        IList<string> tableNames = await GetTableNames();
        IList<string> structure = await GetStructure(tableNames);
        IList<int> data = await GetDatacenters(structure);

        return await SendAnswer("database", "Report", data);
    }

    private async Task<IList<string>> GetTableNames()
    {
        Dictionary<string, string> tables = [];
        ApiResponseDto response = await SendQuery("SHOW TABLES");
        return response.Reply.SelectMany(dict => dict.Values).ToList();
    }

    private async Task<IList<string>> GetStructure(IList<string> tableNames)
    {
        IList<string> structure = [];
        foreach (string table in tableNames)
        {
            ApiResponseDto response = await SendQuery($"SHOW CREATE TABLE {table}");
            string createTable = response.Reply.SelectMany(dict => dict.Where(kvp => kvp.Key == "Create Table").Select(x => x.Value)).First();
            _logger.LogInformation("Create table for {table} is: {createTable}", table, createTable);
            structure.Add(createTable);
        }

        return structure;
    }

    private async Task<IList<int>> GetDatacenters(IList<string> structure)
    {
        string prompt = """
            You are an expert in SQL. 
            You have structure of tables in sql database. Write a sql statement that will solve the problem.
            Write your solution in the given JSON format
            
            <problem>
            which active datacenters (DC_ID) are managed by employees who are on vacation (is_active=0).
            </problem>

            <answer_format>
            {
                "thinking": "write your thinking here",
                "sql": "write your sql here"
            }
            </answer_format>

            """;
        string message = $"{prompt}<structure>{string.Join('\n', structure)}\n</structure>";

        MessageDto answer = await _kernelService.Chat(AIModel.Gpt4o, [new(Role.User, message)], returnJson: true);
        _logger.LogInformation("Sql query: {sql}", answer.Message);

        OutputMessageDto outputMessage = _jsonService.Deserialize<OutputMessageDto>(answer.Message);

        ApiResponseDto response = await SendQuery(outputMessage.Sql);
        return response.Reply.SelectMany(dict => dict.Values).Select(int.Parse).ToList();
    }

    private async Task<ApiResponseDto> SendQuery(string query)
    {
        ApiRequestDto request = new("database", ApiKey, query);
        ApiResponseDto response = await _httpService.PostJson<ApiResponseDto>(GetUrl("ApiDB"), request);

        _logger.LogInformation("Error: {error}", response.Error);
        return response;
    }
}

public record OutputMessageDto(string Thinking, string Sql);

public record ApiRequestDto(string Task, string Apikey, string Query);

public record ApiResponseDto(string Error, IList<IDictionary<string, string>> Reply);
