using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkNet.Model.Keyboard;
using VkNet.Enums.SafetyEnums;
using VkBotCore.Subjects;

namespace VkBotCore.UI
{
	public class KeyboardTextButton : IKeyboardButton
	{
		public string Id { get; set; }

		/// <summary>
		/// Текст кнопки.
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Дополнительная информация.
		/// </summary>
		public string Payload { get; set; }

		/// <summary>
		/// Событие, вызываемое после нажатия кнопки.
		/// </summary>
		public Action<BaseChat, User, KeyboardTextButton> Action { get; private set; }

		/// <summary>
		/// Цвет кнопки.
		/// </summary>
		public ButtonColor Color { get; set; } = ButtonColor.White;

		public KeyboardTextButton(string label, Action<BaseChat, User, KeyboardTextButton> action)
		{
			Label = label;
			Action = action;
		}

		public KeyboardTextButton(string id, string label)
		{
			Id = id;
			Label = label;
		}

		public MessageKeyboardButton GetButton(Keyboard keyboard, long groupId)
		{
			KeyboardButtonColor color = KeyboardButtonColor.Default;
			switch (Color)
			{
				case ButtonColor.Blue:
					color = KeyboardButtonColor.Primary;
					break;
				case ButtonColor.Green:
					color = KeyboardButtonColor.Positive;
					break;
				case ButtonColor.Red:
					color = KeyboardButtonColor.Negative;
					break;
			}

			var action = new MessageKeyboardButtonAction();
			action.Type = KeyboardButtonActionType.Text;
			action.Label = Label;

			var payload = new KeyboardButtonPayload()
			{
				GroupId = groupId,
				KeyboardId = keyboard.Id,
				ButtonId = Id,
				Payload = Payload
			};

			action.Payload = payload.Serialize();

			return new MessageKeyboardButton() { Color = color, Action = action };
		}
	}

	public enum ButtonColor
	{
		Blue,
		White,
		Green,
		Red
	}
}
