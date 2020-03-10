using VkNet.Enums.SafetyEnums;
using VkNet.Model.Keyboard;
using vkBotCore.VKPay;

namespace vkBotCore.UI
{
    public class KeyboardVkPayButton : BaseKeyboardButton
    {
        public string Id { get; set; }

        public VkPay VkPay { get; set; }

        public KeyboardVkPayButton(VkPay vkPay)
        {
            VkPay = vkPay;
        }

        public MessageKeyboardButton GetButton(Keyboard keyboard)
        {
            MessageKeyboardButtonAction action = new MessageKeyboardButtonAction();
            action.Type = KeyboardButtonActionType.VkPay;
            action.Hash = VkPay.ToString();

            return new MessageKeyboardButton() { Action = action };
        }
    }
}
