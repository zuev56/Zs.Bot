using Npgsql;
using System;

namespace Zs.Common.Helpers
{
    public static class DbHelper
    {
        public static string GetQueryResult(string connectionString, string sqlQuery)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString))
                    throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or empty", nameof(connectionString));

                if (string.IsNullOrEmpty(sqlQuery))
                    throw new ArgumentException($"'{nameof(sqlQuery)}' cannot be null or empty", nameof(sqlQuery));

                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                if (!string.IsNullOrWhiteSpace(sqlQuery))
                {
                    using var command = new NpgsqlCommand(sqlQuery, connection);
                    using var reader = command.ExecuteReader();
                    reader.Read();
                    return reader.GetString(0);
                }
                connection.Close();
                return null;
            }
            catch (InvalidCastException icex)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
