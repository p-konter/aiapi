using AIWebApi.Core;

namespace AIWebApi;

public static class Services
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IHttpService, HttpService>();
        services.AddSingleton<IJsonService, JsonService>();
        services.AddSingleton<IKernelService, KernelService>();
        services.AddSingleton<IQdrantService, QdrantService>();
        return services;
    }
}
