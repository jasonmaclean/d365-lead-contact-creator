using Newtonsoft.Json.Linq;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static SitecoreFundamentals.D365LeadContactCreator.Models.Caching;

namespace SitecoreFundamentals.D365LeadContactCreator.Gateway
{
    public partial class D365Gateway
    {
        public async Task<int?> GetPicklistValueAsync(PicklistType picklistType, string label, bool isRetryAfterPopulatingCache, bool useContainsQuery)
        {
            if (label == "0")
                return null;

            var picklist = Caching.Picklists.FirstOrDefault(l => l.Name == picklistType);

            var cachedPicklist = Caching.CachedPicklistLists.FirstOrDefault(c => c.PicklistType == picklistType && c.Expires > DateTime.UtcNow);

            if (cachedPicklist != null)
            {
                KeyValuePair<int, string> matchedItem;

                if (useContainsQuery)
                {
                    matchedItem = cachedPicklist.Items.FirstOrDefault(i => i.Value.IndexOf(label, StringComparison.OrdinalIgnoreCase) >= 0);
                }
                else
                {
                    matchedItem = cachedPicklist.Items.FirstOrDefault(i => i.Value.Equals(label, StringComparison.OrdinalIgnoreCase));
                }

                if (matchedItem.Key != 0)
                    return matchedItem.Key;
            }

            var picklistData = await GetPicklistDataAsync(picklistType);

            // Try again after populating the cache
            if (!isRetryAfterPopulatingCache)
                return await GetPicklistValueAsync(picklistType, label, true, useContainsQuery);

            return null;
        }

        public Dictionary<int, string> GetPicklistData(PicklistType picklistType)
            => GetPicklistDataAsync(picklistType, true).GetAwaiter().GetResult();

        public async Task<Dictionary<int, string>> GetPicklistDataAsync(PicklistType picklistType)
            => await GetPicklistDataAsync(picklistType, false);

        private async Task<Dictionary<int, string>> GetPicklistDataAsync(PicklistType picklistType, bool useSynchronous)
        {
            var picklist = Caching.Picklists.FirstOrDefault(l => l.Name == picklistType);

            var logPrefix = $"[{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}] ->";

            try
            {
                var authHeaderResult = useSynchronous ? SetAuthorizationHeader() : await SetAuthorizationHeaderAsync();

                if (!authHeaderResult)
                    return new Dictionary<int, string>();

                var url = $"EntityDefinitions(LogicalName='{picklist.EntityName.ToString()}')/Attributes(LogicalName='{picklist.AttributeName}')/Microsoft.Dynamics.CRM.PicklistAttributeMetadata?$select=LogicalName&$expand=GlobalOptionSet($select=Options)";

                var response = useSynchronous ? _httpClient.GetAsync(url).GetAwaiter().GetResult() : await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                var json = useSynchronous ? response.Content.ReadAsStringAsync().Result : await response.Content.ReadAsStringAsync();

                var data = JObject.Parse(json);

                if (data == null)
                {
                    Log.Warn($"{logPrefix} No data returned from lookup query.", this);
                    return new Dictionary<int, string>();
                }

                var options = data["GlobalOptionSet"]?["Options"];

                if (options == null)
                    return new Dictionary<int, string>();

                var picklistCacheLifetimeMinutes = Sitecore.Configuration.Settings.GetIntSetting("SitecoreFundamentals.D365.Cache.PicklistCacheLifetimeMinutes", 50);
                if (picklistCacheLifetimeMinutes < 2)
                    picklistCacheLifetimeMinutes = 2;

                var cachedList = new CachedPicklistList()
                {
                    PicklistType = picklistType,
                    Expires = DateTime.UtcNow.AddMinutes(picklistCacheLifetimeMinutes),
                    Items = new Dictionary<int, string>()
                };

                foreach (var option in options)
                {
                    var optionLabel = option["Label"]?["UserLocalizedLabel"]?["Label"]?.ToString();
                    var optionValue = option.Value<int>("Value");
                    if (!string.IsNullOrWhiteSpace(optionLabel))
                        cachedList.Items.Add(optionValue, optionLabel);
                }

                Log.Info($"{logPrefix} Storing {cachedList.Items.Count} items in picklist cache for {picklistType.ToString()} for {picklistCacheLifetimeMinutes} minutes.", this);

                Caching.CachedPicklistLists.RemoveAll(c => c.PicklistType == picklistType);
                Caching.CachedPicklistLists.Add(cachedList);

                var cacheSizeKb = GetObjectSize(Caching.CachedPicklistLists);
                Log.Info($"{logPrefix} CachedPicklistLists size is now: {cacheSizeKb} KB", this);

                return cachedList.Items;
            }
            catch (Exception ex)
            {
                Log.Error($"{logPrefix} {ex}", this);
            }

            return new Dictionary<int, string>();
        }

        private string GetObjectSize(object obj)
        {
            if (obj == null) return "0 bytes";
            try
            {
                using (var ms = new System.IO.MemoryStream())
                {
                    var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    formatter.Serialize(ms, obj);
                    long size = ms.Length;

                    if (size >= 1024)
                        return $"{(size / 1024.0):0.##} KB";

                    return $"{size} bytes";
                }
            }
            catch
            {
                return "error getting size";
            }
        }
    }
}