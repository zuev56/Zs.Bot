﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zs.Common.Enums;
using Zs.Common.Services.Abstractions;
using Zs.Common.Services.Scheduler;

namespace Zs.Tests.Integration.Bot
{
    [TestClass]
    public class SchedulerTest : DataBaseClient
    {

        [TestMethod]
        public async Task Scheduler_Test()
        {
            try
            {
                var scheduler = new Scheduler();
                scheduler.Jobs.AddRange(GetJobs());

                Exception exception = null;
                await Task.Run(async () =>
                {
                    scheduler.Start(0, 500);
                    await Task.Delay(10000);
                    scheduler.Stop();
                }
                ).ContinueWith(task =>
                    exception = task.Exception
                );

                if (exception != null)
                    throw exception;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private IEnumerable<IJobBase> GetJobs()
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
                "SELECT Count(*) FROM bot.users", 
                _connectionString);

            return new List<IJobBase> { job0, job1, job2, job3, job4, job5 };
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
