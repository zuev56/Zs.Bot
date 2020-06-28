using System;
using System.Diagnostics;
using System.Threading;
using Npgsql;

namespace Zs.Common.Modules.CycleWorker
{
    /// <summary>
    /// <see cref="Job"/> based on SQL script
    /// </summary>
    public sealed class SqlJob : Job
    {
        private readonly string _connectionString;
        private readonly string _sqlQuery;

        public SqlJob(TimeSpan period, string sqlQuery, string connectionString)
        {
            Period = period != default ? period : throw new ArgumentException($"{nameof(period)} can't have default value");

            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(sqlQuery));
            _sqlQuery = sqlQuery ?? throw new ArgumentNullException(nameof(sqlQuery));
        }

        protected override void JobBody()
        {
#if DEBUG
            Trace.WriteLine($"SqlJobBody: {_sqlQuery} [{Counter}], ThreadId: {Thread.CurrentThread.ManagedThreadId}");
#endif
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            if (!string.IsNullOrWhiteSpace(_sqlQuery))
            {
                using var command = new NpgsqlCommand(_sqlQuery, connection);
                //command.Parameters.AddWithValue("p", "some_value");
                LastResult = command.ExecuteNonQuery();
            }
        }
    }
}
