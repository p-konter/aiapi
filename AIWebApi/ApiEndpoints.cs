using AIWebApi.Core;

namespace AIWebApi;

public static class ApiEndpoints
{
    public static WebApplication RegisterEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => "Hello World!");
        app.MapGet("/preWork", ApiHandlers.PreWork)
            .Produces<ResponseDto>()
            .WithDescription("PreWork api task")
            .WithTags("Api for AI_devs3");
        app.MapGet("/fillForm", ApiHandlers.FillForm)
            .Produces<FillFormResponseDto>()
            .WithDescription("Fill form api task")
            .WithTags("Api for AI_devs3");
        app.MapGet("/verify", ApiHandlers.Verify)
            .Produces<VerifyDto>()
            .WithDescription("Verify robot")
            .WithTags("Api for AI_devs3");
        app.MapGet("/correctFile", ApiHandlers.CorrectFile)
            .Produces<ResponseDto>()
            .WithDescription("Correct json file")
            .WithTags("Api for AI_devs3");
        app.MapPost("/easyLabirynthPrompt", ApiHandlers.RunLabirynthEasy)
            .Produces<string>()
            .WithDescription("Run through labirynth easy way")
            .WithTags("Api for AI_devs3");
        app.MapGet("/hardLabirynthPrompt", ApiHandlers.RunLabirynthHard)
            .Produces<string>()
            .WithDescription("Run through labirynth hard way - INCOMPLETE")
            .WithTags("ToDo for AI_devs3");
        app.MapGet("/censorFile", ApiHandlers.RunCensorship)
            .Produces<ResponseDto>()
            .WithDescription("Censor text file")
            .WithTags("Api for AI_devs3");
        app.MapGet("/censorFileLocal", ApiHandlers.RunCensorshipWithLocalModel)
            .Produces<ResponseDto>()
            .WithDescription("Censor text file with local model - INCOMPLETE")
            .WithTags("ToDo for AI_devs3");
        app.MapGet("/audioReport", ApiHandlers.RunAudioReport)
            .Produces<ResponseDto>()
            .WithDescription("Run report from audio files")
            .WithTags("Api for AI_devs3");
        app.MapGet("/recognizeMap", ApiHandlers.RunRecognizeMap)
            .Produces<string>()
            .WithDescription("Recognize cities on the map")
            .WithTags("Api for AI_devs3");
        app.MapGet("/generateRobot", ApiHandlers.RunRobotGeneration)
            .Produces<ResponseDto>()
            .WithDescription("Generate robot image")
            .WithTags("Api for AI_devs3");
        app.MapGet("/sortFiles", ApiHandlers.RunSortFiles)
            .Produces<ResponseDto>()
            .WithDescription("Sort data files")
            .WithTags("Api for AI_devs3");
        app.MapGet("/clearFiles", ApiHandlers.ClearSortFiles)
            .Produces<bool>()
            .WithDescription("Clear data files")
            .WithTags("Api for AI_devs3");
        app.MapGet("/generateKeywords", ApiHandlers.RunGenerateKeywords)
            .Produces<ResponseDto>()
            .WithDescription("Generate kaywords to text files")
            .WithTags("Api for AI_devs3");
        app.MapGet("/getDateFromVector", ApiHandlers.GetDateFromVector)
            .Produces<ResponseDto>()
            .WithDescription("Get date with vector data")
            .WithTags("Api for AI_devs3");

        return app;
    }
}
