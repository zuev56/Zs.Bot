using System;
using System.IO;
using System.Linq;

namespace Zs.Common.Extensions
{
    public static class Path
    {
        /// <summary>
        /// NOT USE
        /// </summary>
        /// <returns></returns>
        [Obsolete("Makes problems in production. For development only")]
        public static string TryGetSolutionPath()
        {
            var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            
            while (directoryInfo?.GetFiles("*.sln").Any() != true)
            {
                directoryInfo = directoryInfo.Parent;
            }

            return directoryInfo?.FullName;
        }
    }
}
