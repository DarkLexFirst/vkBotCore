using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VkBotCore.Subjects
{
	public class ChatSettings
	{
		/// <summary>
		/// Чат, к которому привязано данное хранилище.
		/// </summary>
		public Chat Chat { get; private set; }

		public ChatSettings(Chat chat)
		{
			Chat = chat;
		}

		public T Get<T>(string settingName, T defaultValue) where T : struct
		{
			var val = Chat.Storage._settings[settingName];
			if (val == null) return defaultValue;
			return (T) val;
		}

		public T[] Get<T>(string settingName, T[] defaultValue) where T : struct
		{
			var val = Chat.Storage._settings[settingName];
			if (val == null) return defaultValue;
			return (T[]) val;
		}

		public string Get(string settingName, string defaultValue)
		{
			return (string) Chat.Storage._settings[settingName] ?? defaultValue;
		}

		public string[] Get(string settingName, string[] defaultValue)
		{
			return (string[]) Chat.Storage._settings[settingName] ?? defaultValue;
		}

		public void Set<T>(string settingName, T value) where T : struct
		{
			Chat.Storage._settings[settingName] = value;
		}

		public void Set<T>(string settingName, T[] value) where T : struct
		{
			Chat.Storage._settings[settingName] = value;
		}

		public void Set(string settingName, string value)
		{
			Chat.Storage._settings[settingName] = value;
		}

		public void Set(string settingName, string[] value)
		{
			Chat.Storage._settings[settingName] = value;
		}
	}
}
