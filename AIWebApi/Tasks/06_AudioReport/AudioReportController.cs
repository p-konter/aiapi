using AIWebApi.Core;

namespace AIWebApi.Tasks._06_AudioReport;

public interface IAudioReportController
{
    Task<ResponseDto> RunAudioRepost();
}

public class AudioReportController(
    IAudioAIService audioAIService,
    IGPT4AIService chatService,
    IConfiguration configuration,
    IHttpService httpService) : IAudioReportController
{
    private readonly string AudioPath = "ExternalData";
    private readonly List<string> FileNames = ["adam.m4a", "agnieszka.m4a", "ardian.m4a", "michal.m4a", "monika.m4a", "rafal.m4a"];

    private readonly IAudioAIService _audioAIService = audioAIService;
    private readonly IGPT4AIService _chatService = chatService;
    private readonly IHttpService _httpService = httpService;

    private readonly Uri PostDataUrl = new("https://centrala.ag3nts.org/report");
    private readonly string ApiKey = configuration.GetStrictValue<string>("ApiKey");

    public async Task<ResponseDto> RunAudioRepost()
    {
        IList<string> transcriptions = await TranscriptFiles();

        string street = await FindStreet(transcriptions);

        return await PostData(street);
    }

    private async Task<IList<string>> TranscriptFiles()
    {
        _audioAIService.SetFolder(AudioPath);
        List<string> transcriptions = [];
        foreach (string fileName in FileNames)
        {
            transcriptions.Add(await _audioAIService.AudioTranscription(fileName));
        }

        return transcriptions;
    }

    private async Task<string> FindStreet(IList<string> texts)
    {
        List<MessageDto> messages = [CreateSystemPrompt()];
        foreach (string text in texts)
        {
            messages.Add(new MessageDto(Role.User, text));
        }

        MessageDto response = await _chatService.Chat(messages);
        return response.Message;
    }

    private static MessageDto CreateSystemPrompt()
    {
        string prompt = """
        <objective>
        You are a detective. You are conducting an investigation. You have to find a missing person, his name is Andrzej Maj.
        </objective>
        
        <rules>
        - Read the user messages, these are witness statements.
        - Think and answer, based on the information, where you can find Andrzej Maj.
        - Find the address of this place, pay attention to the details, maybe it's a department.
        - Write the street name. Do not write anything else.
        </rules>    
        """;
        return new MessageDto(Role.System, prompt);
    }

    private async Task<ResponseDto> PostData(string value)
    {
        RequestDto request = new("MP3", ApiKey, value);
        return await _httpService.PostJson<ResponseDto>(PostDataUrl, request);
    }
}
