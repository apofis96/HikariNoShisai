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

        public async Task<int> GetTimezoneOffset()
        {
            _memoryCache.TryGetValue<Settings>(Key, out var offset);

            if (offset is null)
            {
                offset = await _context.Settings.FirstOrDefaultAsync(x => x.Id == _id);

                if (offset is null)
                {
                    offset = new Settings
                    {
                        Id = _id,
                        TimezoneOffset = 0
                    };
                    _context.Settings.Add(offset);
                    await _context.SaveChangesAsync();
                }

                _memoryCache.Set(Key, offset);
            }

            return offset.TimezoneOffset;
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
