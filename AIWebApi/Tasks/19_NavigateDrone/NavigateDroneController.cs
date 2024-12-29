using AIWebApi.Core;

namespace AIWebApi.Tasks._19_NavigateDrone;

public interface INavigateDroneController
{
    Task<ResponseDto> StartNavigate();

    Task<FlightResponse> Flight(FlightRequest data);
}

public class NavigateDroneController(
    IConfiguration configuration,
    IHttpService httpService,
    IJsonService jsonService,
    IKernelService kernelService,
    ILogger<NavigateDroneController> logger)
    : BaseController(configuration, httpService), INavigateDroneController
{
    private readonly IJsonService _jsonService = jsonService;
    private readonly IKernelService _kernelService = kernelService;
    private readonly ILogger<NavigateDroneController> _logger = logger;

    public async Task<ResponseDto> StartNavigate() => await SendAnswer("webhook", "Report", "https://prawn-rapid-goose.ngrok-free.app/flight/");

    public async Task<FlightResponse> Flight(FlightRequest data)
    {
        _logger.LogInformation("Flight data: {data}", data.Instruction);
        List<MessageDto> messages = [new(Role.System, Prompt), new(Role.User, data.Instruction)];
        MessageDto response = await _kernelService.Chat(AIModel.Gpt4o, messages, returnJson: true);
        AnswerDto answer = _jsonService.Deserialize<AnswerDto>(response.Message);
        return new FlightResponse(answer.Answer);
    }

    private const string Prompt = """
        You are the drone navigator. Guide the drone according to the map you have below.

        <map>
        (0,0) start, (1,0) trawa,   (2,0) drzewo,   (3,0) dom
        (0,1) trawa, (1,1) wiatrak, (2,1) trawa,    (3,1) trawa
        (0,2) trawa, (1,2) trawa,   (2,2) skały,    (3,2) dwa drzewa
        (0,3) góry,  (1,3) góry,    (2,3) samochód, (3,3) jaskinia
        </map>

        <map_information>
        - the map is 4x4
        - the x-axis runs from left (0) to right (3)
        - the y-axis runs from top (0) to bottom (3)
        </map_information>

        <rules>
        - you always start at position (0,0)
        - analyze the instructions given
        - calculate the coordinates of the position where the drone will be
        - write your thinking and name of the final field as json object
        </rules>

        <answer>
        {
            "thinking": "write your thinking",
            "answer": "write only the name of the field"
        }
        </answer>
        """;
}

public record AnswerDto(string Thinking, string Answer);
