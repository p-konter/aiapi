using OpenAI.Images;

namespace AIWebApi.Core;

public interface IImageAIService
{
    Task<Uri> GenerateImage(string prompt);
}

public class ImageAIService(IConfiguration configuration, ILogger<ImageAIService> logger) : IImageAIService
{
    private const string OpenAIApiKey = "OpenAIApiKey";
    private readonly ImageClient _client = new("dall-e-3", configuration.GetStrictValue<string>(OpenAIApiKey));
    private readonly ILogger<ImageAIService> _logger = logger;

    public async Task<Uri> GenerateImage(string prompt)
    {
        ImageGenerationOptions options = new()
        {
            Quality = GeneratedImageQuality.High,
            Size = GeneratedImageSize.W1024xH1024,
            Style = GeneratedImageStyle.Natural,
            ResponseFormat = GeneratedImageFormat.Uri
        };

        GeneratedImage image = await _client.GenerateImageAsync(prompt, options);
        _logger.LogInformation("Generated image: {image}", image.ImageUri);

        return image.ImageUri;
    }
}
