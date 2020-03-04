using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkNet.Model.Keyboard;

namespace vkBotCore.UI
{
    public class Keyboard
    {
        public string Message { get; set; }

        private List<List<BaseKeyboardButton>> _buttons = new List<List<BaseKeyboardButton>>() { new List<BaseKeyboardButton>() };

        public string Id { get; set; }

        public bool OneTime { get; set; } = false;
        public bool InMessage { get; set; } = false;

        public Keyboard(string message)
        {
            Id = _lastKeyboardId++.ToString();
            Message = message;
        }

        public void Add(BaseKeyboardButton button)
        {
            if (string.IsNullOrEmpty(button.Id)) button.Id = GetButtonId();
            _buttons.Last().Add(button);
        }

        public void AddOnNewLine(BaseKeyboardButton button)
        {
            AddNewLine();
            Add(button);
        }

        public void AddNewLine()
        {
            if (_buttons.Last().Count == 0) return;
            _buttons.Add(new List<BaseKeyboardButton>());
        }

        internal MessageKeyboard GetKeyboard()
        {
            if (_buttons.All(line => line.Count == 0)) throw new KeyboardEmptyException();

            MessageKeyboard keyboard = new MessageKeyboard();
            keyboard.Buttons = _buttons.Select(line => line.Select(b => b.GetButton(this)));
            keyboard.Inline = InMessage;
            keyboard.OneTime = OneTime && !InMessage;
            return keyboard;
        }

        internal void TryInvokeButton(Chat chat, User user, string buttonId)
        {
            if (buttonId == null) return;
            foreach (var line in _buttons)
            {
                var button = line.FirstOrDefault(b => b.Id == buttonId);
                if (button == null) return;

                if (button is KeyboardTextButton textButton)
                    textButton.Action?.Invoke(chat, user);
            }
        }

        private static long _lastKeyboardId = 0;
        private byte _lastButtonId = 0;
        public string GetButtonId()
        {
            return _lastButtonId++.ToString();
        }
    }

    public interface BaseKeyboardButton
    {
        string Id { get; set; }
        MessageKeyboardButton GetButton(Keyboard keyboard);
    }

    public class KeyboardEmptyException : Exception
    {
        public KeyboardEmptyException() : base("Keyboard is empty") { }
    }
}
