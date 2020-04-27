using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkBotCore.Subjects;
using VkBotCore.UI;
using Message = VkNet.Model.Message;

namespace VkBotCore.Subjects
{
	/// <summary>
	/// Класс для взаимодействия с диалогом и пользователями в нём.
	/// </summary>
	public class Chat : BaseChat
	{
		private string _inviteLinkCache { get; set; }

		public Chat(VkCoreApiBase vkApi, long peerId) : base(vkApi, peerId)
		{

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
			VkApi.Messages.RemoveChatUser((ulong)PeerId % BasePeerId, VkApi.GroupId);
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

		/// <summary>
		/// Возвращает ссылку для приглашения пользователя в беседу.
		/// </summary>
		public string GetInviteLink(bool reset = false)
		{
			if (reset || _inviteLinkCache == null)
				_inviteLinkCache = VkApi.Messages.GetInviteLink((ulong)PeerId, reset);
			return _inviteLinkCache;
		}

		/// <summary>
		/// Возвращает идентификаторы администраторов диалога.
		/// </summary>
		public long[] GetAllChatAdministrators()
		{
			try
			{
				var members = VkApi.Messages.GetConversationMembers(PeerId, new List<string>()).Items;
				return members.Where(m => m.IsAdmin).Select(m => m.MemberId).ToArray();
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

		public static IEnumerable<string> GetMentions(IEnumerable<User> users)
		{
			return users.Select(u => u.GetMentionLine());
		}
	}
}
