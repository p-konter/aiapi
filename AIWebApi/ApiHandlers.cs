using AIWebApi.Tasks;
using AIWebApi.Tasks._00_PreWork;
using AIWebApi.Tasks._01_FillForm;
using AIWebApi.Tasks._02_Verify;
using AIWebApi.Tasks._03_FileCorrection;
using AIWebApi.Tasks._04_Labirynth;
using AIWebApi.Tasks._05_Censorship;
using AIWebApi.Tasks._06_AudioReport;
using AIWebApi.Tasks._07_RecognizeMap;
using AIWebApi.Tasks._08_GenerateRobot;
using AIWebApi.Tasks._09_SortFiles;
using AIWebApi.Tasks._11_GenerateKeywords;
using AIWebApi.Tasks._12_DateFromVector;

namespace AIWebApi;

public static class ApiHandlers
{
    public static Task<IResult> ClearLogFiles(ICleanController controller) => ExecuteControllerMethod(c => c.ClearLogFiles(), controller);
    public static Task<IResult> ClearWorkDir(ICleanController controller) => ExecuteControllerMethod(c => c.ClearWorkDir(), controller);
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
    public static Task<IResult> RunRobotGeneration(IGenerateRobotController controller) => ExecuteControllerMethod(c => c.RunRobotGeneration(), controller);
    public static Task<IResult> RunSortFiles(ISortFilesController controller) => ExecuteControllerMethod(c => c.RunSortFiles(), controller);
    public static Task<IResult> RunGenerateKeywords(IGenerateKeywordsController controller) => ExecuteControllerMethod(c => c.RunGenerateKeywords(), controller);
    public static Task<IResult> GetDateFromVector(IDateFromVectorController controller) => ExecuteControllerMethod(c => c.GetDateFromVector(), controller);

    private static async Task<IResult> ExecuteControllerMethod<TController, TResponse>(Func<TController, Task<TResponse>> method, TController controller)
    {
        TResponse response = await method(controller);
        return Results.Json(response);
    }
}
