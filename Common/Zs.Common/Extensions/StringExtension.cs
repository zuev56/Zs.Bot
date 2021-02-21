using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Zs.Common.Extensions
{
    public static class StringExtension
    {
        private static readonly JsonSerializerOptions prettyJsonSerializerOptions = new JsonSerializerOptions() 
        { 
            WriteIndented = true, 
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
        };

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
            // TEST performance
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                var normalizedJsonElement = SortPropertiesAlphabetically(jsonElement);
                return JsonSerializer.Serialize(normalizedJsonElement, prettyJsonSerializerOptions);
            }
            else if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                var jsonArray = jsonElement.EnumerateArray().ToList();

                for (int i = 0; i < jsonArray.Count; i++)
                {
                    var normalizedItem = jsonArray[i].ToString().NormalizeJsonString();
                    jsonArray[i] = JsonSerializer.Deserialize<JsonElement>(normalizedItem);
                }
                return JsonSerializer.Serialize(jsonArray, prettyJsonSerializerOptions);
            }
            else
            {
                return json;
            }
        }

        private static JsonElement SortPropertiesAlphabetically(JsonElement original)
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
                        var jsonElement = SortPropertiesAlphabetically(properties[i].Value);
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
                if (elements.Count > 0 && IsObjectOrArray(elements[0]))
                {
                    for (int i = 0; i < elements.Count; i++)
                    {
                        elements[i] = SortPropertiesAlphabetically(elements[i]);
                    }
                }

                var bytes = JsonSerializer.SerializeToUtf8Bytes(elements);
                return JsonDocument.Parse(bytes).RootElement.Clone();
            }
            else
                return original;
        }

    }
}
