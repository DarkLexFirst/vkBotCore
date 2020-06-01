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
			T? val = Chat.Storage._settings.GetValue<T>(settingName);
			return val == null ? defaultValue : val.Value;
		}

		public string Get(string settingName, string defaultValue)
		{
			return Chat.Storage._settings[settingName] ?? defaultValue;
		}

		public void Set<T>(string settingName, T value) where T : struct
		{
			Chat.Storage._settings.SetValue(settingName, value);
		}

		public void Set(string settingName, string value)
		{
			Chat.Storage._settings[settingName] = value;
		}
	}
}
