using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkBotCore.Utils;
using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;

namespace VkBotCore.Subjects
{
	public class User : IUser, IEquatable<User>
	{
		/// <summary>
		/// VkApi обработчик управляющего сообщества.
		/// </summary>
		public VkCoreApiBase VkApi { get; set; }

		/// <summary>
		/// Идентификатор пользователя.
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		/// Пол.
		/// </summary>
		public Sex Sex { get; set; }

		/// <summary>
		/// Имя в именительном падеже.
		/// </summary>
		public string FirstName { get; set; }
		/// <summary>
		/// Фамилия в именительном падеже.
		/// </summary>
		public string LastName { get; set; }

		/// <summary>
		/// Определяет, есть ли у пользователя права администратора приложения.
		/// </summary>
		public bool IsAppAdmin { get; private set; }

		private const string PermissionsTag = "user_permissions";

		/// <summary>
		/// Уровень разрешений ползователя.
		/// </summary>
		public short PermissionLevel {
			get
			{
				if (IsAppAdmin) return (short) UserPermission.Unlimited;

				var storagePermission = Storage.GetValue<short>(PermissionsTag) ?? 0;
				var groupPermission = (short)VkApi.Group.GetUserPermissions(this);
				return Math.Max(storagePermission, groupPermission);
			}
			set => Storage.SetValue(PermissionsTag, value); }

		/// <summary>
		/// Хранилище.
		/// </summary>
		/// <exception cref="VkNet.Exception.OutOfLimitsException"/>
		public Storage Storage { get; set; }

		internal User(VkCoreApiBase vkApi, long id)
		{
			VkApi = vkApi;
			Id = id;
			var u = GetApiUser(ProfileFields.Sex);

			Sex = u.Sex;

			FirstName = u?.FirstName;
			LastName = u?.LastName;

			IsAppAdmin = vkApi.Core.Configuration.GetArray<long>("Config:Admins").Contains(Id);

			Storage = new Storage(this);
		}

		internal User(VkCoreApiBase vkApi, long id, string firstName, string lastName)
		{
			VkApi = vkApi;
			FirstName = firstName;
			LastName = lastName;
			Id = id;

			Storage = new Storage(this);
		}

		/// <summary>
		/// Опредеяет, является ли пользователь администратором диалога.
		/// </summary>
		public bool IsChatAdmin(Chat chat)
		{
			return chat.GetAllChatAdministrators().Contains(Id);
		}

		/// <summary>
		/// Устанавливает глобальное разрешения для пользователя.
		/// </summary>
		public void SetPermissions(Enum value)
		{
			var permissions = VkApi.Core.PluginManager.Permissions;
			if (permissions.ContainsValue(value))
			{
				var permission = permissions.First(p => p.Value.Equals(value));
				PermissionLevel = permission.Key;
			}
		}

		/// <summary>
		/// Возвращает личную переписку сообщества с данным пользователем.
		/// </summary>
		public BaseChat GetConversation() => GetConversation<BaseChat>();

		/// <summary>
		/// Возвращает личную переписку сообщества с данным пользователем.
		/// </summary>
		public T GetConversation<T>() where T : BaseChat => VkApi.GetChat<T>(Id);

		/// <summary>
		/// Возвращает строку упоминания.
		/// </summary>
		public string GetMentionLine(NameCase nameCase = null)
		{
			return GetMentionLine(Id, GetUserFirstName(nameCase));
		}

		/// <summary>
		/// Возвращает строку упоминания.
		/// </summary>
		public static string GetMentionLine(long id, string value)
		{
			return id >= 0 ? $"[id{id}|{value ?? id.ToString()}]" : string.Empty;
		}

		/// <summary>
		/// Имя в заданном падеже.
		/// </summary>
		public string GetUserFirstName(NameCase nameCase = null)
		{
			return GetUserFullName(nameCase).Item1;
		}

		/// <summary>
		/// Фамилия в заданном падеже.
		/// </summary>
		public string GetUserLastName(NameCase nameCase = null)
		{
			return GetUserFullName(nameCase).Item2;
		}

		private Dictionary<NameCase, (string, string)> _userNames = new Dictionary<NameCase, (string, string)>();
		public (string, string) GetUserFullName(NameCase nameCase = null)
		{
			if (nameCase == null || nameCase == NameCase.Nom) return (FirstName, LastName);

			if(_userNames.TryGetValue(nameCase, out (string, string) name))
			{
				return name;
			}
			else
			{
				var user = GetApiUser(nameCase: nameCase);
				(string, string) _name = (user.FirstName, user.LastName);

				_userNames.Add(nameCase, _name);
				return _name;
			}
		}

		public VkNet.Model.User GetApiUser(ProfileFields fields = null, NameCase nameCase = null)
		{
			return GetApiUserById(VkApi, Id, fields, nameCase);
		}

		public static VkNet.Model.User GetApiUserById(VkCoreApiBase vkApi, long id, ProfileFields fields = null, NameCase nameCase = null)
		{
			if (id <= 0) return null;
			return vkApi.Users.Get(new long[] { id }, fields, nameCase).First();
		}

		public static async Task<VkNet.Model.User> GetApiUserByIdAsync(VkCoreApiBase vkApi, long id)
		{
			if (id <= 0) return null;
			IReadOnlyCollection<VkNet.Model.User> users;
			users = await vkApi.Users.GetAsync(new long[] { id });
			return users.First();
		}

		public override bool Equals(object obj) => obj is User user && Equals(user);
		public bool Equals(User other) => other.Id == Id;

		public override int GetHashCode() => (int)Id;
	}
}
