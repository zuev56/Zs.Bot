using System;
using Npgsql;
using Zs.Common.Enums;
using Zs.Common.Extensions;

namespace Zs.Common.Modules.Scheduler
{
    /// <summary>
    /// <see cref="Job"/> based on SQL script
    /// </summary>
    public sealed class SqlJob : Job
    {
        // Надо определять тип возвращаемого значения при создании  джоба
        // Это может быть пустой тип
        // Дженерик не подходит, т.к. список джобов должен в конечном счёте иметь один тип
        // Вероятно, стоит вернуть JobExecutionResult
        private readonly string _connectionString;
        private readonly string _sqlQuery;
        private QueryResultType _resultType;


        public SqlJob(TimeSpan period,
            QueryResultType resultType,
            string sqlQuery,
            string connectionString,
            DateTime? startDate = null,
            string description = null)
            : base(period, startDate)
        {
            Period = period != default ? period : throw new ArgumentException($"{nameof(period)} can't have default value");

            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(sqlQuery));
            _resultType = resultType;
            _sqlQuery = sqlQuery ?? throw new ArgumentNullException(nameof(sqlQuery));
            Description = description;
        }

        protected override IJobExecutionResult GetExecutionResult()
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
                                ? new JobExecutionResult<double>(reader.GetDouble(0))
                                : null;
                            break;
                        case QueryResultType.Json:
                            LastResult = new JobExecutionResult<string>(reader.ReadToJson());
                            break;
                        case QueryResultType.String:
                            reader.Read();
                            LastResult = !reader.IsDBNull(0)
                                ? new JobExecutionResult<string>(reader.GetString(0))
                                : null;
                            break;
                        default:
                            LastResult = null;
                            break;
                    }
                }

                return LastResult;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
