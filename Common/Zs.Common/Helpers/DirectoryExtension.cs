﻿using System;
using System.IO;
using System.Linq;

namespace Zs.Common.Helpers
{
    public static class Path
    {
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