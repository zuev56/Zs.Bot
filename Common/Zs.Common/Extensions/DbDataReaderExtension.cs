using System.Collections.Generic;
using System.Data.Common;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Zs.Common.Extensions
{
    public static class DbDataReaderExtension
    {
        private static readonly JsonSerializerOptions prettyJsonSerializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static string ReadToJson(this DbDataReader reader)
        {
            var rows = new List<Dictionary<string, string>>();

            while (reader.Read())
            {
                var row = new Dictionary<string,string>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row.Add(reader.GetColumnSchema()[i].BaseColumnName, reader[i].ToString());

                rows.Add(row);
            }

            return JsonSerializer.Serialize(rows, prettyJsonSerializerOptions);
        }
    }
}
