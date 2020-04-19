using VkNet.Model.Keyboard;

namespace VkBotCore.UI
{
	public interface IKeyboardButton
	{
		string Id { get; set; }

		MessageKeyboardButton GetButton(Keyboard keyboard);
	}
}
