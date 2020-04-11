using System;
using System.Collections.Generic;
using System.Linq;

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

        public Storage(User user)
        {
            User = user;
            _storage = new Dictionary<string, string>();
            _changes = new List<string>();
        }

        public string this[string key] {
            get {
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
            set => Set(key, value, false);
        }

        /// <summary>
        /// Принудительно обнавляет ячейку в хранилище.
        /// </summary>
        public void ForcedSet(string key, string value)
        {
            Set(key, value, true);
        }

        private void Set(string key, string value, bool forced)
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
                else if(!_keys.Contains(key))
                    _keys.Add(key);

                if (forced)
                    Set(key, value);
                else if (!_changes.Contains(key))
                    _changes.Add(key);
            }
        }

        private void Set(string key, string value)
        {
            User.VkApi.Storage.Set(key, value, (ulong)User.Id);
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
        public void Save()
        {
            lock (_storage)
            {
                foreach (var ch in _changes)
                    Set(ch, _storage[ch]);
                _changes.Clear();
            }
        }

        public override bool Equals(object obj) => obj is Storage storage && Equals(storage);
        public bool Equals(Storage other) => User.Equals(other.User);

        public override int GetHashCode() => User.GetHashCode();
    }
}
