using AIWebApi;
using AIWebApi._00_PreWork;
using AIWebApi._01_FillForm;
using AIWebApi._02_Verify;
using AIWebApi._03_FileCorrection;
using AIWebApi._04_Labirynth;
using AIWebApi._05_Censorship;
using AIWebApi._06_AudioReport;
using AIWebApi._07_RecognizeMap;
using AIWebApi._08_GenerateRobot;
using AIWebApi._09_SortFiles;
using AIWebApi.Core;

using NLog.Web;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.UseNLog();

// Core services
builder.Services.AddSingleton<IAudioAIService, AudioAIService>();
builder.Services.AddSingleton<IFileService, FileService>();
builder.Services.AddSingleton<IHttpService, HttpService>();
builder.Services.AddSingleton<IImageAIService, ImageAIService>();
builder.Services.AddSingleton<IJsonService, JsonService>();
builder.Services.AddSingleton<IZipService, ZipService>();

builder.Services.AddSingleton<IGPT4AIService>(sp =>
{
    IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
    ILogger<ChatAIService> logger = sp.GetRequiredService<ILogger<ChatAIService>>();
    return new ChatAIService("gpt-4o", configuration, logger);
});
builder.Services.AddSingleton<IGPT4MiniAIService>(sp =>
{
    IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
    ILogger<ChatAIService> logger = sp.GetRequiredService<ILogger<ChatAIService>>();
    return new ChatAIService("gpt-4o-mini", configuration, logger);
});

// App controllers
builder.Services.AddScoped<IAudioReportController, AudioReportController>();
builder.Services.AddScoped<ICensorshipController, CensorshipController>();
builder.Services.AddScoped<IFileCorrectionController, FileCorrectionController>();
builder.Services.AddScoped<IFillFormController, FillFormController>();
builder.Services.AddScoped<IGenerateRobotController, GenerateRobotController>();
builder.Services.AddScoped<ILabirynthController, LabirynthController>();
builder.Services.AddScoped<IPreWorkController, PreWorkController>();
builder.Services.AddScoped<IRecognizeMapController, RecognizeMapController>();
builder.Services.AddScoped<ISortFilesController, SortFilesController>();
builder.Services.AddScoped<IVerifyController, VerifyController>();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.RegisterEndpoints();
app.Run();
