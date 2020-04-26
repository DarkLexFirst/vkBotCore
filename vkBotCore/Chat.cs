using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkBotCore.UI;
using VkNet.Model;

namespace VkBotCore
{
	/// <summary>
	/// Класс для взаимодействия с диалогом и пользователями в нём
	/// </summary>
	public class Chat : IEquatable<Chat>
	{
		/// <summary>
		/// VkApi обработчик управляющего сообщества.
		/// </summary>
		public VkCoreApiBase VkApi { get; set; }

		/// <summary>
		/// Идентификатор диалога
		/// </summary>
		public long PeerId { get; set; }

		/// <summary>
		/// Определяет, является ли данный диалог личой перепиской пользователя с сообществом.
		/// </summary>
		public bool IsUserConversation { get => PeerId < 2000000000; }

		private Dictionary<string, Keyboard> _cachedKeyboards { get; set; }

		public Keyboard BaseKeyboard { get; set; }

		public Chat(VkCoreApiBase vkApi, long peerId)
		{
			VkApi = vkApi;
			PeerId = peerId;
			_cachedKeyboards = new Dictionary<string, Keyboard>();
		}

		protected internal virtual void OnMessasge(User user, string message, Message messageData)
		{

		}

		protected internal virtual void OnCommand(User user, string command, string[] args, Message messageData)
		{

		}

		protected internal virtual void OnJoin(User addedBy)
		{

		}

		protected internal virtual void OnKick(User kickedBy)
		{

		}

		protected internal virtual void OnAddUser(User user, User addedBy, bool joinByLink)
		{

		}

		protected internal virtual void OnKickUser(User user, User kickedBy)
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
		public bool TryKick(User user)
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
			VkApi.Messages.RemoveChatUser((ulong)PeerId % 2000000000, id);
		}

		/// <summary>
		/// Отправляет текстовое сообщение в диалог.
		/// </summary>
		public virtual void SendMessage(string message, bool disableMentions = false)
		{
			if (!string.IsNullOrEmpty(message))
				VkApi.MessageHandler.SendMessage(message, PeerId, BaseKeyboard, disableMentions);
		}

		/// <summary>
		/// Отправляет текстовое сообщение в диалог. (Асинхронная отправка)
		/// </summary>
		public virtual async Task SendMessageAsync(string message, bool disableMentions = false)
		{
			await VkApi.MessageHandler.SendMessageAsync(message, PeerId, BaseKeyboard, disableMentions);
		}

		/// <summary>
		/// Отправляет текстовое сообщение в диалог. (Упорядоченная отправка через пул)
		/// </summary>
		public virtual void SendMessageWithPool(string message, bool disableMentions = false)
		{
			VkApi.MessageHandler.SendMessageWithPool(message, PeerId, BaseKeyboard, disableMentions);
		}

		/// <summary>
		/// Отправляет текстовое сообщение в диалог.
		/// </summary>
		public void SendMessage(object obj, bool disableMentions = false)
		{
			SendMessage(obj?.ToString(), disableMentions);
		}

		/// <summary>
		/// Отправляет текстовое сообщение в диалог. (Асинхронная отправка)
		/// </summary>
		public async Task SendMessageAsync(object obj, bool disableMentions = false)
		{
			await SendMessageAsync(obj?.ToString(), disableMentions);
		}

		/// <summary>
		/// Отправляет текстовое сообщение в диалог. (Упорядоченная отправка через пул)
		/// </summary>
		public virtual void SendMessageWithPool(object obj, bool disableMentions = false)
		{
			SendMessageWithPool(obj?.ToString(), disableMentions);
		}

		/// <summary>
		/// Отправляет клавиатуру в диалог по её идентификатору.
		/// </summary>
		public void SendKeyboard(string keyboardId)
		{
			if (_cachedKeyboards.ContainsKey(keyboardId))
				SendKeyboard(_cachedKeyboards[keyboardId]);
		}

		/// <summary>
		/// Отправляет клавиатуру в диалог по её идентификатору. (Асинхронная отправка)
		/// </summary>
		public async Task SendKeyboardAsync(string keyboardId)
		{
			if (_cachedKeyboards.ContainsKey(keyboardId))
				await SendKeyboardAsync(_cachedKeyboards[keyboardId]);
		}

		/// <summary>
		/// Отправляет клавиатуру в диалог по её идентификатору.
		/// </summary>
		public void SendKeyboardWithPool(string keyboardId)
		{
			if (_cachedKeyboards.ContainsKey(keyboardId))
				SendKeyboardWithPool(_cachedKeyboards[keyboardId]);
		}

		/// <summary>
		/// Отправляет клавиатуру в диалог.
		/// </summary>
		public void SendKeyboard(Keyboard keyboard)
		{
			AddKeyboard(keyboard);
			VkApi.MessageHandler.SendKeyboard(keyboard, PeerId);
		}

		/// <summary>
		/// Отправляет клавиатуру в диалог. (Асинхронная отправка)
		/// </summary>
		public async Task SendKeyboardAsync(Keyboard keyboard)
		{
			AddKeyboard(keyboard);
			await VkApi.MessageHandler.SendKeyboardAsync(keyboard, PeerId);
		}

		/// <summary>
		/// Отправляет клавиатуру в диалог. (Упорядоченная отправка через пул)
		/// </summary>
		public void SendKeyboardWithPool(Keyboard keyboard)
		{
			AddKeyboard(keyboard);
			VkApi.MessageHandler.SendKeyboardWithPool(keyboard, PeerId);
		}

		/// <summary>
		/// Добавляет клавиатуру в кэш для дальнейшего вызова по идентификатору.
		/// </summary>
		public void AddKeyboard(Keyboard keyboard)
		{
			if (!_cachedKeyboards.ContainsKey(keyboard.Id))
				_cachedKeyboards.Add(keyboard.Id, keyboard);
			else
				_cachedKeyboards[keyboard.Id] = keyboard;
		}

		public void InvokeButton(User user, string keyboardId, string buttonId)
		{
			if (BaseKeyboard?.Id == keyboardId)
			{
				BaseKeyboard.TryInvokeButton(this, user, buttonId);
				return;
			}
			if (_cachedKeyboards.ContainsKey(keyboardId))
			{
				var keyboard = _cachedKeyboards[keyboardId];

				if (keyboard.OneTime)
					_cachedKeyboards.Remove(keyboardId);

				keyboard.TryInvokeButton(this, user, buttonId);
			}
		}

		public bool RemoveMessage(ulong id)
		{
			return VkApi.MessageHandler.DeleteMessage(id);
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

		public override bool Equals(object obj) => obj is Chat user && Equals(user);
		public bool Equals(Chat other) => other.PeerId == PeerId && other.VkApi.Equals(VkApi);

		public override int GetHashCode() => (int)PeerId;
	}

	public class ChatEventArgs : EventArgs
	{
		public Chat Chat { get; }

		public ChatEventArgs(Chat chat)
		{
			Chat = chat;
		}
	}
}
