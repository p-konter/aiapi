using AIWebApi.Core;

namespace AIWebApi.Tasks._14_FindLocation;

public interface IFindLocationController
{
    Task<ResponseDto> FindLocation();
}

public class FindLocationController(
    IConfiguration configuration,
    IFileService fileService,
    IHttpService httpService,
    IJsonService jsonService,
    IKernelService kernelService,
    ILogger<FindLocationController> logger) : BaseController(configuration, httpService), IFindLocationController
{
    private readonly IFileService _fileService = fileService;
    private readonly IJsonService _jsonService = jsonService;
    private readonly IKernelService _kernelService = kernelService;
    private readonly ILogger<FindLocationController> _logger = logger;

    public async Task<ResponseDto> FindLocation()
    {
        string note = await GetNote();
        DataDto data = await GetDataFromNote(note);
        Sources sources = ConvertDataDtoToSources(data);

        string? place = await CheckValues(sources);
        _logger.LogInformation("Place: {place}", place);

        return await SendAnswer("loop", "Report", place);
    }

    private async Task<string> GetNote()
    {
        _fileService.SetFolder("ExternalData");
        string note = await _fileService.ReadTextFile("barbara.txt") ?? throw new Exception("Wrong file");

        _logger.LogInformation("Text: {text}", note);
        return note;
    }

    private async Task<DataDto> GetDataFromNote(string note)
    {
        string prompt = """
            You are a skilled detective. Your task is to find information from the given note.

            <rules>
            - Find a list of names and places given in the note.
            - We are only interested in the first name, ignore the last name.
            - Write it as a json containing a list of names and places.
            - Write in polish ie. WARSZAWA not WARSAW.
            - Write all words in capital letters.
            - Write only names and places in the given format. Do not write anything else.
            </rules>

            <example_output>
            {
                "places": ["POZNAN", "WROCLAW"],
                "people": ["PAWEL", "JAN"]
            }
            </example_output>
            """;

        List<MessageDto> messages = [new(Role.System, prompt), new(Role.User, note)];

        MessageDto response = await _kernelService.Chat(AIModel.Gpt4oMini, messages, true);
        _kernelService.ClearHistory();

        string output = Utils.RemovePolishCharacters(response.Message);
        return _jsonService.Deserialize<DataDto>(output);
    }

    private static Sources ConvertDataDtoToSources(DataDto data)
    {
        Dictionary<string, bool> places = data.Places.ToDictionary(place => place, place => false);
        Dictionary<string, bool> people = data.People.ToDictionary(person => person, person => false);
        return new Sources(places, people);
    }

    private async Task<string?> CheckValues(Sources source)
    {
        while (true)
        {
            foreach ((string key, bool value) in source.Places.Where(x => x.Value == false))
            {
                List<string> people = await GetDataFromApi("Places", key);
                foreach (string person in people.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    if (person == "BARBARA" && key != "KRAKOW")
                    {
                        return key;
                    }
                    source.People.TryAdd(person, false);
                }
                source.Places[key] = true;
            }

            foreach ((string key, bool value) in source.People.Where(x => x.Value == false))
            {
                List<string> places = await GetDataFromApi("People", key);
                foreach (string place in places.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    source.Places.TryAdd(place, false);
                }
                source.People[key] = true;
            }
        }
    }

    private async Task<List<string>> GetDataFromApi(string urlKey, string value)
    {
        string apiKey = _configuration.GetStrictValue<string>(ApiKeyConfigName);
        ApiRequest apiRequest = new(apiKey, value);

        Uri url = GetUrl(urlKey);
        ResponseDto response = await _httpService.PostJson<ResponseDto>(url, apiRequest);

        string message = Utils.RemovePolishCharacters(response.Message.ToUpper());
        _logger.LogInformation("Response: {urlkey}, {value}, {response}", urlKey, value, message);

        if (message == "[**RESTRICTED DATA**]" || message.StartsWith("NO DATA FOUND"))
        {
            message = string.Empty;
        }
        return [.. message.Split(" ")];
    }
}

public record ApiRequest(string Apikey, string Query);

public record DataDto(List<string> Places, List<string> People);

public record Sources(Dictionary<string, bool> Places, Dictionary<string, bool> People);
