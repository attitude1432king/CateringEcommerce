using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CateringEcommerce.BAL.Base.Common
{
    /// <summary>
    /// Returns live partner platform statistics for public pages (partner login, home).
    /// Results are cached in-process for 1 hour to avoid hitting the DB on every page load.
    /// AvgGrowthPercent is configurable via system setting STATS.AVG_GROWTH_PERCENT (default 150).
    /// </summary>
    public class PublicStatsRepository : IPublicStatsRepository
    {
        private readonly IDatabaseHelper _db;
        private readonly ISystemSettingsProvider _settings;
        private readonly IMemoryCache _cache;

        private const string CacheKey = "public:partner_stats";

        private static readonly string StatsSql = $@"
            SELECT
                (
                    SELECT COUNT(*)
                    FROM {Table.SysCateringOwner}
                    WHERE COALESCE(c_isactive, FALSE)    = 1
                      AND c_approval_status        = 'Approved'
                      AND COALESCE(c_is_deleted, FALSE)   = 0
                ) AS ActivePartners,

                (
                    SELECT COUNT(*)
                    FROM {Table.SysOrders}
                    WHERE c_order_status = 'Completed'
                ) AS CompletedEvents,

                (
                    SELECT COUNT(DISTINCT cd.c_cityid)
                    FROM   {Table.SysCateringOwner}          co
                    INNER JOIN {Table.SysCateringOwnerAddress} cd ON cd.c_ownerid = co.c_ownerid
                    WHERE  COALESCE(co.c_isactive, 0)  = 1
                      AND  co.c_approval_status       = 'Approved'
                      AND  COALESCE(co.c_is_deleted, 0)  = 0
                ) AS CitiesServed";

        public PublicStatsRepository(
            IDatabaseHelper db,
            ISystemSettingsProvider settings,
            IMemoryCache cache)
        {
            _db = db;
            _settings = settings;
            _cache = cache;
        }

        public async Task<PartnerStats> GetPartnerStatsAsync()
        {
            if (_cache.TryGetValue(CacheKey, out PartnerStats? cached) && cached != null)
                return cached;

            var dt = await _db.ExecuteAsync(StatsSql);

            var stats = new PartnerStats { AvgGrowthPercent = _settings.GetInt("STATS.AVG_GROWTH_PERCENT", 150) };

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                stats.ActivePartners   = row["ActivePartners"]  is DBNull ? 0 : Convert.ToInt32(row["ActivePartners"]);
                stats.CompletedEvents  = row["CompletedEvents"] is DBNull ? 0 : Convert.ToInt32(row["CompletedEvents"]);
                stats.CitiesServed     = row["CitiesServed"]    is DBNull ? 0 : Convert.ToInt32(row["CitiesServed"]);
            }

            _cache.Set(CacheKey, stats, TimeSpan.FromHours(1));
            return stats;
        }
    }
}

