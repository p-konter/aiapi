using AIWebApi;
using AIWebApi._00_PreWork;
using AIWebApi._01_FillForm;
using AIWebApi.Core;

using NLog.Web;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Core services
builder.Services.AddScoped<IHttpService, HttpService>();
builder.Services.AddScoped<IJsonService, JsonService>();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();

// App controllers
builder.Services.AddScoped<IFillFormController, FillFormController>();
builder.Services.AddScoped<IPreWorkController, PreWorkController>();

builder.WebHost.UseNLog();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.RegisterEndpoints();
app.Run();
