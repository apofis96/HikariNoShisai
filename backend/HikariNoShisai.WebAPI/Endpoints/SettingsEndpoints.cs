using HikariNoShisai.Common.Helpers;
using HikariNoShisai.Common.Interfaces;

namespace HikariNoShisai.WebAPI.Endpoints
{
    public static class SettingsEndpoints
    {
        public static void MapSettingsEndpoints(this WebApplication app)
        {
            var agentTerminalsApi = app.MapGroup("/settings").RequireAuthorization();
            agentTerminalsApi.MapGet("/", async (ISettingsService settingsService) =>
            {
                var offset = await settingsService.GetTimezoneMinutes();

                return Results.Ok(StringHelpers.FormatAgentResponse(offset));
            }).WithName("TimezoneOffset");
        }
    }
}
