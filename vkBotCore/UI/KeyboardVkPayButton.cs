using VkNet.Enums.SafetyEnums;
using VkNet.Model.Keyboard;
using VkBotCore.VKPay;

namespace VkBotCore.UI
{
	public class KeyboardVkPayButton : IKeyboardButton
	{
		public string Id { get; set; }

		/// <summary>
		/// VkPay.
		/// </summary>
		public VkPay VkPay { get; set; }

		public KeyboardVkPayButton(VkPay vkPay)
		{
			VkPay = vkPay;
		}

		public MessageKeyboardButton GetButton(Keyboard keyboard, long groupId)
		{
			var action = new MessageKeyboardButtonAction();
			action.Type = KeyboardButtonActionType.VkPay;
			action.Hash = VkPay.ToString();

			return new MessageKeyboardButton() { Action = action };
		}
	}
}
