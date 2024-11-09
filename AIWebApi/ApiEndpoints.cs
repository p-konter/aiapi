using AIWebApi._00_PreWork;
using AIWebApi._01_FillForm;
using AIWebApi._02_Verify;

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
        app.MapGet("/verify", ApiEndpoints.Verify)
            .Produces<VerifyDto>()
            .WithDescription("Verify robot")
            .WithTags("Api for AI_devs3");

        return app;
    }

    public static async Task<IResult> PreWork(IPreWorkController controller)
    {
        ResponseDto response = await controller.RunPreWork();
        return Results.Json(response);
    }

    public static async Task<IResult> FillForm(IFillFormController controller)
    {
        FillFormResponseDto response = await controller.RunFillForm();
        return Results.Json(response);
    }

    public static async Task<IResult> Verify(IVerifyController controller)
    {
        VerifyDto response = await controller.RunVerify();
        return Results.Json(response);
    }
}
