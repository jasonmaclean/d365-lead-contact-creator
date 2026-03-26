using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SitecoreFundamentals.D365LeadContactCreator.Models.Caching;

namespace SitecoreFundamentals.D365LeadContactCreator
{
    public class Controls
    {
        public class D365PicklistMarketingNiche : D365PicklistControl<PicklistType>
        {
            protected override PicklistType PicklistType => PicklistType.MarketingNiche;
        }

        public class D365PicklistMarketingRegion : D365PicklistControl<PicklistType>
        {
            protected override PicklistType PicklistType => PicklistType.MarketingRegion;
        }

        public class D365PicklistControl<TPicklistType> : D365SelectControl<TPicklistType>
        {
            protected virtual TPicklistType PicklistType { get; }

            protected override IEnumerable<KeyValuePair<string, string>> GetOptions()
            {
                var picklistTypeEnum = (PicklistType)(object)PicklistType;
                var cachedListData = Gateway.Caching.GetCachedPicklist(picklistTypeEnum);

                if (cachedListData != null && cachedListData.Items.Any())
                    return cachedListData.Items.Select(i => new KeyValuePair<string, string>(i.Value, i.Value));

                return Enumerable.Empty<KeyValuePair<string, string>>();
            }
        }

        public abstract class D365SelectControl<T> : Sitecore.Web.UI.HtmlControls.Control
        {
            protected abstract IEnumerable<KeyValuePair<string, string>> GetOptions();

            protected override void DoRender(System.Web.UI.HtmlTextWriter output)
            {
                var listOptions = GetOptions().OrderBy(x => x.Value).ToList();

                var sb = new StringBuilder();

                sb.AppendLine("<select" + ControlAttributes + ">");

                if (listOptions.Any())
                {
                    // Adding a value of 0 to no selection option as the form won't post empty strings so LoadPostData would not work correctly
                    if (!listOptions.Any(opt => string.IsNullOrEmpty(opt.Value)))
                        listOptions.Insert(0, new KeyValuePair<string, string>("0", ""));

                    foreach (var listOption in listOptions)
                    {
                        var selected = listOption.Value == Value ? "selected=\"selected\"" : string.Empty;
                        var value = string.IsNullOrWhiteSpace(listOption.Value) ? "0" : listOption.Value;

                        sb.AppendLine($"<option value=\"{value}\" {selected}>{listOption.Value}</option>");
                    }
                }
                else
                {
                    sb.AppendLine($"<option value=\"\">WARNING - no values found. Please check integration settings.</option>");
                }

                sb.AppendLine("</select>");

                output.Write(sb.ToString());
            }

            protected override bool LoadPostData(string value)
            {
                if (value == null)
                    return false;

                if (this.GetViewStateString("Value") != value)
                    Sitecore.Context.ClientPage.Modified = true;

                this.SetViewStateString("Value", value);

                return true;
            }
        }
    }
}