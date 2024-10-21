using AIWebApi;
using AIWebApi.Core;
using AIWebApi.PreWork;

using NLog.Web;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Core services
builder.Services.AddSingleton<IHttpService, HttpService>();
builder.Services.AddSingleton<IJsonService, JsonService>();

// App services
builder.Services.AddSingleton<IPreWorkService, PreWorkService>();

builder.WebHost.UseNLog();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.RegisterEndpoints();
app.Run();
