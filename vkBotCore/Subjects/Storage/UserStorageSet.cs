using System.Collections.Concurrent;

namespace VkBotCore.Subjects
{
	public class UsersStorageSet : ConcurrentDictionary<long, VariableSet>
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

		public void RemoveAllEmpty()
		{
			foreach (var set in this)
				if (set.Value.IsEmpty)
					TryRemove(set.Key, out _);
		}
	}
}
