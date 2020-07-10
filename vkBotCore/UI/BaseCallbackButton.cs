using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkBotCore.Subjects;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Keyboard;

namespace VkBotCore.UI
{
	public abstract class BaseCallbackButton : IKeyboardButton
	{
		public string Id { get; set; }
		
		private CallbackButtonType ButtonType { get; set; }

		/// <summary>
		/// Текст кнопки.
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Дополнительная информация.
		/// </summary>
		public string Payload { get; set; }

		/// <summary>
		/// Цвет кнопки.
		/// </summary>
		public ButtonColor Color { get; set; } = ButtonColor.White;

		public BaseCallbackButton(CallbackButtonType buttonType, string label)
		{
			ButtonType = buttonType;
			Label = label;
		}

		public BaseCallbackButton(CallbackButtonType buttonType, string id, string label)
		{
			ButtonType = buttonType;
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
			action.Label = Label;

			switch (ButtonType)
			{
				case CallbackButtonType.Callback:
					action.Type = KeyboardButtonActionType.RegisterPossibleValue("callback");
					break;
				case CallbackButtonType.Text:
					action.Type = KeyboardButtonActionType.Text;
					break;
			}

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
	
	public enum CallbackButtonType
	{
		Callback,
		Text
	}
}
