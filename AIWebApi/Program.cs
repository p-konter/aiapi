using AIWebApi;

using NLog.Web;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.UseNLog();

builder.Services.AddCoreServices();
builder.Services.AddTasksControllers();

WebApplication app = builder.Build();

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.RegisterEndpoints();
app.Run();
