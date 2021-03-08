using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Zs.Common.Exceptions;

namespace Zs.Common.Extensions
{
    public static class ConfigurationExtension
    {
        /// <summary>
        /// Gets a configuration value.
        /// If the value has {interpolation expression}, the method tries to replace it 
        /// with matching value in [Secrets] section. Otherwise it returns raw value
        /// </summary>
        public static string GetSecretValue(this IConfiguration configuration, string key)
        {
            if (configuration is null)
                throw new NullReferenceException(nameof(configuration));

            if (key is null)
                throw new ArgumentNullException(nameof(key));

            var value = configuration[key];
            if (value != null && Regex.IsMatch(value, @"\{(.*?)\}"))
            {
                var nodeWithValueToReplace = configuration.GetSection($"Secrets:{Regex.Match(value, @"(?<=\{)[^}]*(?=\})")?.Value}");

                value = nodeWithValueToReplace?.Value != null
                    ? Regex.Replace(value, @"\{(.*?)\}", nodeWithValueToReplace.Value)
                    : value;
            }

            return value;
        }

    }
}
