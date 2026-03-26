using Newtonsoft.Json;

namespace SitecoreFundamentals.D365LeadContactCreator.Models
{
    internal class Lead : LeadContactBase
    {
        [JsonProperty("subject")]
        public string OpportunityName { get; set; }

        [JsonProperty("parentcontactid@odata.bind")]
        public string ParentContact { get; set; }
    }
}