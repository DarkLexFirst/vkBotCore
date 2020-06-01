using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VkBotCore.Subjects
{
	public class Storage
	{
		/// <summary>
		/// Пользователь, к которому привязано данное хранилище.
		/// </summary>
		public User User { get; set; }

		private VariableSet _storage { get; set; }
		private List<string> _changes { get; set; }
		private List<string> _keys { get; set; }

		/// <summary>
		/// Имена всех сохранённых ячеек.
		/// </summary>
		public string[] Keys { get => (_keys == null ? _keys = GetKeys() : _keys).ToArray(); }

		private DateTime _lastSaveTime = DateTime.Now;
		private TimeSpan _timeToSave = new TimeSpan(0, 5, 0);

		public Storage(User user)
		{
			User = user;
			_storage = new VariableSet();
			_changes = new List<string>();
		}

		public string this[string key]
		{
			get
			{
				lock (_storage)
				{
					if (_storage.ContainsKey(key))
						return _storage[key];
					var value = Get(key);
					_storage.Add(key, value);
					return value;
				}
			}
			set => Set(key, value, false);
		}

		/// <summary>
		/// Принудительно обновляет ячейку в хранилище.
		/// </summary>
		public void ForcedSet(string key, string value)
		{
			Set(key, value, true);
		}

		/// <summary>
		/// Принудительно обновляет ячейку в хранилище.
		/// </summary>
		public void ForcedSet<T>(string key, T value) where T : class
		{
			Set(key, value, true);
		}

		/// <summary>
		/// Принудительно обновляет ячейку в хранилище.
		/// </summary>
		public void ForcedSetValue<T>(string key, T value) where T : struct
		{
			Set(key, value.ToString(), true);
		}

		/// <summary>
		/// Обновляет ячейку в хранилище.
		/// </summary>
		public void Set<T>(string key, T value) where T : class
		{
			Set(key, value, false);
		}

		/// <summary>
		/// Обновляет ячейку в хранилище.
		/// </summary>
		public void SetValue<T>(string key, T value) where T : struct
		{
			Set(key, value.ToString(), false);
		}

		private void Set<T>(string key, T value, bool forced) where T : class
		{
			_storage.Set(key, value);
			Set(key, _storage[key], forced);
		}

		private void Set(string key, string value, bool forced)
		{
			lock (_storage)
			{
				value = string.IsNullOrEmpty(value) ? null : value;
				if (_keys == null) _keys = GetKeys();

				if (_storage.ContainsKey(key))
				{
					if (_storage[key] == value && !_storage.IsObject(key)) return;
					_storage[key] = value;
				}
				else
				{
					_storage.Add(key, value);
				}

				if (value == null)
					_keys.Remove(key);
				else if (!_keys.Contains(key))
					_keys.Add(key);

				if (forced)
				{
					SetAsync(key, value);
					_changes.Remove(key);
				}
				else if (!_changes.Contains(key))
					_changes.Add(key);
			}
		}

		/// <summary>
		/// Возвращает объект из ячейки.
		/// </summary>
		public T Get<T>(string key) where T : class
		{
			if (!_storage.ContainsKey(key))
				_storage.Add(key, Get(key));
			return _storage.Get<T>(key);
		}

		/// <summary>
		/// Возвращает переменную из ячейки.
		/// </summary>
		public T? GetValue<T>(string key) where T : struct
		{
			if (!_storage.ContainsKey(key))
				_storage.Add(key, Get(key));
			return _storage.GetValue<T>(key);
		}

		private async Task SetAsync(string key, string value)
		{
			await User.VkApi.Storage.SetAsync(key, value, (ulong)User.Id);
		}

		private string Get(string key)
		{
			return User.VkApi.Storage.Get(new string[] { key }, (ulong)User.Id).FirstOrDefault().Value;
		}

		private List<string> GetKeys()
		{
			return User.VkApi.Storage.GetKeys((ulong)User.Id, count: 1000).ToList();
		}

		/// <summary>
		/// Сохраняет все изменения.
		/// </summary>
		public void Save(bool forced = false)
		{
			if (!forced && DateTime.Now - _lastSaveTime < _timeToSave) return;

			_lastSaveTime = DateTime.Now;

			lock (_storage)
			{
				if (_changes.Count == 0) return;
				Task[] tasks = new Task[_changes.Count];
				int i = 0;
				foreach (var ch in _changes)
					tasks[i++] = SetAsync(ch, _storage[ch]);
				_changes.Clear();
				Task.WaitAll(tasks);
			}
		}
	}
}
