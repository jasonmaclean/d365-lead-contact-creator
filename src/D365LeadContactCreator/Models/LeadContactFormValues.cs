namespace SitecoreFundamentals.D365LeadContactCreator.Models
{
    public class LeadContactFormValues : StandardFieldsBase
    {
        public string CreationPreference { get; set; }

        /// <summary>
        /// The following fields are specific to the Lead, and some are used by Contact if needed.
        /// </summary>
        public string OpportunityName { get; set; }

        // The following fields are mapped to option sets in D365 Lead entity
        public string Language { get; set; }
        public string MarketingRegion { get; set; }
        public string MarketingNiche { get; set; }
        public string PreferredContactMethod { get; set; }
    }

    public enum CreationPreference
    {
        ContactAndLead,
        ContactOnly,
        LeadOnly
    }

    public enum Language
    {
        English,
        French
    }

    public enum PreferredContactMethod
    {
        Any,
        Email,
        Phone,
        Fax,
        Mail
    }
}