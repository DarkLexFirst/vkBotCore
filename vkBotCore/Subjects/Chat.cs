using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VkBotCore.Subjects;
using VkBotCore.UI;
using VkBotCore.Utils;
using VkNet.Enums.SafetyEnums;
using Message = VkNet.Model.Message;

namespace VkBotCore.Subjects
{
	/// <summary>
	/// Класс для взаимодействия с диалогом и пользователями в нём.
	/// </summary>
	public class Chat : BaseChat
	{
		private ChatStorage _storage;

		/// <summary>
		/// Хранилище.
		/// </summary>
		public ChatStorage Storage { get {
				if (_storage == null)
					_storage = ChatStorage.ReadFromJson(this);
				return _storage;
			}
		}

		/// <summary>
		/// Настройки.
		/// </summary>
		public ChatSettings Settings { get; private set; }


		public Chat(VkCoreApiBase vkApi, long peerId) : base(vkApi, peerId)
		{
			Settings = new ChatSettings(this);
		}

		internal void Join(IUser addedBy)
		{
			if (addedBy is User user)
			{
				SetUserPermission(user, UserPermission.Admin);
			}

			OnJoin(addedBy);
		}

		protected internal virtual void OnJoin(IUser addedBy)
		{

		}

		protected internal virtual void OnKick(IUser kickedBy)
		{

		}

		protected internal virtual void OnAddUser(IUser user, IUser addedBy, bool joinByLink)
		{

		}

		protected internal virtual void OnKickUser(IUser user, IUser kickedBy)
		{

		}

		public void Pin(long messageId)
		{
			VkApi.Messages.Pin(PeerId, (ulong)messageId);
		}

		/// <summary>
		/// Открепляет сообщение.
		/// </summary>
		public void Unpin()
		{
			VkApi.Messages.Unpin(PeerId);
		}

		/// <summary>
		/// Исключает пользователя из диалога.
		/// </summary>
		public bool TryKick(IUser user)
		{
			return TryKick(user.Id);
		}

		/// <summary>
		/// Исключает пользователя из диалога по его идентификатору.
		/// </summary>
		public bool TryKick(long id)
		{
			try { Kick(id); } catch { return false; }
			return true;
		}

		/// <summary>
		/// Исключает пользователя из диалога по его идентификатору.
		/// </summary>
		public void Kick(long id)
		{
			VkApi.Messages.RemoveChatUser((ulong)PeerId % BasePeerId, id);
		}

		/// <summary>
		/// Осуществляет выход из диалога.
		/// </summary>
		public void Leave()
		{
			VkApi.Messages.RemoveChatUser((ulong)PeerId % BasePeerId, -VkApi.GroupId);
		}

		/// <summary>
		/// Проверяет наличие разрешений у управляющего сообщества.
		/// </summary>
		public bool HavePermissions()
		{
			try
			{
				return GetAllChatAdministrators().Contains(-VkApi.GroupId);
			}
			catch
			{
				return false;
			}
		}

		private string _inviteLinkCache;

		/// <summary>
		/// Возвращает ссылку для приглашения пользователя в беседу.
		/// </summary>
		public string GetInviteLink(bool reset = false)
		{
			if (reset || _inviteLinkCache == null)
				_inviteLinkCache = VkApi.Messages.GetInviteLink((ulong)PeerId, reset);
			return _inviteLinkCache;
		}

		private long[] _adminsCache = null;
		private DateTime _lastAdminsGetTime = DateTime.Now;

		/// <summary>
		/// Возвращает идентификаторы администраторов диалога.
		/// </summary>
		public long[] GetAllChatAdministrators()
		{
			try
			{
				if (_adminsCache != null && (DateTime.Now - _lastAdminsGetTime).TotalSeconds < 10)
					return _adminsCache;

				_adminsCache = JsonConvert.DeserializeObject<Dictionary<string, long[]>>(VkApi.Execute.Execute($@"
				
				var members = API.messages.getConversationMembers({{{($"\"peer_id\": {PeerId}")}}});
				
				var result = [];
				var i = 0;
				
				while(i < members.items.length)
				{{
					if(members.items[i].is_admin)
						result.push(members.items[i].member_id);
					i = i + 1;
				}}
				
				return result;

				").RawJson).FirstOrDefault().Value;

				if (_adminsCache == null) return new long[0];

				return _adminsCache;
			}
			catch
			{
				return new long[0];
			}
		}

		/// <summary>
		/// Возвращает идентификаторы всех участников диалога.
		/// </summary>
		public long[] GetAllChatMembers()
		{
			try
			{
				var members = VkApi.Messages.GetConversationMembers(PeerId, new List<string>()).Profiles;
				return members.Select(m => m.Id).ToArray();
			}
			catch
			{
				return new long[0];
			}
		}

		private const string PermissionsTag = "user_chat_permissions";

		/// <summary>
		/// Устанавливает разрешения для пользователя в чате.
		/// </summary>
		public void SetUserPermission(User user, Enum value)
		{
			var permissions = VkApi.Core.PluginManager.Permissions;
			if (permissions.ContainsValue(value))
			{
				var permission = permissions.First(p => p.Value.Equals(value));
				SetUserPermission(user, permission.Key);
			}
		}

		/// <summary>
		/// Устанавливает разрешения для пользователя в чате.
		/// </summary>
		public void SetUserPermission(User user, short value)
		{
			Storage.UsersStorage[user].SetValue(PermissionsTag, value);
		}

		/// <summary>
		/// Возвращает разрешение для пользователя в чате.
		/// </summary>
		public short GetUserPermission(User user)
		{
			if (user.IsChatAdmin(this)) return (short) UserPermission.Unlimited;
			return Storage.UsersStorage[user].GetValue<short>(PermissionsTag) ?? 0;
		}

		public string GetEveryoneMentionLine(string val = "&#8203;")
		{
			return GetMentionLine(GetAllChatMembers(), val);
		}

		public IEnumerable<string> GetEveryoneMentions(string val = "&#8203;")
		{
			return GetMentions(GetAllChatMembers(), val);
		}

		public static string GetMentionLine(IEnumerable<long> users, string val = "&#8203;")
		{
			return string.Join("", GetMentions(users, val));
		}

		public static IEnumerable<string> GetMentions(IEnumerable<long> users, string val = "&#8203;")
		{
			return users.Select(m => User.GetMentionLine(m, val));
		}

		public static IEnumerable<string> GetMentions(IEnumerable<IUser> users, NameCase nameCase = null)
		{
			return users.Select(u => u.GetMentionLine(nameCase));
		}
	}
}
