using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;

namespace RealTimeKqlLibrary
{
    public static class ObjectToDictionaryHelper
    {
        public static IDictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            Dictionary<string, object> retVal = new Dictionary<string, object>();
            foreach (var propertyInfo in source.GetType().GetProperties(bindingAttr))
            {
                if (propertyInfo.PropertyType.Namespace.StartsWith("SIEMfx.Syslog"))
                {
                    if (propertyInfo.PropertyType.IsEnum)
                    {
                        retVal.Add(propertyInfo.Name, propertyInfo.GetValue(source, null).ToString());
                    }
                    else
                    {
                        // Convert every complex object type into Dictionary object
                        string serializedJson = JsonConvert.SerializeObject(
                            propertyInfo.GetValue(source, null),
                            Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });

                        object val = JsonConvert.DeserializeObject<IDictionary<string, object>>(serializedJson);
                        retVal.Add(propertyInfo.Name, val);
                    }
                }
                else
                {
                    object val = propertyInfo.GetValue(source, null);
                    if (val != null)
                    {
                        retVal.Add(propertyInfo.Name, val);
                    }
                }
            }

            return retVal;
        }
    }
}
