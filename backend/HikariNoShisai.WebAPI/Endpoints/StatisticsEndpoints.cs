namespace HikariNoShisai.WebAPI.Endpoints
{
    public static class StatisticsEndpoints
    {
        public static void MapStatisticsEndpoints(this WebApplication app)
        {
            var agentsApi = app.MapGroup("/statistics");
            agentsApi.MapGet("/", () => "Statistics data")
                     .WithName("GetStatistics");
        }
    }
}
