using System;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zs.Common.T4;

namespace Zs.UnitTest.Tools
{
    [TestClass]
    public class T4GeneratorTest
    {
        private static string _connectionString;

        public T4GeneratorTest()
        {
        }

        [ClassInitialize()]
        public static void Init(TestContext testContext)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile(@"M:\PrivateBotConfiguration.json", true, true).Build();

            _connectionString = configuration["ConnectionString"];
        }

        [TestMethod]
        public void GetDbVersion_Test()
        {
            try
            {
                var version = DbReader.GetVersion(_connectionString);

                Assert.IsNotNull(version);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public void GetDbSchemas_Test()
        {
            try
            {
                var dbInfo = DbReader.GetDbInfo(_connectionString);

                Assert.IsNotNull(dbInfo.Count > 0);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public void ReadJsonConfiguration_Test()
        {
            var parameterValue = ModelGenerator.GetConfigurationValue(@"M:\PrivateBotConfiguration.json", "ConnectionString");

            Assert.IsNotNull(parameterValue);
        }

        [TestMethod]
        public void UnderscoreToPascalCase_Test()
        {
            try
            {
                var underscore = new[] { "_Aa_bB_c_", "_a_b_cc", "aa_bbb_c", "a", "aa", "A_bb_cCc", "_a", "a_", " ", "_", "" };
                var pascaleCase = new[] { "AaBbC", "ABCc", "AaBbbC", "A", "Aa", "ABbCcc", "A", "A", "", "", "" };

                for (int i = 0; i < underscore.Length; i++)
                    Assert.AreEqual(pascaleCase[i], ModelGenerator.UnderscoreToPascalCase(underscore[i]));
            }
            catch (Exception ex)
            {
                throw;
            }
        }



    }
}
