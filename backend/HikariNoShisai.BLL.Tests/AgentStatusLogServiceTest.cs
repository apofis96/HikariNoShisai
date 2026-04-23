using HikariNoShisai.BLL.Services;
using HikariNoShisai.Common.DTO;
using HikariNoShisai.Common.Entities;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.NetworkInformation;
using static HikariNoShisai.Common.Constants.TextConstants;

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
        #region Create
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
        #endregion
        #region EmitGridNotification
        [Fact]
        public async Task EmitGridNotification_WhenNoLastAgentStatusInDb_NoAction()
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
            mockMessageQueue.Verify(x => x.Send(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EmitGridNotification_WhenStatusSame_NoAction(bool status)
        {
            var context = CreateContext();
            var mockMessageQueue = new Mock<IMessageQueue>();
            var mockSettingsService = new Mock<ISettingsService>();
            var service = new AgentStatusLogService(context, mockMessageQueue.Object, mockSettingsService.Object);
            var agentId = Guid.NewGuid();
            context.AgentStatusLogs.Add(new AgentStatusLog
            {
                AgentId = agentId,
                IsGridAvailable = status,
                GridVoltage = 220,
                BatteryVoltage = 12,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
            });
            context.SaveChanges();
            var request = new AgentStatusLogRequest
            {
                AgentId = agentId,
                IsGridAvailable = status,
                GridVoltage = 220,
                BatteryVoltage = 12,
            };

            await service.Create(request);

            Assert.Equal(2, context.AgentStatusLogs.Count());
            mockMessageQueue.Verify(x => x.Send(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EmitGridNotification_WhenStatusNotSame_EmitNotification(bool status)
        {
            var context = CreateContext();
            var mockMessageQueue = new Mock<IMessageQueue>();
            var mockSettingsService = new Mock<ISettingsService>();
            var service = new AgentStatusLogService(context, mockMessageQueue.Object, mockSettingsService.Object);
            var agentId = Guid.NewGuid();
            context.AgentStatusLogs.Add(new AgentStatusLog
            {
                AgentId = agentId,
                IsGridAvailable = status,
                GridVoltage = 220,
                BatteryVoltage = 12,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
            });
            context.SaveChanges();
            var request = new AgentStatusLogRequest
            {
                AgentId = agentId,
                IsGridAvailable = !status,
                GridVoltage = 220,
                BatteryVoltage = 12,
            };

            await service.Create(request);

            Assert.Equal(2, context.AgentStatusLogs.Count());
            mockMessageQueue.Verify(x => x.Send(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
        #endregion
        #region GetGridStatistics
        [Fact]
        public async Task GetGridStatistics_WhenNoData_ReturnsEmptyChart()
        {
            var context = CreateContext();
            var mockMessageQueue = new Mock<IMessageQueue>();
            var mockSettingsService = new Mock<ISettingsService>();
            var service = new AgentStatusLogService(context, mockMessageQueue.Object, mockSettingsService.Object);

            var gridStatistics = await service.GetGridStatistics(DateTimeOffset.UtcNow);

            Assert.NotNull(gridStatistics);
            Assert.Equal(MessageTemplate.StatusLogChartTitle, gridStatistics.Title);
            Assert.Equal(0.0, gridStatistics.GridAvailableCount);
            Assert.Equal(100.0, gridStatistics.GridUnavailableCount);
        }

        [Fact]
        public async Task GetGridStatistics_WhenNoDataForPeriodButAvailable_ReturnsAvailableChart()
        {
            var context = CreateContext();
            var mockMessageQueue = new Mock<IMessageQueue>();
            var mockSettingsService = new Mock<ISettingsService>();
            var service = new AgentStatusLogService(context, mockMessageQueue.Object, mockSettingsService.Object);
            context.AgentStatusLogs.Add(new AgentStatusLog
            {
                AgentId = Guid.NewGuid(),
                IsGridAvailable = true,
                GridVoltage = 220,
                BatteryVoltage = 12,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-5)
            });
            context.SaveChanges();

            var gridStatistics = await service.GetGridStatistics(DateTimeOffset.UtcNow.AddDays(-1));

            Assert.NotNull(gridStatistics);
            Assert.Equal(MessageTemplate.StatusLogChartTitle, gridStatistics.Title);
            Assert.Equal(100.0, gridStatistics.GridAvailableCount);
            Assert.Equal(0.0, gridStatistics.GridUnavailableCount);
        }

        [Fact]
        public async Task GetGridStatistics_WhenNoDataForPeriodButNotAvailable_ReturnsNotAvailableChart()
        {
            var context = CreateContext();
            var mockMessageQueue = new Mock<IMessageQueue>();
            var mockSettingsService = new Mock<ISettingsService>();
            var service = new AgentStatusLogService(context, mockMessageQueue.Object, mockSettingsService.Object);
            context.AgentStatusLogs.Add(new AgentStatusLog
            {
                AgentId = Guid.NewGuid(),
                IsGridAvailable = true,
                GridVoltage = 220,
                BatteryVoltage = 12,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-5)
            });
            context.AgentStatusLogs.Add(new AgentStatusLog
            {
                AgentId = Guid.NewGuid(),
                IsGridAvailable = false,
                GridVoltage = 220,
                BatteryVoltage = 12,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-3)
            });
            context.SaveChanges();

            var gridStatistics = await service.GetGridStatistics(DateTimeOffset.UtcNow.AddDays(-1));

            Assert.NotNull(gridStatistics);
            Assert.Equal(MessageTemplate.StatusLogChartTitle, gridStatistics.Title);
            Assert.Equal(0.0, gridStatistics.GridAvailableCount);
            Assert.Equal(100.0, gridStatistics.GridUnavailableCount);
        }

        [Fact]
        public async Task GetGridStatistics_WhenSingleDataForPeriodWithPrevious_ReturnsChart()
        {
            var context = CreateContext();
            var mockMessageQueue = new Mock<IMessageQueue>();
            var mockSettingsService = new Mock<ISettingsService>();
            var service = new AgentStatusLogService(context, mockMessageQueue.Object, mockSettingsService.Object);
            var utcNow = DateTimeOffset.UtcNow;
            context.AgentStatusLogs.Add(new AgentStatusLog
            {
                AgentId = Guid.NewGuid(),
                IsGridAvailable = true,
                GridVoltage = 220,
                BatteryVoltage = 12,
                CreatedAt = utcNow.AddDays(-5)
            });
            context.AgentStatusLogs.Add(new AgentStatusLog
            {
                AgentId = Guid.NewGuid(),
                IsGridAvailable = false,
                GridVoltage = 220,
                BatteryVoltage = 12,
                CreatedAt = utcNow.AddHours(-12)
            });
            context.SaveChanges();

            var gridStatistics = await service.GetGridStatistics(utcNow.AddDays(-1));

            Assert.NotNull(gridStatistics);
            Assert.Equal(MessageTemplate.StatusLogChartTitle, gridStatistics.Title);
            Assert.Equal(50.0, gridStatistics.GridAvailableCount);
            Assert.Equal(50.0, gridStatistics.GridUnavailableCount);
        }
        [Fact]
        public async Task GetGridStatistics_WhenMultipleDataForPeriodWithPrevious_ReturnsChart()
        {
            var context = CreateContext();
            var mockMessageQueue = new Mock<IMessageQueue>();
            var mockSettingsService = new Mock<ISettingsService>();
            var service = new AgentStatusLogService(context, mockMessageQueue.Object, mockSettingsService.Object);
            var utcNow = DateTimeOffset.UtcNow;
            context.AgentStatusLogs.Add(new AgentStatusLog
            {
                AgentId = Guid.NewGuid(),
                IsGridAvailable = true,
                GridVoltage = 220,
                BatteryVoltage = 12,
                CreatedAt = utcNow.AddDays(-5)
            });
            context.AgentStatusLogs.Add(new AgentStatusLog
            {
                AgentId = Guid.NewGuid(),
                IsGridAvailable = false,
                GridVoltage = 220,
                BatteryVoltage = 12,
                CreatedAt = utcNow.AddHours(-16)
            });
            context.AgentStatusLogs.Add(new AgentStatusLog
            {
                AgentId = Guid.NewGuid(),
                IsGridAvailable = true,
                GridVoltage = 220,
                BatteryVoltage = 12,
                CreatedAt = utcNow.AddHours(-8)
            });
            context.SaveChanges();

            var gridStatistics = await service.GetGridStatistics(utcNow.AddDays(-1));

            Assert.NotNull(gridStatistics);
            Assert.Equal(MessageTemplate.StatusLogChartTitle, gridStatistics.Title);
            Assert.Equal(66.67, gridStatistics.GridAvailableCount);
            Assert.Equal(33.33, gridStatistics.GridUnavailableCount);
        }
        [Fact]
        public async Task GetGridStatistics_WhenMultipleDataForPeriodWithPreviousAgentNotMatch_ReturnsChart()
        {
            var context = CreateContext();
            var mockMessageQueue = new Mock<IMessageQueue>();
            var mockSettingsService = new Mock<ISettingsService>();
            var service = new AgentStatusLogService(context, mockMessageQueue.Object, mockSettingsService.Object);
            var utcNow = DateTimeOffset.UtcNow;
            var agentId = Guid.NewGuid();
            context.AgentStatusLogs.Add(new AgentStatusLog
            {
                AgentId = Guid.NewGuid(),
                IsGridAvailable = true,
                GridVoltage = 220,
                BatteryVoltage = 12,
                CreatedAt = utcNow.AddDays(-5)
            });
            context.AgentStatusLogs.Add(new AgentStatusLog
            {
                AgentId = agentId,
                IsGridAvailable = false,
                GridVoltage = 220,
                BatteryVoltage = 12,
                CreatedAt = utcNow.AddHours(-16)
            });
            context.AgentStatusLogs.Add(new AgentStatusLog
            {
                AgentId = agentId,
                IsGridAvailable = true,
                GridVoltage = 220,
                BatteryVoltage = 12,
                CreatedAt = utcNow.AddHours(-8)
            });
            context.SaveChanges();

            var gridStatistics = await service.GetGridStatistics(utcNow.AddDays(-1), agentId: agentId);

            Assert.NotNull(gridStatistics);
            Assert.Equal(MessageTemplate.StatusLogChartTitle, gridStatistics.Title);
            Assert.Equal(50, gridStatistics.GridAvailableCount);
            Assert.Equal(50, gridStatistics.GridUnavailableCount);
        }
        #endregion
    }
}
