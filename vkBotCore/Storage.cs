using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VkBotCore
{
	public class Storage : IEquatable<Storage>
	{
		/// <summary>
		/// Пользователь, к которому привязано данное хранилище.
		/// </summary>
		public User User { get; set; }

		private Dictionary<string, string> _storage { get; set; }
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
			_storage = new Dictionary<string, string>();
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
					if (value != null)
						_storage.Add(key, value);
					return value;
				}
			}
			set => Set(key, value, false, true);
		}

		/// <summary>
		/// Принудительно обновляет ячейку в хранилище.
		/// </summary>
		public void ForcedSet(string key, string value, bool async = true)
		{
			Set(key, value, true, async);
		}

		private void Set(string key, string value, bool forced, bool async)
		{
			lock (_storage)
			{
				value = string.IsNullOrEmpty(value) ? null : value;
				if (_keys == null) _keys = GetKeys();

				if (_storage.ContainsKey(key))
				{
					if (_storage[key] == value) return;
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
					if (async)
						SetAsync(key, value);
					else
						Set(key, value);
					_changes.Remove(key);
				}
				else if (!_changes.Contains(key))
					_changes.Add(key);
			}
		}

		private void Set(string key, string value)
		{
			User.VkApi.Storage.Set(key, value, (ulong)User.Id);
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

			_lastSaveTime = DateTime.Now;
		}

		public override bool Equals(object obj) => obj is Storage storage && Equals(storage);
		public bool Equals(Storage other) => User.Equals(other.User);

		public override int GetHashCode() => User.GetHashCode();
	}
}
