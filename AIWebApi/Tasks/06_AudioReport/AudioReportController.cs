using AIWebApi.Core;

namespace AIWebApi.Tasks._06_AudioReport;

public interface IAudioReportController
{
    Task<ResponseDto> RunAudioRepost();
}

public class AudioReportController(IConfiguration configuration, IFileService fileService, IHttpService httpService, IKernelService kernelService)
    : BaseController(configuration, httpService), IAudioReportController
{
    private readonly string Path = "ExternalData";

    private readonly IFileService _fileService = fileService;
    private readonly IKernelService _kernelService = kernelService;

    public async Task<ResponseDto> RunAudioRepost()
    {
        IList<string> transcriptions = await TranscriptFiles();

        string street = await FindStreet(transcriptions);

        return await SendAnswer("MP3", "Report", street);
    }

    private async Task<IList<string>> TranscriptFiles()
    {
        _fileService.SetFolder(Path);
        IEnumerable<string> files = _fileService.GetFileNames();

        List<string> transcriptions = [];
        foreach (string file in files)
        {
            if (_fileService.GetFileType(file) == "m4a")
            {
                transcriptions.Add(await _kernelService.AudioTranscription(file));
            }
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

        MessageDto response = await _kernelService.Chat(AIModel.Gpt4o, messages);
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
}
