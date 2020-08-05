using System.Linq;
using Newtonsoft.Json.Linq;

namespace Zs.Common.Extensions
{
    public static class JsonExtension
    {
        /// <summary> Sort parametres and make pretty JSON string </summary>
        public static string NormalizeJsonString(this string json)
        {
            try
            {
                var jTocken = JToken.Parse(json);

                if (jTocken is JObject)
                {
                    var parsedObject = JObject.Parse(json);
                    var normalizedObject = SortPropertiesAlphabetically(parsedObject);

                    return Newtonsoft.Json.JsonConvert.SerializeObject(normalizedObject, Newtonsoft.Json.Formatting.Indented);
                }
                else if (jTocken is JArray)
                {
                    var parsedArray = JArray.Parse(json);

                    for (int i = 0; i < parsedArray.Count; i++)
                    {
                        var normalizedItem = parsedArray[i].ToString().NormalizeJsonString();
                        parsedArray[i] = JToken.Parse(normalizedItem);
                    }
                    return Newtonsoft.Json.JsonConvert.SerializeObject(parsedArray, Newtonsoft.Json.Formatting.Indented);
                }
                else
                {
                    return json;
                }
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
