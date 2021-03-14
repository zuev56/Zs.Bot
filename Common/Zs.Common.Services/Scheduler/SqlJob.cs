using System;
using Microsoft.Extensions.Logging;
using Npgsql;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Extensions;
using Zs.Common.Models;
using Zs.Common.Services.Abstractions;

namespace Zs.Common.Services.Scheduler
{
    /// <summary>
    /// <see cref="Job"/> based on SQL script
    /// </summary>
    public sealed class SqlJob : Job<string>
    {
        private readonly string _connectionString;
        private readonly string _sqlQuery;
        private QueryResultType _resultType;


        public SqlJob(TimeSpan period,
            QueryResultType resultType,
            string sqlQuery,
            string connectionString,
            DateTime? startDate = null,
            string description = null,
            ILogger logger = null)
            : base(period, startDate, logger)
        {
            Period = period != default ? period : throw new ArgumentException($"{nameof(period)} can't have default value");

            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(sqlQuery));
            _resultType = resultType;
            _sqlQuery = sqlQuery ?? throw new ArgumentNullException(nameof(sqlQuery));
            Description = description;
        }

        protected override IServiceResult<string> GetExecutionResult()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                if (!string.IsNullOrWhiteSpace(_sqlQuery))
                {
                    using var command = new NpgsqlCommand(_sqlQuery, connection);
                    using var reader = command.ExecuteReader();
                    
                    switch (_resultType)
                    {
                        case QueryResultType.Double:
                            reader.Read();
                            LastResult = !reader.IsDBNull(0)
                                ? ServiceResult<string>.Success(reader.GetDouble(0).ToString())
                                : null;
                            break;
                        case QueryResultType.Json:
                            LastResult = ServiceResult<string>.Success(reader.ReadToJson());
                            break;
                        case QueryResultType.String:
                            reader.Read();
                            LastResult = !reader.IsDBNull(0)
                                ? ServiceResult<string>.Success(reader.GetString(0))
                                : null;
                            break;
                        default:
                            LastResult = ServiceResult<string>.Success(null);
                            break;
                    }
                }

                return LastResult;
            }
            catch
            {
                throw;
            }
        }
    }
}
