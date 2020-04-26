using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using VkBotCore.UI;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace VkBotCore
{
	public class MessageHandler
	{
		public VkCoreApiBase VkApi { get; set; }

		private Dictionary<Chat, Queue<long>> _lastMessages;
		private long _messageResendBlockTime = 10;

		public MessageHandler(VkCoreApiBase vkApi)
		{
			VkApi = vkApi;

			_lastMessages = new Dictionary<Chat, Queue<long>>();

			InitializePoolWorker();
		}

		public virtual void OnMessage(User user, string message, Chat chat, Message messageData)
		{
			//защита от дублированных или задержанных сообщений
			if ((DateTime.UtcNow - messageData.Date.Value).TotalSeconds > _messageResendBlockTime) return;

			var msgId = messageData.ConversationMessageId.Value;
			if (!_lastMessages.ContainsKey(chat))
				_lastMessages.Add(chat, new Queue<long>());
			else
			{
				if (_lastMessages[chat].Contains(msgId)) return;
				_lastMessages[chat].Enqueue(msgId);
				if (_lastMessages[chat].Count > 10)
					_lastMessages[chat].Dequeue();
			}
			//защита от дублированных или задержанных сообщений


			//actions
			if (messageData.Action != null)
			{
				if (messageData.Action.Type == MessageAction.ChatKickUser)
				{
					if (messageData.Action.MemberId == -VkApi.GroupId)
					{
						chat.OnKick(user);
						VkApi._chatsCache.Remove(chat.PeerId, out _);
						_lastMessages.Remove(chat);
					}
					else
						chat.OnKickUser(VkApi.GetUser(messageData.Action.MemberId.Value), user);
					return;
				}
				else if (messageData.Action.Type == MessageAction.ChatInviteUser)
				{
					chat.OnAddUser(VkApi.GetUser(messageData.Action.MemberId.Value), user, false);
					return;
				}
				else if (messageData.Action.Type == MessageAction.ChatInviteUserByLink)
				{
					chat.OnAddUser(VkApi.GetUser(messageData.Action.MemberId.Value), null, true);
					return;
				}
			}


			//buttons
			if (!string.IsNullOrEmpty(messageData.Payload))
			{
				try
				{
					var payload = JsonConvert.DeserializeObject<KeyboardButtonPayload>(messageData.Payload);
					if (payload.Button != null)
					{
						var s = payload.Button.Split(':');
						OnButtonClick(chat, user, message, s[0], s.Length == 1 ? "0" : s[1], messageData);
						return;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}


			//commands
			if ((message.StartsWith("/") || message.StartsWith(".")) && message.Length != 1)
			{
				try
				{
					message = message.Replace("ё", "е");
					VkApi.Core.PluginManager.HandleCommand(user, chat, message, messageData);

					message = message.Substring(1);
					var args = message.Split(' ').ToList();
					string commandName = args.First();
					args.Remove(commandName);

					chat.OnCommand(user, commandName.ToLower(), args.ToArray(), messageData);
				}
				catch (Exception e)
				{
					chat.SendMessageAsync("Комманда задана неверно!");
					VkApi.Core.Log.Error(e.ToString());
				}
				return;
			}

			//other
			if (!OnGetMessage(new GetMessageEventArgs(chat, user, message, messageData))) return;
			chat.OnMessasge(user, message, messageData);
		}

		public virtual void OnButtonClick(Chat chat, User user, string message, string keyboardId, string buttonId, Message messageData)
		{
			if (!OnButtonClick(new ButtonClickEventArgs(chat, user, message, keyboardId, buttonId, messageData))) return;
			chat.InvokeButton(user, keyboardId, buttonId);
		}

		public void SendMessage(string message, long peerId, Keyboard keyboard = null, bool disableMentions = false)
		{
			SendMessage(new MessagesSendParams
			{
				RandomId = GetRandomId(),
				PeerId = peerId,
				Message = message,
				Keyboard = keyboard?.GetKeyboard(),
				DisableMentions = disableMentions
			});
		}

		public async Task SendMessageAsync(string message, long peerId, Keyboard keyboard = null, bool disableMentions = false)
		{
			await SendMessageAsync(new MessagesSendParams
			{
				RandomId = GetRandomId(),
				PeerId = peerId,
				Message = message,
				Keyboard = keyboard?.GetKeyboard(),
				DisableMentions = disableMentions
			});
		}

		public void SendMessageWithPool(string message, long peerId, Keyboard keyboard = null, bool disableMentions = false)
		{
			SendMessageWithPool(new MessagesSendParams
			{
				RandomId = GetRandomId(),
				PeerId = peerId,
				Message = message,
				Keyboard = keyboard?.GetKeyboard(),
				DisableMentions = disableMentions
			});
		}

		public void SendMessage(MessagesSendParams message)
		{
			VkApi.Messages.Send(message);
		}

		public async Task SendMessageAsync(MessagesSendParams message)
		{
			await VkApi.Messages.SendAsync(message);
		}

		public void SendMessageWithPool(MessagesSendParams message)
		{
			_sendPool.Add(message);
			if (!_poolTimer.Enabled)
				_poolTimer.Start();
		}

		public void SendKeyboard(Keyboard keyboard, long peerId)
		{
			SendMessage(new MessagesSendParams
			{
				RandomId = GetRandomId(),
				PeerId = peerId,
				Message = keyboard.Message,
				Keyboard = keyboard.GetKeyboard()
			});
		}

		public async Task SendKeyboardAsync(Keyboard keyboard, long peerId)
		{
			await SendMessageAsync(new MessagesSendParams
			{
				RandomId = GetRandomId(),
				PeerId = peerId,
				Message = keyboard.Message,
				Keyboard = keyboard.GetKeyboard()
			});
		}

		public void SendKeyboardWithPool(Keyboard keyboard, long peerId)
		{
			SendMessageWithPool(new MessagesSendParams
			{
				RandomId = GetRandomId(),
				PeerId = peerId,
				Message = keyboard.Message,
				Keyboard = keyboard.GetKeyboard()
			});
		}

		public void SendSticker(MessagesSendStickerParams message)
		{
			VkApi.Messages.SendSticker(message);
		}

		public bool DeleteMessage(ulong id)
		{
			return VkApi.Messages.Delete(new ulong[] { id }, false, (ulong)VkApi.GroupId, true).First().Value;
		}

		private Timer _poolTimer;
		private List<MessagesSendParams> _sendPool;
		private void InitializePoolWorker()
		{
			_sendPool = new List<MessagesSendParams>();

			_poolTimer = new Timer(15);
			_poolTimer.AutoReset = false;
			_poolTimer.Elapsed += async (s, e) =>
			{
				try
				{
					if (_sendPool.Count == 0) return;
					var messages = _sendPool;
					_sendPool = new List<MessagesSendParams>();
					foreach (var message in messages)
						await SendMessageAsync(message);
				}
				catch { }
			};
		}

		public event EventHandler<GetMessageEventArgs> GetMessage;

		protected virtual bool OnGetMessage(GetMessageEventArgs e)
		{
			GetMessage?.Invoke(this, e);

			return !e.Cancel;
		}

		public event EventHandler<ButtonClickEventArgs> ButtonClick;

		protected virtual bool OnButtonClick(ButtonClickEventArgs e)
		{
			ButtonClick?.Invoke(this, e);

			return !e.Cancel;
		}

		private Random rnd = new Random();

		private int GetRandomId()
		{
			return rnd.Next();
		}
	}

	public class GetMessageEventArgs : EventArgs
	{
		public bool Cancel { get; set; }

		public Chat Chat { get; set; }

		public User Sender { get; set; }

		public string Message { get; set; }

		public Message MessageData { get; set; }

		public GetMessageEventArgs(Chat chat, User sender, string message, Message messageData)
		{
			Chat = chat;
			Sender = sender;
			Message = message;
			MessageData = messageData;
		}
	}

	public class ButtonClickEventArgs : GetMessageEventArgs
	{

		public string KeyboardId { get; set; }

		public string ButtonId { get; set; }

		public ButtonClickEventArgs(Chat chat, User sender, string message, string keyboardId, string buttonId, Message messageData) : base(chat, sender, message, messageData)
		{
			KeyboardId = keyboardId;
			ButtonId = buttonId;
		}
	}
}
