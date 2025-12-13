using HikariNoShisai.BLL.Services;
using HikariNoShisai.Common.Configs;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.DAL;
using HikariNoShisai.WebAPI.Endpoints;
using HikariNoShisai.WebAPI.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddAuthentication("ApiKey")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>("ApiKey", _ => { });

builder.Services.AddAuthorization();

builder.Services.Configure<TelegramConfig>(
    builder.Configuration.GetSection("Telegram"));
builder.Services.Configure<AppConfig>(
    builder.Configuration.GetSection("App"));

builder.Services.AddDbContext<HikariNoShisaiContext>(options =>
    options.UseSqlite("Data Source=app.db"));

builder.Services.AddHttpClient("tgwebhook").RemoveAllLoggers()
    .AddTypedClient(httpClient => new TelegramBotClient(builder.Configuration["Telegram:Token"]!, httpClient));
builder.Services.AddTransient<IAgentTerminalService, AgentTerminalService>();
builder.Services.AddTransient<IAgentService, AgentService>();
builder.Services.AddTransient<ITelegramService, TelegramService>();
builder.Services.AddTransient<IAgentStatusLogService, AgentStatusLogService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.MapAgentEndpoints();
app.MapStatisticsEndpoints();
app.MapTelegramEndpoints();
app.MapAgentTerminalEndpoints();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
