using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkNet.Model.Keyboard;
using VkNet.Enums.SafetyEnums;

namespace vkBotCore.UI
{
    public class KeyboardTextButton : BaseKeyboardButton
    {
        public string Id { get; set; }

        public string Label { get; set; }

        public Action<Chat, User> Action { get; private set; }


        public ButtonColor Color { get; set; } = ButtonColor.White;

        public KeyboardTextButton(string label, Action<Chat, User> action)
        {
            Label = label;
            Action = action;
        }

        public KeyboardTextButton(string id, string label)
        {
            Id = id;
            Label = label;
        }

        public MessageKeyboardButton GetButton(Keyboard keyboard)
        {
            KeyboardButtonColor color = KeyboardButtonColor.Default;
            switch(Color)
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

            MessageKeyboardButtonAction action = new MessageKeyboardButtonAction();
            action.Type = KeyboardButtonActionType.Text;
            action.Label = Label;

            action.Payload = $"{{\"button\": \"{keyboard.Id}:{Id}\"}}";

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
