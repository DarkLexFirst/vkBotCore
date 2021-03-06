﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using VkBotCore.Subjects;
using VkBotCore.UI;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model.RequestParams;
using Message = VkNet.Model.Message;
using EventData = VkNet.Model.EventData;

namespace VkBotCore
{
	public class MessageHandler
	{
		public VkCoreApiBase VkApi { get; set; }

		private Dictionary<BaseChat, Queue<long>> _lastMessages;
		public string[] CommandStarts { get; set; }

		public MessageHandler(VkCoreApiBase vkApi)
		{
			VkApi = vkApi;

			_lastMessages = new Dictionary<BaseChat, Queue<long>>();
			CommandStarts = vkApi.Core.Configuration.GetArray($"Config:Groups:{vkApi.GroupId}:CommandStarts", new string[] { "/", "." });

			InitializePoolWorker();
		}

		public virtual void OnMessage(IUser sender, string message, BaseChat chat, Message messageData)
		{
			//защита от дублированных или задержанных сообщений
			{
				long msgId = messageData.ConversationMessageId.Value;
				if (!_lastMessages.ContainsKey(chat))
					_lastMessages.Add(chat, new Queue<long>());
				else
				{
					if (_lastMessages[chat].Contains(msgId)) return;
					_lastMessages[chat].Enqueue(msgId);
					if (_lastMessages[chat].Count > 10)
						_lastMessages[chat].Dequeue();
				}
			}
			//защита от дублированных или задержанных сообщений


			//actions
			if (messageData.Action != null && chat is Chat _chat)
			{
				if (messageData.Action.Type == MessageAction.ChatKickUser)
				{
					//if (messageData.Action.MemberId == -VkApi.GroupId) //нет события при кике бота.
					//	_chat.OnKick(sender);
					//else
					_chat.OnKickUser(VkApi.GetUser(messageData.Action.MemberId.Value), sender);
					return;
				}
				else if (messageData.Action.Type == MessageAction.ChatInviteUser)
				{
					if (messageData.Action.MemberId == -VkApi.GroupId)
						_chat.Join(sender);
					else
						_chat.OnAddUser(VkApi.GetUser(messageData.Action.MemberId.Value), sender, false);
					return;
				}
				else if (messageData.Action.Type == MessageAction.ChatInviteUserByLink)
				{
					_chat.OnAddUser(sender, null, true);
					return;
				}
			}

			if (sender is User user)
			{
				if (ClickButton(user, message, chat, messageData)) return;

				if (UseCommand(user, message, chat, messageData)) return;
			}

			if (!OnGetMessage(new GetMessageEventArgs(chat, sender, message, messageData))) return;

			chat.OnMessasge(sender, message, messageData);
		}

		private bool UseCommand(User user, string message, BaseChat chat, Message messageData)
		{
			var starts = CommandStarts.Select(s => Regex.Escape(s)).ToList();

			foreach (var start in starts.ToArray())
			{
				starts.Add($@"\[(public|club){VkApi.GroupId}\|[\S\s]{{1,}}](,|) {start}");
			}

			string regStart = @"\A(" + string.Join('|', starts) + @")";

			message = Regex.Match(message, regStart + @"[\S\s]{1,}", RegexOptions.IgnoreCase).Value;

			if (!string.IsNullOrEmpty(message))
			{
				try
				{
					message = Regex.Replace(message.Replace("ё", "е"), regStart, "", RegexOptions.IgnoreCase);

					VkApi.Core.PluginManager.HandleCommand(user, chat, message, messageData);
					return true;
				}
				catch (Exception e)
				{
					chat.SendMessageAsync("Команда задана неверно!");
					VkApi.Core.Log.Error(e.ToString());
				}
			}
			return false;
		}

		private bool ClickButton(User user, string message, BaseChat chat, Message messageData)
		{
			if (!string.IsNullOrEmpty(messageData.Payload))
			{
				try
				{
					var payload = KeyboardButtonPayload.Deserialize(messageData.Payload);
					if (payload != null && payload.IsValid())
					{
						if (payload.GroupId == VkApi.GroupId || payload.GroupId == 0)
							OnButtonClick(chat, user, message, payload, messageData);
						return true;
					}

				}
				catch (Exception e)
				{
					VkApi.Core.Log.Error(e.ToString());
				}
			}
			return false;
		}

		public virtual void OnButtonClick(BaseChat chat, User user, string message, KeyboardButtonPayload payload, Message messageData)
		{
			if (!OnButtonClick(new ButtonClickEventArgs(chat, user, message, payload, messageData))) return;
			chat.InvokeButton(user, payload);
		}

		internal bool ClickButton(BaseChat chat, User user, EventId eventId, string payload)
		{
			if (!string.IsNullOrEmpty(payload))
			{
				try
				{
					var _payload = KeyboardButtonPayload.Deserialize(payload);
					if (_payload != null && _payload.IsValid())
					{
						_payload.EventId = eventId;
						if (_payload.GroupId == VkApi.GroupId || _payload.GroupId == 0)
							OnButtonClick(chat, user, _payload);
						return true;
					}

				}
				catch (Exception e)
				{
					VkApi.Core.Log.Error(e.ToString());
				}
			}
			return false;
		}

		public virtual void OnButtonClick(BaseChat chat, User user, KeyboardButtonPayload payload)
		{
			if (!OnButtonClick(new ButtonClickEventArgs(chat, user, payload)))
				return;
			chat.InvokeButton(user, payload);
		}

		public void SendMessage(long peerId, string message, Keyboard keyboard = null, bool disableMentions = false)
		{
			SendMessage(new MessagesSendParams
			{
				RandomId = GetRandomId(),
				PeerId = peerId,
				Message = message,
				Keyboard = keyboard?.GetKeyboard(VkApi.GroupId),
				DisableMentions = disableMentions
			});
		}

		public async Task SendMessageAsync(long peerId, string message, Keyboard keyboard = null, bool disableMentions = false)
		{
			await SendMessageAsync(new MessagesSendParams
			{
				RandomId = GetRandomId(),
				PeerId = peerId,
				Message = message,
				Keyboard = keyboard?.GetKeyboard(VkApi.GroupId),
				DisableMentions = disableMentions
			});
		}

		public void SendMessageWithPool(long peerId, string message, Keyboard keyboard = null, bool disableMentions = false)
		{
			SendMessageWithPool(new MessagesSendParams
			{
				RandomId = GetRandomId(),
				PeerId = peerId,
				Message = message,
				Keyboard = keyboard?.GetKeyboard(VkApi.GroupId),
				DisableMentions = disableMentions
			});
		}

		public void SendMessage(MessagesSendParams message)
		{
			try
			{
				VkApi.Messages.Send(message);
			}
			catch (ConversationAccessDeniedException e)
			{
				long peerId = message.PeerId.Value;
				if (!BaseChat.IsUserConversation(peerId))
				{
					VkApi.GetChat<Chat>(peerId).OnKick(null);
					return;
				}
				throw e;
			}
		}

		public async Task SendMessageAsync(MessagesSendParams message)
		{
			try
			{
				await VkApi.Messages.SendAsync(message);
			}
			catch (Exception e)
			{
				long peerId = message.PeerId.Value;
				if (!BaseChat.IsUserConversation(peerId))
				{
					VkApi.GetChat<Chat>(peerId).OnKick(null);
				}
				throw e;
			}
		}

		public void SendMessageWithPool(MessagesSendParams message)
		{
			_sendPool.Add(message);
			if (!_poolTimer.Enabled)
				_poolTimer.Start();
		}

		public async Task<bool> SendMessageEventAnswerAsync(EventId eventId, long userId, long peerId, EventData eventData)
		{
			try
			{
				string _eventId;
				lock (eventId)
				{
					if (string.IsNullOrEmpty(eventId)) return false;

					_eventId = eventId;
					eventId.Clear();
				}

				return await VkApi.Messages.SendMessageEventAnswerAsync(_eventId, userId, peerId, eventData);
			}
			catch (ConversationAccessDeniedException e)
			{
				if (!BaseChat.IsUserConversation(peerId))
				{
					VkApi.GetChat<Chat>(peerId).OnKick(null);
					return false;
				}
				throw e;
			}
		}

		public async Task<bool> EditAsync(long peerId, long messageId, string message, Keyboard keyboard = null)
		{
			try
			{
				return await VkApi.Messages.EditAsync(new MessageEditParams() 
				{
					PeerId = peerId,
					ConversationMessageId = messageId,
					Message = message,
					Keyboard = keyboard.GetKeyboard(VkApi.GroupId)
				});
			}
			catch (ConversationAccessDeniedException e)
			{
				if (!BaseChat.IsUserConversation(peerId))
				{
					VkApi.GetChat<Chat>(peerId).OnKick(null);
					return false;
				}
				throw e;
			}
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

	public class GetMessageEventArgs : GetMessageEventArgs<IUser>
	{
		public GetMessageEventArgs(BaseChat chat, IUser sender, string message, Message messageData) : base(chat, sender, message, messageData)
		{
		}
	}

	public class GetMessageEventArgs<TUser> : EventArgs where TUser : IUser
	{
		public bool Cancel { get; set; }

		public BaseChat Chat { get; set; }

		public TUser Sender { get; set; }

		public string Message { get; set; }

		public Message MessageData { get; set; }

		public GetMessageEventArgs(BaseChat chat, TUser sender, string message, Message messageData)
		{
			Chat = chat;
			Sender = sender;
			Message = message;
			MessageData = messageData;
		}
	}
}
