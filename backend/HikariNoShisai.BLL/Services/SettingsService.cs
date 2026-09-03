using HikariNoShisai.Common.Entities;
using HikariNoShisai.Common.Interfaces;
using HikariNoShisai.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HikariNoShisai.BLL.Services
{
    public class SettingsService(HikariNoShisaiContext context, IMemoryCache memoryCache) : ISettingsService
    {
        private readonly HikariNoShisaiContext _context = context;
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly Guid _id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private const string Key = "Settings";

        public async Task<TimeSpan> GetTimezoneOffset()
        {
            return TimeSpan.FromMinutes(await GetTimezoneMinutes());
        }

        public async Task<int> GetTimezoneMinutes()
        {
            _memoryCache.TryGetValue<Settings>(Key, out var settings);

            if (settings is null)
            {
                settings = await _context.Settings.FirstOrDefaultAsync(x => x.Id == _id);

                if (settings is null)
                {
                    settings = new Settings
                    {
                        Id = _id,
                        TimezoneOffset = 0
                    };
                    _context.Settings.Add(settings);
                    await _context.SaveChangesAsync();
                }

                _memoryCache.Set(Key, settings);
            }

            return settings.TimezoneOffset;
        }

        public async Task SetTimezoneOffset(int offset)
        {
            var settings = await _context.Settings.FirstOrDefaultAsync(x => x.Id == _id);
            settings!.TimezoneOffset = offset;
            await _context.SaveChangesAsync();
            _memoryCache.Set(Key, settings);
        }

        public async Task MigrateDate()
        {
            await UpdateDateTimeFormattedAsync<Agent>(batchSize: 100);
            await UpdateDateTimeFormattedAsync<AgentStatusLog>(batchSize: 100);
            await UpdateDateTimeFormattedAsync<AgentTerminal>(batchSize: 100);
            await UpdateDateTimeFormattedAsync<Settings>(batchSize: 100);
            await UpdateDateTimeFormattedAsync<User>(batchSize: 100);
        }

        private async Task UpdateDateTimeFormattedAsync<TEntity>(int batchSize = 100) where TEntity : BaseEntity
        {
            var set = _context.Set<TEntity>();
            var total = await set.CountAsync();

            for (int i = 0; i < total; i += batchSize)
            {
                var items = await set
                    .OrderBy(e => e.Id)
                    .Skip(i)
                    .Take(batchSize)
                    .ToListAsync();

                foreach (var item in items)
                {
                    item.CreatedAtFormatted = item.CreatedAt.ToString();
                    item.UpdatedAtFormatted = item.UpdatedAt.ToString();
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}
