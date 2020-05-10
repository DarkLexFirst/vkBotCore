using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using VkBotCore.Configuration;
using VkBotCore.Subjects;
using VkNet;

namespace VkBotCore
{
	public class VkCoreApiBase : VkApi
	{
		public BotCore Core { get; private set; }

		/// <summary>
		/// Идентификатор сообщества.
		/// </summary>
		public long GroupId { get; private set; }

		internal ConcurrentDictionary<long, BaseChat> _chatsCache { get; set; }
		internal ConcurrentDictionary<long, IUser> _usersCache { get; set; }

		/// <summary>
		/// Обработчик сообщений.
		/// </summary>
		public MessageHandler MessageHandler { get; private set; }


		/// <summary>
		/// Обрабатываемые пространства имён.
		/// </summary>
		public string[] AvailableNamespaces { get; private set; }

		public VkCoreApiBase(BotCore core, long groupId)
		{
			Core = core;
			GroupId = groupId;
			RequestsPerSecond = Core.Configuration.GetValue($"Config:Groups:{groupId}:RequestsPerSecond", 20);
			MessageHandler = new MessageHandler(this);
			AvailableNamespaces = Core.Configuration.GetArray($"Config:Groups:{groupId}:AvailableNamespaces", new string[0]);

			_chatsCache = new ConcurrentDictionary<long, BaseChat>();
			_usersCache = new ConcurrentDictionary<long, IUser>();
		}

		/// <summary>
		/// Перопределение создания чатов.
		/// </summary>
		public Func<VkCoreApiBase, long, BaseChat> GetNewChat { get; set; }

		/// <summary>
		/// Инициализирует новый диалог.
		/// </summary>
		public T GetChat<T>(long peerId) where T : BaseChat => (T) GetChat(peerId);

		/// <summary>
		/// Инициализирует новый диалог.
		/// </summary>
		public BaseChat GetChat(long peerId)
		{
			return _chatsCache.GetOrAdd(peerId, _peerId =>
			{
				BaseChat chat = GetNewChat?.Invoke(this, _peerId) ?? (BaseChat.IsUserConversation(_peerId) ? (BaseChat) new Conversation(this, _peerId) : new Chat(this, _peerId));
				Core.VkApi.OnChatCreated(new ChatEventArgs(chat));
				if (Core.VkApi != this)
					OnChatCreated(new ChatEventArgs(chat));
				return chat;
			});
		}

		/// <summary>
		/// Создаёт новый диалог.
		/// </summary>
		public Chat CreateNewChat(string title)
		{
			long peerId = Messages.CreateChat(null, title) + BaseChat.BasePeerId;
			return GetChat<Chat>(peerId);
		}

		/// <summary>
		/// Событие, вызываемое при инициализации чата.
		/// </summary>
		public event EventHandler<ChatEventArgs> ChatCreated;

		protected virtual void OnChatCreated(ChatEventArgs e)
		{
			ChatCreated?.Invoke(this, e);
		}

		/// <summary>
		/// Перопределение создания пользователей.
		/// </summary>
		public Func<VkCoreApiBase, long, IUser> GetNewUser { get; set; }


		public T GetUser<T>(long id) where T : IUser => (T) GetUser(id);

		public IUser GetUser(long id)
		{
			return _usersCache.GetOrAdd(id, _id =>
			{
				IUser user = GetNewUser?.Invoke(this, _id) ?? (_id < 0 ? (IUser) new Group(this, _id) : new User(this, _id));
				Core.VkApi.OnUserCreated(new UserEventArgs(user));
				if (Core.VkApi != this)
					OnUserCreated(new UserEventArgs(user));
				return user;
			});
		}

		/// <summary>
		/// Событие, вызываемое при инициализации пользователя.
		/// </summary>
		public event EventHandler<UserEventArgs> UserCreated;

		protected virtual void OnUserCreated(UserEventArgs e)
		{
			UserCreated?.Invoke(this, e);
		}
	}
}
