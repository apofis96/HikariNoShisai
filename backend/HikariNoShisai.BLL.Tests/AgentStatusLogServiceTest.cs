using HikariNoShisai.BLL.Services;
using HikariNoShisai.Common.DTO;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.DAL;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HikariNoShisai.BLL.Tests
{
    public class AgentStatusLogServiceTest
    {
        private HikariNoShisaiContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<HikariNoShisaiContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new HikariNoShisaiContext(options);
        }

        [Fact]
        public async Task Create_SaveToDB()
        {
            var context = CreateContext();
            var mockMessageQueue = new Mock<IMessageQueue>();
            var mockSettingsService = new Mock<ISettingsService>();
            var service = new AgentStatusLogService(context, mockMessageQueue.Object, mockSettingsService.Object);
            var request = new AgentStatusLogRequest
            {
                AgentId = Guid.NewGuid(),
                IsGridAvailable = true,
                GridVoltage = 220,
                BatteryVoltage = 12,
            };

            await service.Create(request);

            Assert.Single(context.AgentStatusLogs);
            Assert.Equal(request.AgentId, context.AgentStatusLogs.First().AgentId);
            Assert.Equal(request.IsGridAvailable, context.AgentStatusLogs.First().IsGridAvailable);
            Assert.Equal(request.GridVoltage, context.AgentStatusLogs.First().GridVoltage);
            Assert.Equal(request.BatteryVoltage, context.AgentStatusLogs.First().BatteryVoltage);
        }
    }
}
