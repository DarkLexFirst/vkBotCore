using VkBotCore.Subjects;
using VkNet.Model.Keyboard;
using Message = VkNet.Model.Message;

namespace VkBotCore.UI
{
	public interface IKeyboardButton
	{
		string Id { get; set; }

		MessageKeyboardButton GetButton(Keyboard keyboard);
	}

	public class ButtonClickEventArgs : GetMessageEventArgs<User>
	{

		public string KeyboardId { get; set; }

		public string ButtonId { get; set; }

		public ButtonClickEventArgs(BaseChat chat, User sender, string message, string keyboardId, string buttonId, Message messageData) : base(chat, sender, message, messageData)
		{
			KeyboardId = keyboardId;
			ButtonId = buttonId;
		}
	}
}
