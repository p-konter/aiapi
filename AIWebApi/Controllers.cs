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
using AIWebApi.Tasks._10_AnswerQuestions;
using AIWebApi.Tasks._11_GenerateKeywords;
using AIWebApi.Tasks._12_DateFromVector;
using AIWebApi.Tasks._13_ExtractFromSql;

namespace AIWebApi;

public static class Controllers
{
    public static IServiceCollection AddTasksControllers(this IServiceCollection services)
    {
        services.AddScoped<IAnswerQuestionsController, AnswerQuestionsController>();
        services.AddScoped<IAudioReportController, AudioReportController>();
        services.AddScoped<ICensorshipController, CensorshipController>();
        services.AddScoped<ICleanController, CleanController>();
        services.AddScoped<IDateFromVectorController, DateFromVectorController>();
        services.AddScoped<IExtractFromSqlController, ExtractFromSqlController>();
        services.AddScoped<IFileCorrectionController, FileCorrectionController>();
        services.AddScoped<IFillFormController, FillFormController>();
        services.AddScoped<IGenerateKeywordsController, GenerateKeywordsController>();
        services.AddScoped<IGenerateRobotController, GenerateRobotController>();
        services.AddScoped<ILabirynthController, LabirynthController>();
        services.AddScoped<IPreWorkController, PreWorkController>();
        services.AddScoped<IRecognizeMapController, RecognizeMapController>();
        services.AddScoped<ISortFilesController, SortFilesController>();
        services.AddScoped<IVerifyController, VerifyController>();

        return services;
    }
}
