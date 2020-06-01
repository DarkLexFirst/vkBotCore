using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;

namespace VkBotCore.Subjects
{
	public class ChatStorage
	{
		[JsonIgnore]
		public Chat Chat { get; private set; }

		[JsonIgnore]
		private string _cache;

		[JsonProperty("Settings")]
		internal VariableSet _settings = new VariableSet();

		public VariableSet Variables { get; } = new VariableSet();
		public UsersStorageSet UsersStorage { get; } = new UsersStorageSet();

		private const string BasePath = "ChatsStorage";

		private DateTime _lastSaveTime = DateTime.Now;
		private TimeSpan _timeToSave = new TimeSpan(0, 5, 0);

		private static string GetDirectoryPath(Chat chat)
		{
			return Path.Combine(BasePath, $"group_{chat.VkApi.GroupId}");
		}

		private static string GetFullFilePath(Chat chat)
		{
			return Path.Combine(GetDirectoryPath(chat), $"chat_{chat.PeerId}.json");
		}

		internal static ChatStorage ReadFromJson(Chat chat)
		{
			string path = GetFullFilePath(chat);
			ChatStorage storage = null;
			if (File.Exists(path))
			{
				var settings = new JsonSerializerSettings();
				settings.NullValueHandling = NullValueHandling.Ignore;
				settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
				settings.MissingMemberHandling = MissingMemberHandling.Error;
				settings.Formatting = Formatting.Indented;

				string json = File.ReadAllText(path);

				storage = JsonConvert.DeserializeObject<ChatStorage>(json, settings);
				storage.Chat = chat;
				storage._cache = json;
			}

			return storage ?? new ChatStorage() { Chat = chat };
		}

		internal static void SaveToJson(ChatStorage storage)
		{
			var _path = GetDirectoryPath(storage.Chat);
			if (!Directory.Exists(_path))
				Directory.CreateDirectory(_path);

			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
			settings.MissingMemberHandling = MissingMemberHandling.Error;
			settings.Formatting = Formatting.Indented;

			storage.Variables.SerializeAllCache();
			storage.UsersStorage.SerializeAllCache();

			string path = GetFullFilePath(storage.Chat);
			string json = JsonConvert.SerializeObject(storage, settings);

			if (storage._cache == json) return;
			storage._cache = json;

			File.WriteAllText(path, json);
		}

		/// <summary>
		/// Сохраняет все изменения.
		/// </summary>
		public void Save(bool forced = false)
		{
			if (!forced && DateTime.Now - _lastSaveTime < _timeToSave) return;

			_lastSaveTime = DateTime.Now;

			if (_cache == null)
			{
				if (Variables.Count == 0 && UsersStorage.Count == 0 && _settings.Count == 0) return;
			}

			SaveToJson(this);
		}
	}
}
