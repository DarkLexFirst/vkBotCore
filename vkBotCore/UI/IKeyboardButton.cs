using VkBotCore.Subjects;
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

		public KeyboardButtonPayload Payload { get; set; }

		public ButtonClickEventArgs(BaseChat chat, User sender, string message, KeyboardButtonPayload payload, Message messageData) : base(chat, sender, message, messageData)
		{
			Payload = payload;
		}
	}
}
