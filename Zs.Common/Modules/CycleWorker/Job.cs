using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace Zs.Common.Modules.CycleWorker
{
    public abstract class Job
    {
        public long      Counter     { get; private set; }
        public bool      IsRunning   { get; protected set; }
        public DateTime? NextRunDate { get; protected set; }
        public DateTime? LastRunDate { get; protected set; }
        public TimeSpan  Period      { get; protected set; }
        public object    LastResult  { get; protected set; }

        protected abstract void JobBody();

        public Task Execute()
        {
            if (Period == default)
            {
                NextRunDate = LastRunDate = DateTime.MaxValue;
                Period = TimeSpan.MaxValue;
                throw new ArgumentException($"{nameof(Period)} can't have default value");
            }

            IsRunning = true;

            JobBody();

            LastRunDate = DateTime.Now;
            NextRunDate = DateTime.Now + Period;
            IsRunning = false;
            Counter++;
            return Task.CompletedTask;
        }

    }
    public sealed class ProgramJob : Job
    {
        private readonly Action _method;
        private readonly object _parameter;


        public ProgramJob(TimeSpan period, Action method, object parameter = null)
        {
            Period = period != default ? period : throw new ArgumentException($"{nameof(period)} can't have default value");

            _method = method ?? throw new ArgumentNullException(nameof(method));
            _parameter = parameter;
        }

        protected override void JobBody()
        {
#if DEBUG
            Trace.WriteLine($"ProgramJobBody: [{Counter}], ThreadId: {Thread.CurrentThread.ManagedThreadId}");
#endif
            _method.Invoke();
        }
    }

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
