using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace vkBotCore
{
    public class Storage : IEquatable<Storage>
    {
        private static List<Storage> cache { get; set; } = new List<Storage>();

        public User User { get; set; }

        private Dictionary<string, string> _storage { get; set; }
        private List<string> _changes { get; set; }
        private List<string> _keys { get; set; }

        public string[] Keys { get => (_keys == null ? _keys = GetKeys() : _keys).ToArray(); }

        private static Timer _saveTimer;
        private DateTime _lastUpdate = DateTime.Now;

        static Storage()
        {
            _saveTimer = new Timer(1000);
            _saveTimer.AutoReset = true;
            int i = 0;
            _saveTimer.Elapsed += (s, e) =>
            {
                if (++i == 300)
                {
                    foreach (var storage in cache.ToArray())
                    {
                        storage.Save();
                        if ((DateTime.Now - storage._lastUpdate).TotalMinutes > 60)
                            cache.Remove(storage);
                    }
                    i = 0;
                }
            };
            _saveTimer.Start();
        }

        private bool _initialized = false;

        public Storage(User user)
        {
            User = user;
            _storage = new Dictionary<string, string>();
            _changes = new List<string>();
        }

        public string this[string key] {
            get {
                var storage = Initialize();
                if (storage != null)
                    return storage[key];

                lock (_storage)
                {
                    _lastUpdate = DateTime.Now;
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

        private Storage Initialize()
        {
            if (!_initialized)
            {
                _initialized = true;
                var storage = cache.FirstOrDefault(c => Equals(c));
                if (storage != null)
                {
                    User.Storage = storage;
                    User.Storage.User = User;
                    _storage = storage._storage;
                    _changes = storage._changes;
                    _keys = storage._keys;
                }
                else
                    cache.Add(this);
                return storage;
            }
            return null;
        }

        public void ForcedSet(string key, string value)
        {
            Set(key, value, true);
        }

        private void Set(string key, string value, bool forced)
        {
            var storage = Initialize();
            if (storage != null)
            {
                User.Storage.Set(key, value, forced);
                return;
            }

            lock (_storage)
            {
                _lastUpdate = DateTime.Now;
                if (_storage.ContainsKey(key))
                {
                    if (_storage[key] == value) return;
                    _storage[key] = value;
                }
                else
                {
                    _storage.Add(key, value);
                    if (_keys != null)
                    {
                        if (value == null)
                            _keys.Remove(key);
                        else
                            _keys.Add(key);
                    }
                }
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

        public void Save()
        {
            Initialize();
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
