using AIWebApi.PreWork;

namespace AIWebApi;

public static class ApiEndpoints
{
    public static WebApplication RegisterEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => "Hello World!");
        app.MapGet("/preWork", ApiEndpoints.PreWork)
           .Produces<ResponseDto>()
           .WithDescription("PreWork api task")
           .WithTags("Api for AI_devs3");

        return app;
    }

    public static async Task<IResult> PreWork(IPreWorkService service)
    {
        ResponseDto response = await service.RunPreWork();
        return Results.Json(response);
    }
}
