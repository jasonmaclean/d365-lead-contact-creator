using System;
using System.Collections.Generic;

namespace SitecoreFundamentals.D365LeadContactCreator.Models
{
    public class Caching
    {
        public class AuthorizationCache
        {
            public DateTime Expires { get; set; }
            public string Token { get; set; }
        }

        [Serializable]
        public class CachedPicklistList
        {
            public PicklistType PicklistType { get; set; }
            public DateTime Expires { get; set; }
            public Dictionary<int, string> Items { get; set; }
        }

        public class PicklistDefinition
        {
            public PicklistType Name { get; set; }
            public PicklistEntityName EntityName { get; set; }
            public string AttributeName { get; set; }
        }
        public enum PicklistEntityName
        {
            contact,
            lead
        }

        public enum PicklistType
        {
            Language,
            PreferredContactMethod,
            MarketingRegion,
            LineOfService,
            MarketingNiche,
            AccountRole,
            RelatedPartnerTechnologies,
        }
    }
}