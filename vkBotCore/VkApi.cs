using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Linq;
using VkBotCore.Configuration;
using VkNet;
using VkNet.Model;

namespace VkBotCore
{
	public class VkCoreApi : VkCoreApiBase
	{
		internal ConcurrentDictionary<long, VkCoreApiBase> _vkApi { get; private set; }

		public VkCoreApi(BotCore core) : base(core, 0)
		{
			_vkApi = new ConcurrentDictionary<long, VkCoreApiBase>();

			var accesToken = Core.Configuration.GetValue<string>($"Config:AccessToken", null);
			if (accesToken != null)
				Authorize(new ApiAuthParams { AccessToken = accesToken });

			LoadAll();
		}

		public void LoadAll()
		{
			foreach (var a in Core.Configuration.GetSection("Config:Groups").GetChildren())
				Get(long.Parse(a.Key));
		}

		public VkCoreApiBase Get(long groupId)
		{
			if (groupId == GroupId) return this;
			return _vkApi.GetOrAdd(groupId, _groupId =>
			{
				var accesToken = Core.Configuration.GetValue<string>($"Config:Groups:{groupId}:AccessToken", null);
				if (accesToken == null)
					return this;
				var api = new VkCoreApiBase(Core, _groupId);
				api.Authorize(new ApiAuthParams { AccessToken = accesToken });
				return api;
			});
		}

		public VkCoreApiBase[] GetAvailableApis(string _namespace)
		{
			var apis = _vkApi.Values.Where(a => a.AvailableNamespaces.Contains(_namespace));
			if (AvailableNamespaces.Contains(_namespace))
			{
				var _apis = apis.ToList();
				_apis.Add(this);
				return _apis.ToArray();
			}
			return apis.ToArray();
		}
	}

	public class VkCoreApiBase : VkApi
	{
		public BotCore Core { get; private set; }

		/// <summary>
		/// Идентификатор сообщества.
		/// </summary>
		public long GroupId { get; private set; }

		internal ConcurrentDictionary<long, Chat> _chatsCache { get; set; }
		internal ConcurrentDictionary<long, User> _usersCache { get; set; }

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

			_chatsCache = new ConcurrentDictionary<long, Chat>();
			_usersCache = new ConcurrentDictionary<long, User>();
		}

		/// <summary>
		/// Перопределение создания чатов.
		/// </summary>
		public Func<VkCoreApiBase, long, Chat> GetNewChat { get; set; }

		public Chat GetChat(long peerId)
		{
			return _chatsCache.GetOrAdd(peerId, _peerId =>
			{
				Chat chat = GetNewChat?.Invoke(this, _peerId) ?? new Chat(this, _peerId);
				Core.VkApi.OnChatCreated(new ChatEventArgs(chat));
				if (Core.VkApi != this)
					OnChatCreated(new ChatEventArgs(chat));
				return chat;
			});
		}

		/// <summary>
		/// Событие вызываемое при инициализации чата.
		/// </summary>
		public event EventHandler<ChatEventArgs> ChatCreated;

		protected virtual void OnChatCreated(ChatEventArgs e)
		{
			ChatCreated?.Invoke(this, e);
		}

		/// <summary>
		/// Перопределение создания пользователей.
		/// </summary>
		public Func<VkCoreApiBase, long, User> GetNewUser { get; set; }

		public User GetUser(long id)
		{
			return _usersCache.GetOrAdd(id, _id =>
			{
				User user = GetNewUser?.Invoke(this, _id) ?? new User(this, _id);
				//Core.VkApi.OnUserCreated(new UserEventArgs(user));
				//if (Core.VkApi != this)
				//    OnUserCreated(new UserEventArgs(user));
				return user;
			});
		}

		///// <summary>
		///// Событие вызываемое при инициализации пользователя.
		///// </summary>
		//public event EventHandler<UserEventArgs> UserCreated;

		//protected virtual void OnUserCreated(UserEventArgs e)
		//{
		//    UserCreated?.Invoke(this, e);
		//}
	}
}
