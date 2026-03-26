using HikariNoShisai.Common.Helpers;
using HikariNoShisai.Common.Interfaces;

namespace HikariNoShisai.WebAPI.Endpoints
{
    public static class AgentEndpoints
    {
        public static void MapAgentEndpoints(this WebApplication app)
        {
            var agentsApi = app.MapGroup("/agents").RequireAuthorization();
            agentsApi.MapGet("/", async (IAgentService agentService) => {
                var all = await agentService.GetAll();
                return Results.Ok("List of agents " + all.Count());
            }).WithName("GetAgents");

            agentsApi.MapGet("/{id}", (int id) => $"Agent with ID: {id}")
                .WithName("GetAgentById");

            agentsApi.MapGet("/{id}/weather", async (Guid id, IAgentService agentService) => {
                var weather = await agentService.GetWeather(id);
                return Results.Ok(StringHelpers.FormatAgentResponse(weather));
            }).WithName("GetAgentWeather");
        }
    }
}
