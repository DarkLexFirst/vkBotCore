using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkNet.Model.Keyboard;
using VkNet.Enums.SafetyEnums;
using VkBotCore.Subjects;

namespace VkBotCore.UI
{
	public class KeyboardTextButton : BaseCallbackButton
	{
		/// <summary>
		/// Событие, вызываемое после нажатия кнопки.
		/// </summary>
		public Action<BaseChat, User, KeyboardTextButton, KeyboardButtonPayload> Action { get; private set; }

		public KeyboardTextButton(string label, Action<BaseChat, User, KeyboardTextButton, KeyboardButtonPayload> action) : base(CallbackButtonType.Text, label)
		{
			Label = label;
			Action = action;
		}

		public KeyboardTextButton(string id, string label) : base(CallbackButtonType.Text, id, label)
		{
			Id = id;
			Label = label;
		}
	}
}
