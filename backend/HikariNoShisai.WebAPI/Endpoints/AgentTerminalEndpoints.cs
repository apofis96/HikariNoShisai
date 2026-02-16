using HikariNoShisai.Common.DTO;
using HikariNoShisai.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace HikariNoShisai.WebAPI.Endpoints
{
    public static class AgentTerminalEndpoints
    {
        public static void MapAgentTerminalEndpoints(this WebApplication app)
        {
            var agentTerminalsApi = app.MapGroup("/terminal").RequireAuthorization();
            agentTerminalsApi.MapGet("/", async ([AsParameters] AgentTerminalRequest test, IAgentTerminalService agentTerminalService, IAgentWatchdog agentWatchdog) =>
            {
                agentWatchdog.Update(test.AgentId);
                var status = await agentTerminalService.GetAgentTerminalStatus(test.AgentId, test.TerminalId);

                return Results.Ok("<"+status+">");
            }).WithName("GetAgentTerminal");
            agentTerminalsApi.MapPatch("/", async ([AsParameters] AgentTerminalStatusPatch request, IAgentTerminalService agentTerminalService) =>
            {
                await agentTerminalService.SetAgentTerminalStatus(request.AgentId, request.TerminalId, request.IsActive);

                return Results.NoContent();
            }).WithName("SetAgentTerminal");
        }
    }
}
