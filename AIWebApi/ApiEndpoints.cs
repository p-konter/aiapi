using AIWebApi._00_PreWork;
using AIWebApi._01_FillForm;

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
        app.MapGet("/fillForm", ApiEndpoints.FillForm)
            .Produces<FillFormResponseDto>()
            .WithDescription("Fill form api task")
            .WithTags("Api for AI_devs3");

        return app;
    }

    public static async Task<IResult> PreWork(IPreWorkController service)
    {
        ResponseDto response = await service.RunPreWork();
        return Results.Json(response);
    }

    public static async Task<IResult> FillForm(IFillFormController service)
    {
        FillFormResponseDto response = await service.RunFillForm();
        return Results.Json(response);
    }
}
