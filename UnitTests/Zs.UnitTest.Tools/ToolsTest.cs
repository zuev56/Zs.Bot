using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zs.Common.Extensions;

namespace Zs.UnitTest.Tools
{

    [TestClass]
    public class ToolsTest
    {
        private string _unPrettyJson;

        public ToolsTest()
        {

            _unPrettyJson = File.ReadAllText(@"S:\UnprettyJson.json");
        }

        [TestMethod]
        public void NormalizeJsonTest()
        {
            try
            {
                int count1 = 20, count2 = 1000;
                long[] t1 = new long[count1], t2 = new long[count1];

                var stopWatch = new Stopwatch();
                string s1 = null, s2 = null;

                for (int j = 0; j < count1 / 2; j++)
                {
                    stopWatch.Start();
                    for (int i = 0; i < count2; i++)
                    {
                        s1 = _unPrettyJson.NormalizeJsonString();
                    }
                    stopWatch.Stop();
                    t1[j] = stopWatch.ElapsedMilliseconds;

                    stopWatch.Reset();
                    Task.Delay(1000);

                    stopWatch.Start();
                    for (int i = 0; i < count2; i++)
                    {
                        s2 = _unPrettyJson.NormalizeJsonString_2();
                    }
                    stopWatch.Stop();
                    t2[j] = stopWatch.ElapsedMilliseconds;
                }

                stopWatch.Reset();
                for (int j = count1 / 2; j < count1; j++)
                {
                    stopWatch.Start();
                    for (int i = 0; i < count2; i++)
                    {
                        s1 = _unPrettyJson.NormalizeJsonString();
                    }
                    stopWatch.Stop();
                    t1[j] = stopWatch.ElapsedMilliseconds;

                    stopWatch.Reset();
                    Task.Delay(1000);

                    stopWatch.Start();
                    for (int i = 0; i < count2; i++)
                    {
                        s2 = _unPrettyJson.NormalizeJsonString_2();
                    }
                    stopWatch.Stop();
                    t2[j] = stopWatch.ElapsedMilliseconds;
                }

                for (int i = 0; i < count1; i++)
                {
                    Trace.WriteLine($"t1[{i}] = {t1[i]}    t2[{i}] = {t2[i]}");
                }
                Trace.WriteLine($"t1.Avg = {t1.Sum() / t1.Length}    t2.Avg = {t2.Sum() / t2.Length}");
                
                bool eq = s1 == s2;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
}
