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
        string textFileName = Path.ChangeExtension(fileName, ".txt");
        string textFilePath = Path.Combine("ExternalData", textFileName);
        if (File.Exists(textFilePath))
        {
            return await File.ReadAllTextAsync(textFilePath);
        }

        string audioFilePath = Path.Combine("ExternalData", fileName);
        if (!File.Exists(audioFilePath))
        {
            throw new FileNotFoundException("Audio file not found", audioFilePath);
        }

        AudioTranscription transcription = await _client.TranscribeAudioAsync(audioFilePath);

        _logger.LogInformation("Transcription language: {language}", transcription.Language);
        _logger.LogInformation("Transcription text: {text}", transcription.Text);

        await File.WriteAllTextAsync(textFilePath, transcription.Text);
        return transcription.Text;
    }
}
