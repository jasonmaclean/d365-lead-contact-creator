using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SitecoreFundamentals.D365LeadContactCreator.Gateway
{
    public partial class D365Gateway
    {
        private void TrimProperties<T>(T entity) where T : class
        {
            if (entity == null)
                return;

            var properties = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.PropertyType != typeof(string) || !property.CanRead || !property.CanWrite)
                    continue;

                var value = property.GetValue(entity) as string;

                if (string.IsNullOrWhiteSpace(value))
                    continue;

                var maxLengthAttr = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.MaxLengthAttribute), false).FirstOrDefault() as System.ComponentModel.DataAnnotations.MaxLengthAttribute;

                if (maxLengthAttr != null && value.Length > maxLengthAttr.Length)
                {
                    var trimmedValue = value.Substring(0, maxLengthAttr.Length);
                    property.SetValue(entity, trimmedValue);
                }
            }
        }
    }
}