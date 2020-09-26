using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Zs.Common.Extensions
{
    public static class AssemblyExtension
    {
        public static string ReadResource(this Assembly assembly, string resourceName)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));

            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException($"'{nameof(resourceName)}' cannot be null or whitespace", nameof(resourceName));

            var resourcePath = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(resourceName));

            using var stream = assembly.GetManifestResourceStream(resourcePath);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
