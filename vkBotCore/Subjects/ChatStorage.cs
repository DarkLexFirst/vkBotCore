using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

			if (_cache == null)
			{
				if (Variables.Count == 0 && UsersStorage.Count == 0 && _settings.Count == 0) return;
			}

			SaveToJson(this);

			_lastSaveTime = DateTime.Now;
		}
	}

	public class UsersStorageSet : Dictionary<long, VariableSet>
	{
		public new VariableSet this[long userId]
		{
			get
			{
				TryAdd(userId, new VariableSet());
				return base[userId];
			}
		}

		public VariableSet this[IUser user] { get => this[user.Id]; }

		internal void SerializeAllCache()
		{
			foreach (var set in this)
				set.Value.SerializeAllCache();
		}
	}

	public class VariableSet : Dictionary<string, string>
	{
		private Dictionary<string, object> _objectsCache { get; set; } = new Dictionary<string, object>();

		public new int Count { get => Math.Max(base.Count, _objectsCache.Count); }

		public new string this[string key]
		{
			get => ContainsKey(key) ? base[key] : null;
			set
			{
				if(value == null)
				{
					Remove(key);
					_objectsCache.Remove(key);
					return;
				}
				if (!TryAdd(key, value))
					base[key] = value;
			}
		}

		public T? GetValue<T>(string key) where T : struct
		{
			var value = this[key];
			if (value == null) return null;
			if (typeof(Enum).IsAssignableFrom(typeof(T)))
				return Enum.Parse(typeof(T), value) as T?;
			return Convert.ChangeType(value, typeof(T)) as T?;
		}
		public void SetValue<T>(string key, T value) where T : struct
		{
			this[key] = value.ToString();
		}

		public T Get<T>(string key) where T : class
		{
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
			settings.MissingMemberHandling = MissingMemberHandling.Error;

			if (_objectsCache.TryGetValue(key, out object cache))
			{
				if (cache is T) return (T)cache;
			}

			var value = this[key];
			if (value == null) return null;

			T obj = JsonConvert.DeserializeObject<T>(value, settings);
			if (!_objectsCache.TryAdd(key, obj))
				_objectsCache[key] = obj;
			return obj;
		}

		public void Set<T>(string key, T value, bool serialize = false) where T : class
		{
			if(value == null)
			{
				Remove(key);
				_objectsCache.Remove(key);
				return;
			}
			if (!_objectsCache.TryAdd(key, value))
				_objectsCache[key] = value;
			if (serialize) Serialize<T>(key, value);
		}

		internal void Serialize<T>(string key, T value) where T : class
		{
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
			settings.MissingMemberHandling = MissingMemberHandling.Error;

			this[key] = JsonConvert.SerializeObject(value, settings);
		}

		internal void SerializeAllCache()
		{
			foreach (var cache in _objectsCache)
				Serialize(cache.Key, cache.Value);
		}
	}
}
