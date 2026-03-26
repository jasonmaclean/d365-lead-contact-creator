using SitecoreFundamentals.D365LeadContactCreator.Gateway;
using SitecoreFundamentals.D365LeadContactCreator.Models;
using System;
using System.Threading.Tasks;
using static SitecoreFundamentals.D365LeadContactCreator.Models.Caching;

namespace SitecoreFundamentals.D365LeadContactCreator
{
    public partial class Helpers
    {
        public static void CreateLeadWithContact(LeadContactFormValues leadContactFormValues)
        {
            var service = new FireAndForgetService();
            service.StartAndForgetAsyncMethod(leadContactFormValues);
        }

        public static async Task<bool> CreateLeadWithContactAsync(LeadContactFormValues formValues)
        {
            if (formValues == null || string.IsNullOrWhiteSpace(formValues.Email))
                return false;

            var creationPreference = ParseCreationPreference(formValues.CreationPreference);

            using (D365Gateway gateway = new D365Gateway())
            {
                // Populate all data regardless of creation preference as fields are borrowed for both Lead and Contact
                var lead = new Lead()
                {
                    OpportunityName = formValues.OpportunityName,
                    FirstName = formValues.FirstName,
                    LastName = formValues.LastName,
                    Email = formValues.Email,
                    Address1 = formValues.Address1,
                    Address2 = formValues.Address2,
                    City = formValues.City,
                    Province = formValues.Province,
                    PostalCode = formValues.PostalCode,
                    Country = formValues.Country,
                    JobTitle = formValues.JobTitle,
                    BusinessPhone = formValues.BusinessPhone,
                    MobilePhone = formValues.MobilePhone,
                };

                lead.Language = await gateway.GetPicklistValueAsync(PicklistType.Language, ParseLanguage(formValues.Language).ToString(), false, false);

                if (!string.IsNullOrWhiteSpace(formValues.MarketingRegion))
                    lead.MarketingRegion = await gateway.GetPicklistValueAsync(PicklistType.MarketingRegion, formValues.MarketingRegion, false, false);

                if (!string.IsNullOrWhiteSpace(formValues.MarketingNiche))
                    lead.MarketingNiche = await gateway.GetPicklistValueAsync(PicklistType.MarketingNiche, formValues.MarketingNiche, false, false);

                var preferredContactMethod = ParsePreferredContactMethod(formValues.PreferredContactMethod).ToString();

                if (!string.IsNullOrWhiteSpace(preferredContactMethod))
                    lead.PreferredContactMethod = await gateway.GetPicklistValueAsync(PicklistType.PreferredContactMethod, preferredContactMethod, false, false);

                var existingContactId = await gateway.GetContactAsync(lead.Email);

                if (!string.IsNullOrWhiteSpace(existingContactId))
                {
                    lead.ParentContact = $"/contacts({existingContactId})";
                }
                else if (creationPreference == null || creationPreference == CreationPreference.ContactOnly || creationPreference == CreationPreference.ContactAndLead)
                {
                    var contact = new Contact()
                    {
                        JobTitle = lead.JobTitle,
                        FirstName = lead.FirstName,
                        LastName = lead.LastName,
                        Email = lead.Email,
                        BusinessPhone = lead.BusinessPhone,
                        MobilePhone = lead.MobilePhone,
                        Address1 = lead.Address1,
                        Address2 = lead.Address2,
                        City = lead.City,
                        Province = lead.Province,
                        PostalCode = lead.PostalCode,
                        Country = lead.Country,
                        Language = lead.Language,
                        MarketingRegion = lead.MarketingRegion,
                        MarketingNiche = lead.MarketingNiche,
                        PreferredContactMethod = lead.PreferredContactMethod
                    };

                    var newContactId = await gateway.CreateContactAsync(contact);

                    if (creationPreference == CreationPreference.ContactOnly)
                        return newContactId != null;

                    if (!string.IsNullOrWhiteSpace(newContactId))
                        lead.ParentContact = $"/contacts({newContactId})";
                }

                if (creationPreference == null || creationPreference == CreationPreference.LeadOnly || creationPreference == CreationPreference.ContactAndLead)
                    return await gateway.CreateLeadAsync(lead);

                return true;
            }
        }

        private static Language? ParseLanguage(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var normalized = input.Trim().ToLowerInvariant();

            if (normalized == "en")
                return Language.English;

            if (normalized == "fr")
                return Language.French;

            foreach (Language lang in Enum.GetValues(typeof(Language)))
            {
                if (string.Equals(lang.ToString(), input, StringComparison.OrdinalIgnoreCase))
                    return lang;
            }

            return null;
        }

        private static PreferredContactMethod? ParsePreferredContactMethod(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                foreach (PreferredContactMethod method in Enum.GetValues(typeof(PreferredContactMethod)))
                {
                    if (string.Equals(method.ToString(), value, StringComparison.OrdinalIgnoreCase))
                        return method;
                }
            }

            return null;
        }

        private static CreationPreference? ParseCreationPreference(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                value = value.Replace(" ", "").Trim();

                foreach (CreationPreference creationPreference in Enum.GetValues(typeof(CreationPreference)))
                {
                    if (string.Equals(creationPreference.ToString(), value, StringComparison.OrdinalIgnoreCase))
                        return creationPreference;
                }
            }

            return null;
        }

        public class FireAndForgetService
        {
            public async Task StartCreateLeadWithContactAsync(LeadContactFormValues leadContactFormValues)
            {
                await CreateLeadWithContactAsync(leadContactFormValues);
            }

            public void StartAndForgetAsyncMethod(LeadContactFormValues leadContactFormValues)
            {
                _ = Task.Run(async () => await StartCreateLeadWithContactAsync(leadContactFormValues));
            }
        }
    }
}