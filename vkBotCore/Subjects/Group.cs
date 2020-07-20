using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkBotCore.Utils;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;

namespace VkBotCore.Subjects
{
	public class Group : IUser, IEquatable<Group>
	{
		/// <summary>
		/// VkApi обработчик управляющего сообщества.
		/// </summary>
		public VkCoreApiBase VkApi { get; set; }

		/// <summary>
		/// Идентификатор сообщества.
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		/// Название сообщества.
		/// </summary>
		public string Name { get; set; }

		internal Group(VkCoreApiBase vkApi, long id)
		{
			VkApi = vkApi;
			Id = id;
			var g = GetApiGroup();
			Name = g.Name;
		}

		/// <summary>
		/// Опредеяет, является ли сообщество администратором диалога.
		/// </summary>
		public bool IsChatAdmin(Chat chat)
		{
			return chat.GetAllChatAdministrators().Contains(Id);
		}

		public UserPermission GetUserPermissions(User user)
		{
			return GetUserPermissions(user.Id);
		}

		public UserPermission GetUserPermissions(long userId)
		{
			if(Managers.TryGetValue(userId, out UserPermission value))
				return value;
			return UserPermission.None;
		}

		internal Dictionary<long, UserPermission> Managers
		{
			get
			{
				if (_managers == null)
					_managers = GetGroupManagers();
				return _managers;
			}
		}

		private Dictionary<long, UserPermission> _managers;
		private Dictionary<long, UserPermission> GetGroupManagers()
		{
			var groupManagers = VkApi.Groups.GetMembers(new GroupsGetMembersParams()
			{
				GroupId = (-Id).ToString(),
				Count = 100,
				Filter = GroupsMemberFilters.Managers
			});

			var managers = new Dictionary<long, UserPermission>();

			foreach (var manager in groupManagers)
			{
				switch (manager.Role?.ToString())
				{
					case "creator":
						managers.Add(manager.Id, UserPermission.Unlimited);
						break;
					case "administrator":
						managers.Add(manager.Id, UserPermission.Admin);
						break;
					case "moderator":
						managers.Add(manager.Id, UserPermission.Modarator);
						break;
					case "editor":
						managers.Add(manager.Id, UserPermission.Editor);
						break;
				}
			};

			return managers;
		}

		/// <summary>
		/// Возвращает строку упоминания.
		/// </summary>
		public string GetMentionLine(NameCase nameCase = null)
		{
			return GetMentionLine(Id, Name);
		}

		/// <summary>
		/// Возвращает строку упоминания.
		/// </summary>
		public static string GetMentionLine(long id, string value)
		{
			return id >= 0 ? $"[club{id}|{value ?? id.ToString()}]" : string.Empty;
		}

		public VkNet.Model.Group GetApiGroup()
		{
			return GetApiGroupById(VkApi, Id);
		}

		public static VkNet.Model.Group GetApiGroupById(VkCoreApiBase vkApi, long id)
		{
			return vkApi.Groups.GetById(null, $"club{Math.Abs(-id)}", null).First();
		}

		public static async Task<VkNet.Model.Group> GetApiGroupByIdAsync(VkCoreApiBase vkApi, long id)
		{
			IReadOnlyCollection<VkNet.Model.Group> groups;
			groups = await vkApi.Groups.GetByIdAsync(null, $"club{Math.Abs(-id)}", null);
			return groups.First();
		}

		public override bool Equals(object obj) => obj is Group group && Equals(group);
		public bool Equals(Group other) => other.Id == Id;

		public override int GetHashCode() => (int)Id;
	}
}
