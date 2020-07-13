using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Keyboard;

namespace VkBotCore.UI
{
	public class KeyboardLinkButton : IKeyboardButton
	{
		public string Id { get; set; }

		/// <summary>
		/// Текст кнопки.
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Ссылка.
		/// </summary>
		public string Link { get; set; }

		public KeyboardLinkButton(string label, string link)
		{
			Label = label;
			Link = link;
		}

		public MessageKeyboardButton GetButton(Keyboard keyboard, long groupId)
		{
			var action = new MessageKeyboardButtonAction()
			{
				Type = KeyboardButtonActionType.OpenLink,
				Label = Label,
				Link = new Uri(Link)
			};

			return new MessageKeyboardButton() { Action = action };
		}
	}
}
