using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zs.Common.Enums;
using Zs.Common.Modules.CycleWorker;

namespace Zs.UnitTest.Bot
{
    [TestClass]
    public class CycleWorkerTest
    {
        //private readonly CycleWorker _cycleWorker = new CycleWorker();
        private readonly string _connectionString 
            = "Host=localhost;Port=5632;Username=app;Password=app;Database=ZsBot;";

        public CycleWorkerTest()
        {
        }

        [TestMethod]
        public async Task CycleWorker_Test()
        {
            try
            {
                var cycleWorker = new CycleWorker();
                cycleWorker.Jobs.AddRange(GetJobs());

                Exception exception = null;
                await Task.Run(async () =>
                {
                    cycleWorker.Start(0, 500);
                    await Task.Delay(10000);
                    cycleWorker.Stop();
                }
                ).ContinueWith(task => exception = task.Exception);
                
                if (exception != null)
                    throw exception;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IEnumerable<Job> GetJobs()
        {
            var job0 = new ProgramJob(TimeSpan.FromSeconds(1), () => HeavyMethod(2));
            var job1 = new ProgramJob(TimeSpan.FromSeconds(2), () => HeavyMethod(4));
            var job2 = new ProgramJob(TimeSpan.FromSeconds(3), () => HeavyMethod(6));

            var job3 = new SqlJob(
                TimeSpan.FromSeconds(1),
                QueryResultType.Double,
                "SELECT Count(*) FROM bot.logs", 
                _connectionString);
            var job4 = new SqlJob(
                TimeSpan.FromSeconds(2),
                QueryResultType.Double,
                "SELECT Count(*) FROM bot.messages", 
                _connectionString);
            var job5 = new SqlJob(
                TimeSpan.FromSeconds(3),
                QueryResultType.Double,
                "SELECT Count(*) FROM bot.options", 
                _connectionString);

            return new List<Job> { job0, job1, job2, job3, job4, job5 };
        }

        private void HeavyMethod(byte heavyLevel)
        {
            if (heavyLevel > 50)
                heavyLevel = 50;

            int d = heavyLevel * 10000;
            int count = 0;
            long a = 2;
            while (count < d)
            {
                long b = 2;
                int prime = 1;
                while (b * b <= a)
                {
                    if (a % b == 0)
                    {
                        prime = 0;
                        break;
                    }
                    b++;
                }
                if (prime > 0)
                {
                    count++;
                }
                a++;
            }
        }
    }
}
