using OpenAI.Audio;

namespace AIWebApi.Core;

public interface IAudioAIService
{
    void SetFolder(string folder);

    Task<string> AudioTranscription(string fileName);
}

public class AudioAIService(IConfiguration configuration, IFileService fileService, ILogger<AudioAIService> logger)
    : BaseFileAIService(fileService), IAudioAIService
{
    private const string OpenAIApiKey = "OpenAIApiKey";
    private readonly AudioClient _client = new("whisper-1", configuration.GetStrictValue<string>(OpenAIApiKey));

    private readonly ILogger<AudioAIService> _logger = logger;

    public async Task<string> AudioTranscription(string fileName)
    {
        string? data = await LoadProcessedData(fileName);
        if (data is not null)
        {
            return data;
        }
        _logger.LogInformation("Path: {text}", base._fileService.GetFolder());
        string filePath = ReturnFilePath(fileName);
        AudioTranscription transcription = await _client.TranscribeAudioAsync(filePath);

        _logger.LogInformation("Transcription language: {language}", transcription.Language);
        _logger.LogInformation("Transcription text: {text}", transcription.Text);

        await SaveProcessedData(fileName, transcription.Text);
        return transcription.Text;
    }
}
