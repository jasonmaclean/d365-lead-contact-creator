using Sitecore.Configuration;
using Sitecore.Data.Items;
using System;
using System.Net.Http;

namespace SitecoreFundamentals.D365LeadContactCreator.Gateway
{
    public class GatewayBase : IDisposable
    {
        public readonly HttpClient _httpClient;
        public Item GlobalConfigItem { get; set; }

        public GatewayBase(string configItemID)
        {
            _httpClient = new HttpClient();

            var databaseName = Util.Context.MasterOrWebDatabase();

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new InvalidOperationException("Database name is not set based on role. Cannot initialize gateway.");
            }

            var contextDb = Factory.GetDatabase(databaseName);

            GlobalConfigItem = contextDb.Items.GetItem(configItemID);

            if (GlobalConfigItem == null)
            {
                throw new InvalidOperationException($"Configuration item with ID {configItemID} not found.");
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}