namespace SitecoreFundamentals.D365LeadContactCreator
{
    public class Constants
    {
        public static class Items
        {
            public static class D365CommonFormSettings
            {
                public static readonly string ID = "{D13C2856-63DC-4006-AAF5-9F488DE4D2AC}";
            }
            public static class D365IntegrationSettings
            {
                public static readonly string ID = "{94BD5384-DE21-473F-A9E5-764F3998C959}";
            }
        }

        public class Templates
        {
            public static class D365Gateway
            {
                public static class D365GatewaySettings
                {
                    public static readonly string ID = "{D9B8B1EE-C100-41A5-9900-036D0D7238DC}";
                    public class Data
                    {
                        public static class Fields
                        {
                            public static readonly string ClientId = "{55AB5826-566D-4764-9D89-0AA5C94C0C18}";
                            public static readonly string ClientSecret = "{38226DA0-A483-409F-AAA0-F5075DD414BD}";
                            public static readonly string TenantId = "{57A3DE12-7AEE-4FE2-9662-92F6DBB5C6A9}";
                        }
                    }
                }

                public static class D365FormSettings
                {
                    public static readonly string ID = "{4BE59854-B69B-40CD-A424-F894A0AD7EB1}";
                    public class Data
                    {
                        public static class Fields
                        {
                            public static readonly string Enabled = "{4432A64A-F040-4F23-A6D3-1F75B2DA33D1}";
                            public static readonly string CreationPreference = "{1D7ABB44-809B-4E33-98FD-491D0812B57B}";
                            public static readonly string MarketingNiche = "{2E71E92B-7760-41BA-BE09-37CE9A154CD5}";
                            public static readonly string MarketingRegion = "{F9727F7B-9863-4E98-A74D-427F1ACC485D}";
                        }
                    }
                }
            }
        }
    }
}