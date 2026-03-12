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
    }
}
