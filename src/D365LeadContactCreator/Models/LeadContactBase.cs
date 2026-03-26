using Newtonsoft.Json;

namespace SitecoreFundamentals.D365LeadContactCreator.Models
{
    internal class LeadContactBase : StandardFieldsBase
    {
        [JsonProperty("mnp_language", NullValueHandling = NullValueHandling.Ignore)]
        public int? Language { get; set; }

        [JsonProperty("mnp_marketingregion", NullValueHandling = NullValueHandling.Ignore)]
        public int? MarketingRegion { get; set; }

        [JsonProperty("mnp_marketingniche", NullValueHandling = NullValueHandling.Ignore)]
        public int? MarketingNiche { get; set; }

        [JsonProperty("preferredcontactmethodcode", NullValueHandling = NullValueHandling.Ignore)]
        public int? PreferredContactMethod { get; set; }
    }
}