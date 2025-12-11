using HikariNoShisai.Common.DTO;
using HikariNoShisai.Common.Interfaces;

namespace HikariNoShisai.WebAPI.Endpoints
{
    public static class StatisticsEndpoints
    {
        public static void MapStatisticsEndpoints(this WebApplication app)
        {
            var agentsApi = app.MapGroup("/statistics").RequireAuthorization();

            agentsApi.MapPost("/", async (AgentStatusLogRequest request, IAgentStatusLogService agentStatusLogService) =>
            {
                await agentStatusLogService.Create(request);

                return Results.Created();
            }).WithName("WriteAgentStatusLog");
        }
    }
}
