using System;
using System.Collections.Generic;
using System.Linq;

namespace VkBotCore.Utils
{
	public class PermissionSet : Dictionary<short, Enum>
	{
		public void Add(Enum permission)
		{
			Add((short) permission.GetHashCode(), permission);
		}

		public bool TryGetPermission(Enum permission, out short permissionLevel)
		{
			permissionLevel = (short) permission.GetHashCode();
			if (!ContainsValue(permission)) return false;
			permissionLevel = this.First(p => p.Value.Equals(permission)).Key;
			return true;
		}
	}
}
