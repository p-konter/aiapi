using AIWebApi.Core;

namespace AIWebApi;

public static class Services
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IAudioAIService, AudioAIService>();
        services.AddSingleton<IEmbeddingAIService, EmbeddingAIService>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IHttpService, HttpService>();
        services.AddSingleton<IImageAIService, ImageAIService>();
        services.AddSingleton<IJsonService, JsonService>();
        services.AddSingleton<IKernelService, KernelService>();

        services.AddSingleton<IGPT4AIService>(sp =>
        {
            IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
            IFileService fileService = sp.GetRequiredService<IFileService>();
            ILogger<ChatAIService> logger = sp.GetRequiredService<ILogger<ChatAIService>>();
            return new ChatAIService("gpt-4o", configuration, fileService, logger);
        });
        services.AddSingleton<IGPT4MiniAIService>(sp =>
        {
            IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
            IFileService fileService = sp.GetRequiredService<IFileService>();
            ILogger<ChatAIService> logger = sp.GetRequiredService<ILogger<ChatAIService>>();
            return new ChatAIService("gpt-4o-mini", configuration, fileService, logger);
        });
        services.AddSingleton<IQdrantService, QdrantService>();
        return services;
    }
}
