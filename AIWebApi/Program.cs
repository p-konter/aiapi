using AIWebApi;
using AIWebApi._00_PreWork;
using AIWebApi._01_FillForm;
using AIWebApi._02_Verify;
using AIWebApi._03_FileCorrection;
using AIWebApi._04_Labirynth;
using AIWebApi._05_Censorship;
using AIWebApi.Core;

using NLog.Web;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Core services
builder.Services.AddSingleton<IHttpService, HttpService>();
builder.Services.AddSingleton<IJsonService, JsonService>();

// App controllers
builder.Services.AddSingleton<ICensorshipController, CensorshipController>();
builder.Services.AddSingleton<IFileCorrectionController, FileCorrectionController>();
builder.Services.AddSingleton<IFillFormController, FillFormController>();
builder.Services.AddSingleton<ILabirynthController, LabirynthController>();
builder.Services.AddSingleton<IPreWorkController, PreWorkController>();
builder.Services.AddSingleton<IVerifyController, VerifyController>();

builder.WebHost.UseNLog();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.RegisterEndpoints();
app.Run();
