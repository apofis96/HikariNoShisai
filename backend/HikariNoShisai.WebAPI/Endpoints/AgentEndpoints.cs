using HikariNoShisai.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HikariNoShisai.WebAPI.Endpoints
{
    public static class AgentEndpoints
    {
        public static void MapAgentEndpoints(this WebApplication app)
        {
            var agentsApi = app.MapGroup("/agents");
            agentsApi.MapGet("/", async (IAgentService agentService) => {
                var all = await agentService.GetAll();
                return Results.Ok("List of agents " + all.Count());
            }).WithName("GetAgents");
            agentsApi.MapGet("/{id}", (int id) => $"Agent with ID: {id}")
                     .WithName("GetAgentById");
        }
    }
}
