using Microsoft.Extensions.Configuration;
using System;

namespace VkBotCore.Subjects
{
	public class LogChat : Chat
	{
		protected string TypeFormat { get; set; } = "[{0}] {1}";
		protected string TypeFormat_Chat { get; set; } = "[{0}] Sended from {1}\n{2}";

		protected LogValueType MinValue { get; set; }
		protected LogValueType MaxValue { get; set; }

		public LogChat(VkCoreApiBase vkApi) : base(vkApi, vkApi.Core.Configuration.GetValue<long>("Config:Log:ChatId", -1))
		{
			MinValue = Enum.Parse<LogValueType>(VkApi.Core.Configuration["Config:Log:MinValue"]);
			MaxValue = Enum.Parse<LogValueType>(VkApi.Core.Configuration["Config:Log:MaxValue"]);
		}

		public void Debug(BaseChat chat, string message, params object[] args) => SendMessage(chat, message, LogValueType.DEBUG, args);
		public void Debug(string message, params object[] args) => SendMessage(message, LogValueType.DEBUG, args);

		public void Info(BaseChat chat, string message, params object[] args) => SendMessage(chat, message, LogValueType.INFO, args);
		public void Info(string message, params object[] args) => SendMessage(message, LogValueType.INFO, args);

		public void Warn(BaseChat chat, string message, params object[] args) => SendMessage(chat, message, LogValueType.WARN, args);
		public void Warn(string message, params object[] args) => SendMessage(message, LogValueType.WARN, args);

		public void Error(BaseChat chat, string message, params object[] args) => SendMessage(chat, message, LogValueType.ERROR, args);
		public void Error(string message, params object[] args) => SendMessage(message, LogValueType.ERROR, args);

		public void Fatal(BaseChat chat, string message, params object[] args) => SendMessage(chat, message, LogValueType.FATAL, args);
		public void Fatal(string message, params object[] args) => SendMessage(message, LogValueType.FATAL, args);

		private void SendMessage(BaseChat chat, string message, LogValueType value, params object[] args)
		{
			if (MinValue <= value && MaxValue >= value)
				SendMessage(string.Format(TypeFormat_Chat, value, chat.PeerId, string.Format(message, args)));
		}

		private void SendMessage(string message, LogValueType value, params object[] args)
		{
			if (MinValue <= value && MaxValue >= value)
				SendMessage(string.Format(TypeFormat, value, string.Format(message, args)));
		}

		public override void SendMessage(string message, bool disableMentions = false)
		{
			Console.WriteLine(message);
			if (PeerId == -1) return;
			base.SendMessageAsync(message);
		}

		protected enum LogValueType
		{
			DEBUG,
			INFO,
			WARN,
			ERROR,
			FATAL
		}
	}
}
