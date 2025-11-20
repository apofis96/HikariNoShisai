using HikariNoShisai.BLL.Services;
using HikariNoShisai.Common.Configs;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.DAL;
using HikariNoShisai.WebAPI;
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
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});

builder.Services.AddHttpClient("tgwebhook").RemoveAllLoggers()
    .AddTypedClient(httpClient => new TelegramBotClient(builder.Configuration["Telegram:Token"]!, httpClient));
builder.Services.AddTransient<IAgentTerminalService, AgentTerminalService>();
builder.Services.AddTransient<IAgentService, AgentService>();
builder.Services.AddTransient<ITelegramService, TelegramService>();


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapAgentEndpoints();
app.MapStatisticsEndpoints();
app.MapTelegramEndpoints();
app.MapAgentTerminalEndpoints();

app.Run();
