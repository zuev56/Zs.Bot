namespace Zs.Common.Modules.Scheduler
{
    public interface IJobExecutionResult
    {
        string TextValue { get; }
    }

    public class JobExecutionResult<TResult> : IJobExecutionResult
    {
        private readonly TResult _result;

        public string TextValue => _result?.ToString();

        public JobExecutionResult(TResult result)
        {
            _result = result;
        }
    }

    //public class StringResult : IJobExecutionResult
    //{
    //    public string Result { get; private set; }
    //
    //    public StringResult(string result)
    //    {
    //        Result = result;
    //    }
    //    public string Show() => Result.ToString();
    //}
    //
    //public class IntResult : IJobExecutionResult
    //{
    //    public int Result { get; set; }
    //    public string Show() => Result.ToString();
    //}
    //
    //public class ObjectResult : IJobExecutionResult
    //{
    //    public object Result { get; set; }
    //    public string Show() => Result.ToString();
    //}
}
