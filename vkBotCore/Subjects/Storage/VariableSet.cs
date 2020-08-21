using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace VkBotCore.Subjects
{


	public class VariableSet : ConcurrentDictionary<string, string>
	{
		private ConcurrentDictionary<string, object> _objectsCache { get; set; } = new ConcurrentDictionary<string, object>();

		public new int Count { get => Math.Max(base.Count, _objectsCache.Count); }

		public new bool IsEmpty { get => base.IsEmpty && _objectsCache.IsEmpty; }

		public new ICollection<string> Keys { get => new ReadOnlyCollection<string>(Keys.Union(_objectsCache.Keys).ToList()); }

		public new string this[string key]
		{
			get
			{
				if (_objectsCache.TryGetValue(key, out object value))
					return Serialize(key, value);
				else if (base.ContainsKey(key))
					return base[key];
				else
					return null;

			}
			set
			{
				if (value == null)
				{
					TryRemove(key, out _);
					_objectsCache.TryRemove(key, out _);
					return;
				}
				if (!TryAdd(key, value))
					base[key] = value;
			}
		}

		public new bool ContainsKey(string key)
		{
			return base.ContainsKey(key) || _objectsCache.ContainsKey(key);
		}

		public T? GetValue<T>(string key) where T : struct
		{
			var value = this[key];
			if (value == null)
				return null;
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
			if (_objectsCache.TryGetValue(key, out object cache))
			{
				if (cache is T)
					return (T) cache;
			}

			var value = this[key];
			if (value == null)
				return null;

			T obj;
			try
			{
				var settings = new JsonSerializerSettings();
				settings.NullValueHandling = NullValueHandling.Ignore;
				settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
				settings.MissingMemberHandling = MissingMemberHandling.Error;

				obj = JsonConvert.DeserializeObject<T>(value, settings);
			}
			catch
			{
				return null;
			}

			if (!_objectsCache.TryAdd(key, obj))
				_objectsCache[key] = obj;
			return obj;
		}

		public void Set<T>(string key, T value, bool serialize = false) where T : class
		{
			if (value == null)
			{
				TryRemove(key, out _);
				_objectsCache.TryRemove(key, out _);
				return;
			}
			if (!_objectsCache.TryAdd(key, value))
				_objectsCache[key] = value;
			if (serialize)
				Serialize(key, value);
		}

		public bool IsObject(string key)
		{
			return _objectsCache.ContainsKey(key);
		}

		public new void Clear()
		{
			base.Clear();
			_objectsCache.Clear();
		}

		internal string Serialize(string key, object value)
		{
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
			settings.MissingMemberHandling = MissingMemberHandling.Error;

			return this[key] = JsonConvert.SerializeObject(value, settings);
		}

		internal void SerializeAllCache()
		{
			foreach (var cache in _objectsCache)
				Serialize(cache.Key, cache.Value);
		}
	}
}
