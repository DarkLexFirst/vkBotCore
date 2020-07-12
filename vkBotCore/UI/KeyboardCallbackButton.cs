using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkBotCore.Subjects;
using EventData = VkNet.Model.EventData;

namespace VkBotCore.UI
{
	public class KeyboardCallbackButton : BaseCallbackButton
	{
		/// <summary>
		/// Событие, вызываемое после нажатия кнопки.
		/// </summary>
		public Func<BaseChat, User, KeyboardCallbackButton, KeyboardButtonPayload, EventData> Action { get; private set; }

		public KeyboardCallbackButton(string label, Func<BaseChat, User, KeyboardCallbackButton, KeyboardButtonPayload, EventData> action) : base(CallbackButtonType.Callback, label)
		{
			Label = label;
			Action = action;
		}

		public KeyboardCallbackButton(string label, Action<BaseChat, User, KeyboardCallbackButton, KeyboardButtonPayload> action) : base(CallbackButtonType.Callback, label)
		{
			Label = label;
			Action = (chat, user, keyboard, payload) =>
			{
				action?.Invoke(chat, user, keyboard, payload);
				return null;
			};
		}

		public KeyboardCallbackButton(string id, string label) : base(CallbackButtonType.Callback, id, label)
		{
			Id = id;
			Label = label;
		}
	}
}
