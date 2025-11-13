using HikariNoShisai.Common.Entities;
using HikariNoShisai.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;

namespace HikariNoShisai.WebAPI.Endpoints
{
    public static class AgentEndpoints
    {
        public static void MapAgentEndpoints(this WebApplication app)
        {
            var agentsApi = app.MapGroup("/agents");
            agentsApi.MapGet("/", (HikariNoShisaiContext context) => GetAgentCount(context).ToString())
                     .WithName("GetAgents");
            agentsApi.MapGet("/{id}", (int id) => $"Agent with ID: {id}")
                     .WithName("GetAgentById");
        }

        public static readonly Func<HikariNoShisaiContext, int> GetAgentCount =
            EF.CompileQuery((HikariNoShisaiContext db) =>
                db.Agents.Count());
    }
}
