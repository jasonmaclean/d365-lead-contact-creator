using Microsoft.Identity.Client;
using Sitecore.Diagnostics;
using SitecoreFundamentals.D365LeadContactCreator.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using static SitecoreFundamentals.D365LeadContactCreator.Models.Caching;

namespace SitecoreFundamentals.D365LeadContactCreator.Gateway
{
    public partial class D365Gateway : GatewayBase
    {
        private static AuthorizationCache _authCache;
        private static readonly object _authCacheLock = new object();

        public D365Gateway() : base(Constants.Items.D365IntegrationSettings.ID)
        {
            var d365Resource = Sitecore.Configuration.Settings.GetSetting("SitecoreFundamentals.D365.D365Resource").TrimEnd('/');
            var baseAddress = Sitecore.Configuration.Settings.GetSetting("SitecoreFundamentals.D365.BaseAddress");

            _httpClient.BaseAddress = new Uri($"{d365Resource}{baseAddress}");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private bool SetAuthorizationHeader()
            => SetAuthorizationHeaderAsync(true).Result;

        private async Task<bool> SetAuthorizationHeaderAsync()
            => await SetAuthorizationHeaderAsync(false);

        private async Task<bool> SetAuthorizationHeaderAsync(bool useSynchronous)
        {
            if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
                return true;

            var accessToken = useSynchronous ? GetAccessToken() : await GetAccessTokenAsync();

            if (string.IsNullOrWhiteSpace(accessToken))
                return false;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return true;
        }

        private string GetAccessToken()
            => GetAccessTokenAsync(true).GetAwaiter().GetResult();

        private async Task<string> GetAccessTokenAsync()
            => await GetAccessTokenAsync(false);

        private async Task<string> GetAccessTokenAsync(bool useSynchronous)
        {
            // Add a 1-minute buffer to avoid using a token that's about to expire
            var buffer = TimeSpan.FromMinutes(1);

            lock (_authCacheLock)
            {
                if (_authCache != null && _authCache.Expires > DateTime.UtcNow.Add(buffer) && !string.IsNullOrWhiteSpace(_authCache.Token))
                    return _authCache.Token;
            }

            var logPrefix = $"[{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}] ->";

            Log.Info($"{logPrefix} Fetching new access token.", this);

            var authCacheLifetimeMinutes = Sitecore.Configuration.Settings.GetIntSetting("SitecoreFundamentals.D365.Cache.AuthCacheLifetimeMinutes", 50);
            if (authCacheLifetimeMinutes < 2)
                authCacheLifetimeMinutes = 2;

            var clientId = GlobalConfigItem.Fields[Constants.Templates.D365Gateway.D365GatewaySettings.Data.Fields.ClientId]?.Value;
            var tenantId = GlobalConfigItem.Fields[Constants.Templates.D365Gateway.D365GatewaySettings.Data.Fields.TenantId]?.Value;
            var clientSecret = GlobalConfigItem.Fields[Constants.Templates.D365Gateway.D365GatewaySettings.Data.Fields.ClientSecret]?.Value;

            var authority = Sitecore.Configuration.Settings.GetSetting("SitecoreFundamentals.D365.Authority").TrimEnd('/');
            var d365Resource = Sitecore.Configuration.Settings.GetSetting("SitecoreFundamentals.D365.D365Resource").TrimEnd('/');

            var authorityWithTenantId = $"{authority}/{tenantId}";

            try
            {
                var app = ConfidentialClientApplicationBuilder.Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(new Uri(authorityWithTenantId))
                    .Build();

                var scopes = new[] { $"{d365Resource}/.default" };

                var authResult = useSynchronous ? app.AcquireTokenForClient(scopes).ExecuteAsync().Result : await app.AcquireTokenForClient(scopes).ExecuteAsync();

                if (!string.IsNullOrWhiteSpace(authResult.AccessToken))
                {
                    lock (_authCacheLock)
                    {
                        _authCache = new AuthorizationCache
                        {
                            Expires = DateTime.UtcNow.AddMinutes(authCacheLifetimeMinutes),
                            Token = authResult.AccessToken
                        };
                    }
                    Log.Info($"{logPrefix} Acquired and caching new access token which will expire {_authCache.Expires}", this);
                }

                return authResult.AccessToken;
            }
            catch (Exception ex)
            {
                Log.Error($"{logPrefix} {ex}", this);
                return "";
            }
        }

        internal async Task<bool> CreateLeadAsync(Lead lead)
        {
            try
            {
                if (await SetAuthorizationHeaderAsync() == false)
                    return false;

                var logPrefix = $"[{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}] ->";

                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(lead), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("leads", content);

                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var requestContent = Newtonsoft.Json.JsonConvert.SerializeObject(lead);
                    Log.Error($"{logPrefix} Failed to create lead. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Request: {requestContent}. Response: {responseContent}", this);
                    return false;
                }
                else
                {
                    Log.Info($"{logPrefix} Lead created. OpportunityName: '{lead.OpportunityName}', Email: {lead.Email}, ParentContact: '{lead.ParentContact}'", this);
                }

                return true;
            }
            catch (Exception ex)
            {
                var logPrefix = $"[{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}] ->";

                Log.Error($"{logPrefix} {ex}", this);

                return false;
            }
        }

        /// <summary>
        /// This will return the ID of an existing contact as a string.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<string> GetContactAsync(string email)
        {
            try
            {
                if (await SetAuthorizationHeaderAsync() == false)
                    return null;

                var fetchContactUrl = $"contacts?$select=contactid&$filter=emailaddress1 eq '{email.Replace("'", "''")}'";

                var contactResponse = await _httpClient.GetAsync(fetchContactUrl);

                if (contactResponse.IsSuccessStatusCode)
                {
                    var contactJson = await contactResponse.Content.ReadAsStringAsync();
                    dynamic contactResult = Newtonsoft.Json.JsonConvert.DeserializeObject(contactJson);

                    if (contactResult.value != null && contactResult.value.Count > 0)
                        return $"{contactResult.value[0].contactid}";
                }
            }
            catch (Exception ex)
            {
                var logPrefix = $"[{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}] ->";

                Log.Error($"{logPrefix} {ex}", this);

                return null;
            }

            return null;
        }

        /// <summary>
        /// This will return the ID of a new contact as a string.
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        internal async Task<string> CreateContactAsync(Contact contact)
        {
            try
            {
                if (await SetAuthorizationHeaderAsync() == false)
                    return null;

                var logPrefix = $"[{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}] ->";

                var contactContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(contact), System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("contacts", contactContent);

                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var requestContent = Newtonsoft.Json.JsonConvert.SerializeObject(contact);
                    Log.Error($"{logPrefix} Failed to create contact. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Request: {requestContent}. Response: {responseContent}", this);
                    return null;
                }
                else
                {
                    Log.Info($"{logPrefix} Contact created. Name: {contact.FirstName} {contact.LastName}, Email: {contact.Email}", this);
                }

                if (response.Headers.Contains("OData-EntityId"))
                {
                    var entityIdHeader = response.Headers.GetValues("OData-EntityId").FirstOrDefault();
                    if (!string.IsNullOrEmpty(entityIdHeader))
                    {
                        var idStart = entityIdHeader.LastIndexOf('(') + 1;
                        var idEnd = entityIdHeader.LastIndexOf(')');
                        if (idStart > 0 && idEnd > idStart)
                        {
                            return entityIdHeader.Substring(idStart, idEnd - idStart);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var logPrefix = $"[{GetType().FullName}.{MethodBase.GetCurrentMethod().Name}] ->";

                Log.Error($"{logPrefix} {ex}", this);

                return null;
            }

            return null;
        }
    }
}