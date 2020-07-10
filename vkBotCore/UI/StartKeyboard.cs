using System;
using VkBotCore.Subjects;

namespace VkBotCore.UI
{
	/// <summary>
	/// Клавиатура для обработки событий стандартной кнопки "Начать". (Данную клавиатуру нельзя отправить)
	/// </summary>
	public class StartKeyboard : Keyboard
	{
		private const string StartCommand = "start";

		public StartKeyboard(Action<BaseChat, User, KeyboardTextButton, KeyboardButtonPayload> action) : base(null)
		{
			Id = StartCommand;
			Add(new KeyboardTextButton("Старт", action) { Id = StartCommand });
		}
	}
}
