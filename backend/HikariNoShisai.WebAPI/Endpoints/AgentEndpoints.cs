using HikariNoShisai.DAL;

namespace HikariNoShisai.WebAPI.Endpoints
{
    public static class AgentEndpoints
    {
        public static void MapAgentEndpoints(this WebApplication app)
        {
            var agentsApi = app.MapGroup("/agents");
            agentsApi.MapGet("/", (HikariNoShisaiContext context) => context.Agents.ToList().Count.ToString())
                     .WithName("GetAgents");
            agentsApi.MapGet("/{id}", (int id) => $"Agent with ID: {id}")
                     .WithName("GetAgentById");
        }
    }
}
