using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Zs.Common.Extensions
{
    public static class StringExtension
    {
        private static readonly JsonSerializerOptions prettyJsonSerializerOptions
            = new JsonSerializerOptions() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        // TODO: Replace Newtonsoft.Json with System.Text.Json

        public static string GetMD5Hash(this string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            using var md5Hash = MD5.Create();
            var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(value));
            var sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));

            return sBuilder.ToString();
        }

        public static IEnumerable<string> SplitToParts(this string value, int partLength)
        {
            if (value == null)
                throw new NullReferenceException();
            
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", nameof(partLength));

            for (var i = 0; i < value.Length; i += partLength)
                yield return value.Substring(i, Math.Min(partLength, value.Length - i));
        }

        public static string WithoutDigits(this string value)
        {
            if (value == null)
                throw new NullReferenceException();

            return Regex.Replace(value, @"[\d]", "");
        }

        /// <summary> Sort parametres and make pretty JSON string </summary>
        public static string NormalizeJsonString(this string json)
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

        // Test System.Text.Json
        // JObject  -> JsonElement
        public static string NormalizeJsonString_2(this string json)
        {
            // TEST performance
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                var normalizedJsonElement = SortPropertiesAlphabetically_2(jsonElement);
                return JsonSerializer.Serialize(normalizedJsonElement, prettyJsonSerializerOptions);
            }
            else if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                var jsonArray = jsonElement.EnumerateArray().ToList();

                for (int i = 0; i < jsonArray.Count; i++)
                {
                    var normalizedItem = jsonArray[i].ToString().NormalizeJsonString_2();
                    jsonArray[i] = JsonSerializer.Deserialize<JsonElement>(normalizedItem);
                }
                return JsonSerializer.Serialize(jsonArray, prettyJsonSerializerOptions);
            }
            else
            {
                return json;
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
        
        private static JsonElement SortPropertiesAlphabetically_2(JsonElement original)
        {
            bool IsObjectOrArray(JsonElement original) => original.ValueKind == JsonValueKind.Object || original.ValueKind == JsonValueKind.Array;

            if (original.ValueKind == JsonValueKind.Object)
            {
                var properties = original.EnumerateObject().OrderBy(p => p.Name).ToList();
                var keyValuePairs = new Dictionary<string, JsonElement>(properties.Count);
                for (int i = 0; i < properties.Count; i++)
                {
                    // Для составных объектов сначала вызываем этот метод
                    if (IsObjectOrArray(properties[i].Value))
                    {
                        var jsonElement = SortPropertiesAlphabetically_2(properties[i].Value);
                        keyValuePairs.Add(properties[i].Name, jsonElement);
                    }
                    else
                    {
                        keyValuePairs.Add(properties[i].Name, properties[i].Value);
                    }
                }
                var bytes = JsonSerializer.SerializeToUtf8Bytes(keyValuePairs);
                return JsonDocument.Parse(bytes).RootElement.Clone();
            }
            else if (original.ValueKind == JsonValueKind.Array)
            {
                var elements = original.EnumerateArray().ToList();

                // Если элементы массива - сложные объекты, то для каждого рекурсивно вызываем этот метод
                if (elements.Count > 0 && IsObjectOrArray(elements[0]))
                {
                    for (int i = 0; i < elements.Count; i++)
                    {
                        elements[i] = SortPropertiesAlphabetically_2(elements[i]);
                    }
                }
                // Примитивные элементы массива не сортируем

                var bytes = JsonSerializer.SerializeToUtf8Bytes(elements);
                return JsonDocument.Parse(bytes).RootElement.Clone();
            }
            else
                return original;
        }

    }
}
