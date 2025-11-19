using HikariNoShisai.Common.DTO;
using HikariNoShisai.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace HikariNoShisai.WebAPI.Endpoints
{
    public static class AgentTerminalEndpoints
    {
        public static void MapAgentTerminalEndpoints(this WebApplication app)
        {
            var agentTerminalsApi = app.MapGroup("/terminal");
            agentTerminalsApi.MapGet("/", async ([AsParameters] AgentTerminalRequest test, IAgentTerminalService agentTerminalService) =>
            {
                var status = await agentTerminalService.GetAgentTerminalStatus(test.AgentId, test.TerminalId);

                return Results.Ok(status);
            }).WithName("GetAgentTerminal");
        }
    }
}
