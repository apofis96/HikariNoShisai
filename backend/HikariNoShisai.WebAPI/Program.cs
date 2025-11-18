using HikariNoShisai.Common.Configs;
using HikariNoShisai.DAL;
using HikariNoShisai.WebAPI.Endpoints;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

var builder = WebApplication.CreateSlimBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<TelegramConfig>(
    builder.Configuration.GetSection("Telegram"));

builder.Services.AddDbContext<HikariNoShisaiContext>(options =>
    options.UseSqlite("Data Source=app.db")
    .UseModel(HikariNoShisaiContext.CompiledModel));

builder.Services.ConfigureTelegramBot<JsonOptions>(opt => opt.SerializerOptions);
builder.Services.AddHttpClient("tgwebhook").RemoveAllLoggers()
    .AddTypedClient(httpClient => new TelegramBotClient(builder.Configuration["Telegram:Token"]!, httpClient));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapAgentEndpoints();
app.MapStatisticsEndpoints();
app.MapTelegramEndpoints();

app.Run();
