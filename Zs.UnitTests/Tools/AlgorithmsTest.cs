using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zs.Common.Enums;
using Zs.Common.Extensions;

namespace Zs.UnitTest.Tools
{
    [Flags]
    public enum EntityTypes
    {
        Undefined = -1,
        DbClass = 1,
        DbInterface = 2,
        WcfClass = 4,
        WcfInterface = 8,
        VmClass = 16,
        VmInterface = 32,
        DbEntities = DbClass | DbInterface,
        WcfEntities = WcfClass | WcfInterface,
        VmEntities = VmClass | VmInterface,
        AllClasses = DbClass | WcfClass | VmClass,
        AllInterfaces = DbInterface | WcfInterface | VmInterface,
        All = AllClasses | AllInterfaces
    }

    [TestClass]
    public class AlgorithmsTest
    {
        
        public AlgorithmsTest()
        {
        }

        [TestMethod]
        public void Flags_Test()
        {
            try
            {
                var flag1 = EntityTypes.DbClass | EntityTypes.WcfInterface;
                var flag2 = EntityTypes.VmEntities;
                var flag3 = EntityTypes.All;
                var flag4 = EntityTypes.AllClasses;
                var flag5 = EntityTypes.VmClass;
                var flag6 = EntityTypes.VmClass | EntityTypes.VmInterface;


                //var flags = Enum.
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public void EnumSafeParse_Test()
        {
            try
            {
                var parseResult1 = "VmEntities".SafeParse<EntityTypes>();
                var parseResult2 = "VmEntity".SafeParse<EntityTypes>();
                var parseResult3 = "AllClasses".SafeParse<EntityTypes>();

                var parseResult4 = "Info".SafeParse<LogType>();
                //var parseResult5 = "Inform".SafeParse<LogType>();

                Assert.AreEqual(parseResult1, EntityTypes.VmEntities);
                Assert.AreEqual(parseResult2, EntityTypes.Undefined);
                Assert.AreEqual(parseResult3, EntityTypes.AllClasses);
                Assert.AreEqual(parseResult4, LogType.Info);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public void EnumToSingleFlagList_Test()
        {
            try
            {
                var singleFlags1 = EntityTypes.AllInterfaces.ToSingleFlagList<EntityTypes>();
                var singleFlags2 = EntityTypes.All.ToSingleFlagList<EntityTypes>();
                
                Assert.IsTrue(singleFlags1.Count == 3
                    && singleFlags1.Contains(EntityTypes.DbInterface)
                    && singleFlags1.Contains(EntityTypes.VmInterface)
                    && singleFlags1.Contains(EntityTypes.WcfInterface));

                Assert.IsTrue(singleFlags2.Count == 6
                    && singleFlags2.Contains(EntityTypes.DbInterface)
                    && singleFlags2.Contains(EntityTypes.VmInterface)
                    && singleFlags2.Contains(EntityTypes.WcfInterface)
                    && singleFlags2.Contains(EntityTypes.DbClass)
                    && singleFlags2.Contains(EntityTypes.VmClass)
                    && singleFlags2.Contains(EntityTypes.WcfClass));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public void JsonParse_Test()
        {
            try
            {
                var filePath = @"M:\Zs.Bot\Zs.Bot.Model\DbModel\ReferenceTables.json";
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("Не удалось найти файл", filePath);
                var fileContent = File.ReadAllText(filePath);

                foreach (var jItem in JArray.Parse(fileContent))
                {
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [TestMethod]
        public void JsonSerialize_Test()
        {
            try
            {
                //int hash = "{\"value\": \"UnitTest1\"}".GetHashCode();

                var innerException = new Exception("Inner");
                var exception = new Exception("Test", innerException);


                //var jSettings = new JsonSerializerSettings()
                //{
                //    NullValueHandling = NullValueHandling.Ignore,
                //    //MissingMemberHandling = MissingMemberHandling.Ignore,
                //    //Formatting = Formatting.Indented
                //};
                ////var jObject = JObject.FromObject(exception, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore });
                //var jsonData = JsonConvert.SerializeObject(exception, Formatting.Indented, jSettings);

                var options = new System.Text.Json.JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    WriteIndented = true
                };
                var jsonData = System.Text.Json.JsonSerializer.Serialize(exception, options);

                Assert.IsNotNull(jsonData);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
}
