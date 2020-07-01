using System.Linq;
using Newtonsoft.Json.Linq;

namespace Zs.Common.Extensions
{
    public static class JsonExtension
    {
        public static string NormalizeJsonString(this string json)
        {
            try
            {
                var parsedObject = JObject.Parse(json);

                var normalizedObject = SortPropertiesAlphabetically(parsedObject);

                return Newtonsoft.Json.JsonConvert.SerializeObject(normalizedObject, Newtonsoft.Json.Formatting.Indented);

            }
            catch (System.Exception ex)
            {

                throw;
            }
        }

        private static JObject SortPropertiesAlphabetically(JObject original)
        {
            var result = new JObject();

            foreach (var property in original.Properties().ToList().OrderBy(p => p.Name))
            {
                var value = property.Value as JObject;

                if (value != null)
                {
                    value = SortPropertiesAlphabetically(value);
                    result.Add(property.Name, value);
                }
                else
                    result.Add(property.Name, property.Value);
            }

            return result;
        }
    }
}
