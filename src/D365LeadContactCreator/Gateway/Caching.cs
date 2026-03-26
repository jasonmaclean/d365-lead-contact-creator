using System;
using System.Collections.Generic;
using System.Linq;
using static SitecoreFundamentals.D365LeadContactCreator.Models.Caching;

namespace SitecoreFundamentals.D365LeadContactCreator.Gateway
{
    public static class Caching
    {
        public static CachedPicklistList GetCachedPicklist(PicklistType picklistType)
        {
            var picklist = Picklists.FirstOrDefault(l => l.Name == picklistType);

            var cachedPicklist = CachedPicklistLists.FirstOrDefault(c => c.PicklistType == picklistType && c.Expires > DateTime.UtcNow);

            if (cachedPicklist != null)
                return cachedPicklist;

            using (var gateway = new D365Gateway())
            {
                // Calling this also populates the cache
                var listData = gateway.GetPicklistData(picklistType);

                return new CachedPicklistList()
                {
                    PicklistType = picklistType,
                    Items = listData
                };
            }
        }

        public static List<CachedPicklistList> CachedPicklistLists { get; set; } = new List<CachedPicklistList>();

        public static List<PicklistDefinition> Picklists { get; set; } = new List<PicklistDefinition>()
        {
            new PicklistDefinition()
            {
                Name = PicklistType.MarketingRegion,
                EntityName = PicklistEntityName.lead,
                AttributeName = "mnp_marketingregion"
            },
            new PicklistDefinition()
            {
                Name = PicklistType.MarketingNiche,
                EntityName = PicklistEntityName.lead,
                AttributeName = "mnp_marketingniche"
            }
        };
    }
}