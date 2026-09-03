using HikariNoShisai.Common.Helpers;
using HikariNoShisai.Common.Interfaces;

namespace HikariNoShisai.WebAPI.Endpoints
{
    public static class SettingsEndpoints
    {
        public static void MapSettingsEndpoints(this WebApplication app)
        {
            var settingsApi = app.MapGroup("/settings").RequireAuthorization();
            settingsApi.MapGet("/", async (ISettingsService settingsService) =>
            {
                var offset = await settingsService.GetTimezoneMinutes();

                return Results.Ok(StringHelpers.FormatAgentResponse(offset));
            }).WithName("TimezoneOffset");

            settingsApi.MapGet("/migrate", async (ISettingsService settingsService) =>
            {
                await settingsService.MigrateDate();

                return Results.Ok();
            }).WithName("MigrateDate");
        }
    }
}
