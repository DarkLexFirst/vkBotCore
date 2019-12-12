using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vkBotCore.Configuration
{
    public static class ConfigurationAddon
    {
        public static T[] GetArray<T>(this IConfiguration configuration, string key, T[] defaultValue = null)
        {
            return configuration.GetSection(key)?.Get<T[]>() ?? defaultValue ?? new T[0];
        }
    }
}
