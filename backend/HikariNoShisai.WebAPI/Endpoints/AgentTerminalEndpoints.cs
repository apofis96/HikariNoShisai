using HikariNoShisai.Common.DTO;
using HikariNoShisai.Common.Helpers;
using HikariNoShisai.Common.Interfaces;

namespace HikariNoShisai.WebAPI.Endpoints
{
    public static class AgentTerminalEndpoints
    {
        public static void MapAgentTerminalEndpoints(this WebApplication app)
        {
            var agentTerminalsApi = app.MapGroup("/terminal").RequireAuthorization();
            agentTerminalsApi.MapGet("/", async ([AsParameters] AgentTerminalRequest request, IAgentTerminalService agentTerminalService, IAgentWatchdog agentWatchdog) =>
            {
                agentWatchdog.Update(request.AgentId);
                var status = await agentTerminalService.GetAgentTerminalStatus(request.AgentId, request.TerminalId);

                return Results.Ok(StringHelpers.FormatAgentResponse(status));
            }).WithName("GetAgentTerminal");
            agentTerminalsApi.MapPatch("/", async ([AsParameters] AgentTerminalStatusPatch request, IAgentTerminalService agentTerminalService) =>
            {
                await agentTerminalService.SetAgentTerminalStatus(request.AgentId, request.TerminalId, request.IsActive);

                return Results.NoContent();
            }).WithName("SetAgentTerminal");
        }
    }
}
