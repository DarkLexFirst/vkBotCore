using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkBotCore.Subjects;
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

		internal MessageKeyboard GetKeyboard(long groupId)
		{
			if (IsEmpty) throw new KeyboardEmptyException();

			_buttons.RemoveAll(l => l.Count == 0);

			var keyboard = new MessageKeyboard();
			keyboard.Buttons = _buttons.Select(line => line.Select(b => b.GetButton(this, groupId)));
			keyboard.Inline = InMessage;
			keyboard.OneTime = OneTime && !InMessage;
			return keyboard;
		}

		internal void TryInvokeButton(BaseChat chat, User user, KeyboardButtonPayload payload)
		{
			if (payload.ButtonId == null) return;
			foreach (var line in _buttons)
			{
				var button = line.FirstOrDefault(b => b.Id == payload.ButtonId);
				if (button == null) continue;

				if (button is KeyboardTextButton textButton)
				{
					textButton.Action?.Invoke(chat, user, textButton, payload);
				}
				else if (button is KeyboardCallbackButton callbackButton)
				{
					var eventData = callbackButton.Action?.Invoke(chat, user, callbackButton, payload);
					if (!string.IsNullOrEmpty(payload.EventId))
					{
						chat.VkApi.MessageHandler.SendMessageEventAnswerAsync(chat.PeerId,
							payload.EventId,
							user.Id,
							eventData);
					}
				}
			}
		}

		private static long _lastKeyboardId = 0;
		private byte _lastButtonId = 0;
		internal string GetButtonId()
		{
			return _lastButtonId++.ToString();
		}
	}

	public class KeyboardEmptyException : Exception
	{
		public KeyboardEmptyException() : base("Keyboard is empty") { }
	}
}
