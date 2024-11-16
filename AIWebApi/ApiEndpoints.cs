using AIWebApi._00_PreWork;
using AIWebApi._01_FillForm;
using AIWebApi._02_Verify;
using AIWebApi._03_FileCorrection;
using AIWebApi._04_Labirynth;
using AIWebApi._05_Censorship;
using AIWebApi._06_AudioReport;
using AIWebApi._07_RecognizeMap;
using AIWebApi.Core;

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
        app.MapGet("/audioReport", ApiEndpoints.RunAudioReport)
            .Produces<ResponseDto>()
            .WithDescription("Run report from audio files")
            .WithTags("Api for AI_devs3");
        app.MapGet("/recognizeMap", ApiEndpoints.RunRecognizeMap)
            .Produces<string>()
            .WithDescription("Recognize cities on the map")
            .WithTags("Api for AI_devs3");

        return app;
    }

    public static Task<IResult> PreWork(IPreWorkController controller) => ExecuteControllerMethod(c => c.RunPreWork(), controller);
    public static Task<IResult> FillForm(IFillFormController controller) => ExecuteControllerMethod(c => c.RunFillForm(), controller);
    public static Task<IResult> Verify(IVerifyController controller) => ExecuteControllerMethod(c => c.RunVerify(), controller);
    public static Task<IResult> CorrectFile(IFileCorrectionController controller) => ExecuteControllerMethod(c => c.RunFileCorrection(), controller);
    public static Task<IResult> RunLabirynthEasy(ILabirynthController controller) => ExecuteControllerMethod(c => c.WriteLabirynthPromptEasy(), controller);
    public static Task<IResult> RunLabirynthHard(ILabirynthController controller) => ExecuteControllerMethod(c => c.WriteLabirynthPromptHard(), controller);
    public static Task<IResult> RunCensorship(ICensorshipController controller) => ExecuteControllerMethod(c => c.RunCensorship(), controller);
    public static Task<IResult> RunCensorshipWithLocalModel(ICensorshipController controller) => ExecuteControllerMethod(c => c.RunCensorshipLocal(), controller);
    public static Task<IResult> RunAudioReport(IAudioReportController controller) => ExecuteControllerMethod(c => c.RunAudioRepost(), controller);
    public static Task<IResult> RunRecognizeMap(IRecognizeMapController controller) => ExecuteControllerMethod(c => c.RunRecognizeMap(), controller);

    private static async Task<IResult> ExecuteControllerMethod<TController, TResponse>(Func<TController, Task<TResponse>> method, TController controller)
    {
        TResponse response = await method(controller);
        return Results.Json(response);
    }
}
