using Microsoft.Extensions.Configuration;

namespace VkBotCore.Utils
{
	public static class ConfigurationExtended
	{
		public static T[] GetArray<T>(this IConfiguration configuration, string key, T[] defaultValue = null)
		{
			return configuration.GetSection(key)?.Get<T[]>() ?? defaultValue ?? new T[0];
		}
	}
}
