﻿using VkBotCore.Subjects;
using VkNet.Model.Keyboard;
using Message = VkNet.Model.Message;

namespace VkBotCore.UI
{
	public interface IKeyboardButton
	{
		string Id { get; set; }

		MessageKeyboardButton GetButton(Keyboard keyboard, long groupId);
	}

	public class ButtonClickEventArgs : GetMessageEventArgs<User>
	{
		public string EventId { get; set; }

		public KeyboardButtonPayload Payload { get; set; }

		public ButtonClickEventArgs(BaseChat chat, User sender, string message, KeyboardButtonPayload payload, Message messageData) : base(chat, sender, message, messageData)
		{
			Payload = payload;
		}
		public ButtonClickEventArgs(BaseChat chat, User sender, string eventId, KeyboardButtonPayload payload) : base(chat, sender, null, null)
		{
			EventId = eventId;
			Payload = payload;
		}
	}
}
