using Microsoft.Extensions.Configuration;

namespace VkBotCore.Utils
{
	public class CoreConfiguration
	{
		public IConfiguration Base { get; set; }

		public CoreConfiguration(IConfiguration configuration)
		{
			Base = configuration;
		}

		public string this[string key] { get => Base[key]; }

		public T[] GetArray<T>(string key)
		{
			return Base.GetArray(key, new T[0]);
		}

		public T[] GetArray<T>(string key, T[] defaultValue)
		{
			return Base.GetArray(key, defaultValue);
		}

		public T GetValue<T>(string key)
		{
			return Base.GetValue<T>(key);
		}

		public T GetValue<T>(string key, T defaultValue)
		{
			return Base.GetValue<T>(key, defaultValue);
		}
	}
}
