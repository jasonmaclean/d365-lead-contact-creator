using Sitecore.Configuration;
using SitecoreFundamentals.D365LeadContactCreator.Models;
using System;

namespace SitecoreFundamentals.D365LeadContactCreator
{
    public partial class Helpers
    {
        public LeadContactFormValues GetD365LeadContactDetails()
        {
            var databaseName = Util.Context.MasterOrWebDatabase();

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new InvalidOperationException("Database name is not set based on role. Cannot initialize gateway.");
            }

            var contextDb = Factory.GetDatabase(databaseName);

            var d365FormSettings = contextDb.Items.GetItem(Constants.Templates.D365Gateway.D365FormSettings.ID);

            var leadContact = new LeadContactFormValues();

            if (d365FormSettings == null)
                return leadContact;

            leadContact.Language = d365FormSettings.Language.Name.ToString();
            leadContact.CreationPreference = d365FormSettings.Fields[Constants.Templates.D365Gateway.D365FormSettings.Data.Fields.CreationPreference]?.Value;
            leadContact.MarketingRegion = d365FormSettings.Fields[Constants.Templates.D365Gateway.D365FormSettings.Data.Fields.MarketingRegion]?.Value;
            leadContact.MarketingNiche = d365FormSettings.Fields[Constants.Templates.D365Gateway.D365FormSettings.Data.Fields.MarketingNiche]?.Value;

            return leadContact;
        }
    }
}