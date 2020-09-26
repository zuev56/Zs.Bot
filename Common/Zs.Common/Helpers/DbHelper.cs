using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;

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
            catch (Exception ex)
            {

                throw;
            }

            //try
            //{
            //    string connectionString;
            //    using (var context = _contextFactory.GetContext())
            //        connectionString = context.Database.GetDbConnection().ConnectionString;
            //
            //    using (var connection = new SqlConnection(connectionString))
            //    {
            //        Console.WriteLine("\nQuery data example:");
            //        Console.WriteLine("=========================================\n");
            //
            //        connection.Open();
            //        var sb = new StringBuilder();
            //        sb.Append("SELECT TOP 20 pc.Name as CategoryName, p.name as ProductName ");
            //        sb.Append("FROM [SalesLT].[ProductCategory] pc ");
            //        sb.Append("JOIN [SalesLT].[Product] p ");
            //        sb.Append("ON pc.productcategoryid = p.productcategoryid;");
            //        String sql = sb.ToString();
            //
            //        using (var command = new SqlCommand(sql, connection))
            //        {
            //            using (var reader = command.ExecuteReader())
            //            {
            //                while (reader.Read())
            //                {
            //                    Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (SqlException e)
            //{
            //    Console.WriteLine(e.ToString());
            //}
            //Console.WriteLine("\nDone. Press enter.");
            //Console.ReadLine();
        }
    }
}
