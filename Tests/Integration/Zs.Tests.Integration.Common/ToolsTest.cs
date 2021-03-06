﻿using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using Zs.Common.Extensions;
using Zs.Common.Services.Logging.Seq;

namespace Zs.Tests.Integration.Common
{

    [TestClass]
    public class ToolsTest
    {
        private string _unPrettyJson;
        private readonly IConfiguration _configuration;

        public ToolsTest()
        {
            _unPrettyJson = File.ReadAllText(@"S:\UnprettyJson.json");
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile(@"S:\Repos\Zs.Bot\Apps\Zs.App.Home\Zs.App.Home.Bot\appsettings.json");
            
            var tmpConfig = configurationBuilder.Build();
            if (tmpConfig["SecretsPath"] != null)
                configurationBuilder.AddJsonFile(tmpConfig["SecretsPath"]);

            _configuration = configurationBuilder.Build();

        }

        [TestMethod]
        public void NormalizeJsonTest()
        {
            //  try
            //  {
            //      int count1 = 20, count2 = 1000;
            //      long[] t1 = new long[count1], t2 = new long[count1];
            //
            //      var stopWatch = new Stopwatch();
            //      string s1 = null, s2 = null;
            //
            //      for (int j = 0; j < count1 / 2; j++)
            //      {
            //          stopWatch.Start();
            //          for (int i = 0; i < count2; i++)
            //          {
            //              s1 = _unPrettyJson.NormalizeJsonString();
            //          }
            //          stopWatch.Stop();
            //          t1[j] = stopWatch.ElapsedMilliseconds;
            //
            //          stopWatch.Reset();
            //          Task.Delay(1000);
            //
            //          stopWatch.Start();
            //          for (int i = 0; i < count2; i++)
            //          {
            //              s2 = _unPrettyJson.NormalizeJsonString_2();
            //          }
            //          stopWatch.Stop();
            //          t2[j] = stopWatch.ElapsedMilliseconds;
            //      }
            //
            //      stopWatch.Reset();
            //      for (int j = count1 / 2; j < count1; j++)
            //      {
            //          stopWatch.Start();
            //          for (int i = 0; i < count2; i++)
            //          {
            //              s1 = _unPrettyJson.NormalizeJsonString();
            //          }
            //          stopWatch.Stop();
            //          t1[j] = stopWatch.ElapsedMilliseconds;
            //
            //          stopWatch.Reset();
            //          Task.Delay(1000);
            //
            //          stopWatch.Start();
            //          for (int i = 0; i < count2; i++)
            //          {
            //              s2 = _unPrettyJson.NormalizeJsonString_2();
            //          }
            //          stopWatch.Stop();
            //          t2[j] = stopWatch.ElapsedMilliseconds;
            //      }
            //
            //      for (int i = 0; i < count1; i++)
            //      {
            //          Trace.WriteLine($"t1[{i}] = {t1[i]}    t2[{i}] = {t2[i]}");
            //      }
            //      Trace.WriteLine($"t1.Avg = {t1.Sum() / t1.Length}    t2.Avg = {t2.Sum() / t2.Length}");
            //      
            //      bool eq = s1 == s2;
            //  }
            //  catch (Exception ex)
            //  {
            //      throw;
            //  }
        }

        [TestMethod]
        public void SeqServiceTest()
        {
            SeqService seq = new SeqService("http://localhost:5341/", _configuration.GetSecretValue("Seq:ApiToken"));

            var t = seq.GetLastEvents(10, 39, 41);

        }
        
    }
}
