using AIWebApi._00_PreWork;
using AIWebApi._01_FillForm;
using AIWebApi._02_Verify;
using AIWebApi._03_FileCorrection;
using AIWebApi._04_Labirynth;
using AIWebApi._05_Censorship;

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
        app.MapGet("/correctFile", ApiEndpoints.CorrectFile)
            .Produces<ResponseDto>()
            .WithDescription("Correct json file")
            .WithTags("Api for AI_devs3");
        app.MapPost("/writeEasyLabirynthPrompt", ApiEndpoints.RunLabirynthEasy)
            .Produces<string>()
            .WithDescription("Run through labirynth easy way")
            .WithTags("Api for AI_devs3");
        app.MapGet("/writeHardLabirynthPrompt", ApiEndpoints.RunLabirynthHard)
            .Produces<string>()
            .WithDescription("Run through labirynth hard way - INCOMPLETE")
            .WithTags("ToDo for AI_devs3");
        app.MapGet("/censorFile", ApiEndpoints.RunCensorship)
            .Produces<ResponseDto>()
            .WithDescription("Censor text file")
            .WithTags("Api for AI_devs3");
        app.MapGet("/censorFileLocal", ApiEndpoints.RunCensorshipWithLocalModel)
            .Produces<ResponseDto>()
            .WithDescription("Censor text file with local model - INCOMPLETE")
            .WithTags("ToDo for AI_devs3");

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

    public static async Task<IResult> CorrectFile(IFileCorrectionController controller)
    {
        ResponseDto response = await controller.RunFileCorrection();
        return Results.Json(response);
    }

    public static async Task<IResult> RunLabirynthEasy(ILabirynthController controller)
    {
        string prompt = await controller.WriteLabirynthPromptEasy();
        return Results.Json(prompt);
    }

    public static async Task<IResult> RunLabirynthHard(ILabirynthController controller)
    {
        string prompt = await controller.WriteLabirynthPromptHard();
        return Results.Json(prompt);
    }

    public static async Task<IResult> RunCensorship(ICensorshipController controller)
    {
        ResponseDto response = await controller.RunCensorship();
        return Results.Json(response);
    }

    public static async Task<IResult> RunCensorshipWithLocalModel(ICensorshipController controller)
    {
        ResponseDto response = await controller.RunCensorshipLocal();
        return Results.Json(response);
    }
}
