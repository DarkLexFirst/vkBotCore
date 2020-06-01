using System.Collections.Generic;

namespace VkBotCore.Subjects
{
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
}
