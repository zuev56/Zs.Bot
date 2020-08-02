using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Npgsql;

namespace Zs.Common.Extensions
{
    public static class NpgsqlDataReaderExtension
    {
        public static string ReadToJson(this NpgsqlDataReader reader)
        {
            var rows = new List<Dictionary<string, string>>();

            while (reader.Read())
            {
                var row = new Dictionary<string,string>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row.Add(reader.GetColumnSchema()[i].BaseColumnName, reader[i].ToString());

                rows.Add(row);
            }

            return JsonConvert.SerializeObject(rows, Formatting.Indented);
        }
    }
}
