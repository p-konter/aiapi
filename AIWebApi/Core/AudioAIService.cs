using OpenAI.Audio;

namespace AIWebApi.Core;

public interface IAudioAIService
{
    Task<string> AudioTranscription(string fileName);
}

public class AudioAIService(IConfiguration configuration, ILogger<AudioAIService> logger) : IAudioAIService
{
    private const string OpenAIApiKey = "OpenAIApiKey";
    private readonly AudioClient _client = new("whisper-1", configuration.GetStrictValue<string>(OpenAIApiKey));

    private readonly ILogger<AudioAIService> _logger = logger;

    public async Task<string> AudioTranscription(string fileName)
    {
        AudioTranscription transcription = await _client.TranscribeAudioAsync(fileName);

        _logger.LogInformation("Transcription language: {language}", transcription.Language);
        _logger.LogInformation("Transcription text: {text}", transcription.Text);

        return transcription.Text;
    }
}
