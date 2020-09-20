using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zs.Common.Enums;
using Zs.Common.Extensions;
using Zs.Common.Helpers;

namespace Zs.UnitTest.Tools
{

    [TestClass]
    public class ToolsTest
    {
        [TestMethod]
        public void GetSolutionPath_Test()
        {
            try
            {
                var solutionPath = Path.TryGetSolutionPath();

                Assert.IsNotNull(solutionPath);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
