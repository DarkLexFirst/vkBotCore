using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkNet.Model.Keyboard;

namespace VkBotCore.UI
{
    public class Keyboard
    {
        /// <summary>
        /// Сообщение, которое отправляется при отображении клавиатуры.
        /// </summary>
        public string Message { get; set; }

        private List<List<IKeyboardButton>> _buttons = new List<List<IKeyboardButton>>() { new List<IKeyboardButton>() };

        /// <summary>
        /// Идентификатор клавиатуры.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Скрывать клавиатуру после использования (не работает в сообщениях).
        /// </summary>
        public bool OneTime { get; set; } = false;
        /// <summary>
        /// Отобразить клавиатуру в сообщении.
        /// </summary>
        public bool InMessage { get; set; } = false;

        /// <summary>
        /// Определяет, наполнена ли клавиатура.
        /// </summary>
        public bool IsEmpty { get => _buttons.All(l => l.Count == 0); }

        public Keyboard(string message)
        {
            Id = _lastKeyboardId++.ToString();
            Message = message;
        }

        /// <summary>
        /// Добавляет кнопку в последнюю строку.
        /// </summary>
        public void Add(IKeyboardButton button)
        {
            if (string.IsNullOrEmpty(button.Id)) button.Id = GetButtonId();
            _buttons.Last().Add(button);
        }

        /// <summary>
        /// Добавляет кнопку в новую строку.
        /// </summary>
        public void AddOnNewLine(IKeyboardButton button)
        {
            AddNewLine();
            Add(button);
        }

        /// <summary>
        /// Добавляет новую строку.
        /// </summary>
        public void AddNewLine()
        {
            if (_buttons.Last().Count == 0) return;
            _buttons.Add(new List<IKeyboardButton>());
        }

        internal MessageKeyboard GetKeyboard()
        {
            if (IsEmpty) throw new KeyboardEmptyException();

            _buttons.RemoveAll(l => l.Count == 0);

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

    public class KeyboardEmptyException : Exception
    {
        public KeyboardEmptyException() : base("Keyboard is empty") { }
    }
}
