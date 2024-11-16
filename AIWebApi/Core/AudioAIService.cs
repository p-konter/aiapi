using OpenAI.Audio;

namespace AIWebApi.Core;

public interface IAudioAIService
{
    Task<string> AudioTranscription(string fileName);
}

public class AudioAIService(IConfiguration configuration, IFileService fileService, ILogger<AudioAIService> logger) : IAudioAIService
{
    private const string OpenAIApiKey = "OpenAIApiKey";
    private readonly AudioClient _client = new("whisper-1", configuration.GetStrictValue<string>(OpenAIApiKey));

    private readonly IFileService _fileService = fileService;
    private readonly ILogger<AudioAIService> _logger = logger;

    public async Task<string> AudioTranscription(string fileName)
    {
        string textFileName = _fileService.ChangeExtension(fileName, ".txt");

        string? textFile = await _fileService.ReadTextFile(textFileName);
        if (textFile is not null)
        {
            return textFile;
        }

        string filePath = _fileService.CheckFileExists(fileName);
        AudioTranscription transcription = await _client.TranscribeAudioAsync(filePath);

        _logger.LogInformation("Transcription language: {language}", transcription.Language);
        _logger.LogInformation("Transcription text: {text}", transcription.Text);

        await _fileService.WriteTextFile(textFileName, transcription.Text);
        return transcription.Text;
    }
}
