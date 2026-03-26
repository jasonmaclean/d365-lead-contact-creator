using Newtonsoft.Json;

namespace SitecoreFundamentals.D365LeadContactCreator.Models
{
    public class StandardFieldsBase
    {
        [JsonProperty("jobtitle")]
        public string JobTitle { get; set; }

        [JsonProperty("firstname")]
        public string FirstName { get; set; }

        [JsonProperty("lastname")]
        public string LastName { get; set; }

        [JsonProperty("emailaddress1")]
        public string Email { get; set; }

        [JsonProperty("telephone1")]
        public string BusinessPhone { get; set; }

        [JsonProperty("mobilephone")]
        public string MobilePhone { get; set; }

        [JsonProperty("address1_line1")]
        public string Address1 { get; set; }

        [JsonProperty("address1_line2")]
        public string Address2 { get; set; }

        [JsonProperty("address1_city")]
        public string City { get; set; }

        [JsonProperty("address1_stateorprovince")]
        public string Province { get; set; }

        [JsonProperty("address1_postalcode")]
        public string PostalCode { get; set; }

        [JsonProperty("address1_country")]
        public string Country { get; set; }
    }
}