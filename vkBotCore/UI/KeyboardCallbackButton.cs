using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkBotCore.Subjects;

namespace VkBotCore.UI
{
	public class KeyboardCallbackButton : BaseCallbackButton
	{
		/// <summary>
		/// Событие, вызываемое после нажатия кнопки.
		/// </summary>
		public Func<BaseChat, User, KeyboardCallbackButton, KeyboardButtonPayload, string> Action { get; private set; }

		public KeyboardCallbackButton(string label, Func<BaseChat, User, KeyboardCallbackButton, KeyboardButtonPayload, string> action) : base(CallbackButtonType.Callback, label)
		{
			Label = label;
			Action = action;
		}

		public KeyboardCallbackButton(string id, string label) : base(CallbackButtonType.Callback, id, label)
		{
			Id = id;
			Label = label;
		}
	}
}
