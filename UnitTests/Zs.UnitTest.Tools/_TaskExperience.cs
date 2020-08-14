using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Zs.UnitTest.Tools
{
    [TestClass]
    public class TaskExperience
    {
        private readonly object _locker = new object();
        public TaskExperience()
        {
        }

        [TestMethod]
        public void ThreadPool_Test()
        {
            try
            {
                int cyclesNumber = 40;
                bool preferLocal = false;

                Trace.WriteLine($"MainThread start: {Thread.CurrentThread.ManagedThreadId}");

                for (int i = 0; i < cyclesNumber; i++)
                {
                    ThreadPool.QueueUserWorkItem((string threadName) => 
                        HeavyTaskBody(threadName), 
                        $"ThreadPool{(i < 10 ? $"0{i}" : i.ToString())}", 
                        preferLocal
                    );
                }

                HeavyMethod(20);
                Trace.WriteLine($"MainThread finished: {Thread.CurrentThread.ManagedThreadId}");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public async Task Task_Test()
        {
            try
            {
                int cyclesNumber = 40;
                Trace.WriteLine($"MainThread start: {Thread.CurrentThread.ManagedThreadId}");

                for (int i = 0; i < cyclesNumber; i++)
                    new Task(
                        () => { HeavyTaskBody($"ThreadPool{(i < 10 ? $"0{i}" : i.ToString())}"); },
                        TaskCreationOptions.None
                        ).Start();


                HeavyMethod(20); 
                Trace.WriteLine($"MainThread finished: {Thread.CurrentThread.ManagedThreadId}");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public void Parallel_Test()
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void HeavyTaskBody(string threadName)
        {
            lock (_locker)
            {
                Trace.WriteLine($"{threadName} start: {Thread.CurrentThread.ManagedThreadId}");
                HeavyMethod(10);
                Trace.WriteLine($"{threadName} finish: {Thread.CurrentThread.ManagedThreadId}");
            }
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