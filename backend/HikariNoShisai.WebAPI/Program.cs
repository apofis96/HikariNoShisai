using HikariNoShisai.DAL;
using HikariNoShisai.WebAPI.Endpoints;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

var builder = WebApplication.CreateSlimBuilder(args);

var token = builder.Configuration["Telegram:Token"]!;

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<HikariNoShisaiContext>(options =>
    options.UseSqlite("Data Source=app.db")
    .UseModel(HikariNoShisaiContext.CompiledModel));

builder.Services.ConfigureTelegramBot<JsonOptions>(opt => opt.SerializerOptions);
builder.Services.AddHttpClient("tgwebhook").RemoveAllLoggers().AddTypedClient(httpClient => new TelegramBotClient(token, httpClient));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapAgentEndpoints();
app.MapStatisticsEndpoints();
app.MapTelegramEndpoints();

app.Run();
