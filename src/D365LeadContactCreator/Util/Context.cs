using Sitecore.Configuration;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace SitecoreFundamentals.D365LeadContactCreator.Util
{
    public partial class Context
    {
        internal static string MasterOrWebDatabase()
        {
            var instanceRoles = InstanceRoles();

            if (InstanceRoles().Contains(SitecoreRole.ContentDelivery.Name.ToLower()))
                return "web";

            if (InstanceRoles().Contains(SitecoreRole.ContentManagement.Name.ToLower()) || InstanceRoles().Contains(SitecoreRole.Standalone.Name.ToLower()))
                return "master";

            return string.Empty;
        }

        private static List<string> InstanceRoles()
        {
            var appSetting = ConfigurationManager.AppSettings["role:define"];
            List<string> rolesFromAppSettings = appSetting.Split("|,;"
                .ToCharArray())
                .Select(r => Regex.Match(r, "^\\s*(\\S*)\\s*$").Groups[1].Value)
                .Where(s => s.Length > 0)
                .Select(x => x.ToLowerInvariant())
                .Distinct()
                .ToList();

            return rolesFromAppSettings;
        }
    }
}