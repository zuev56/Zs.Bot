using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Zs.Common.T4
{
    internal sealed class T4Logger
    {
        public static void TraceException(Exception ex)
        {
            var jsonData = JsonConvert.SerializeObject(ex, Formatting.Indented);
            Trace.WriteLine(jsonData);
        }
    }
}
