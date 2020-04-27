using VkBotCore.Subjects;
using Message = VkNet.Model.Message;

namespace VkBotCore.Plugins
{
	public class CommandContext
	{
		public BotCore Core { get; set; }
		public User Sender { get; set; }
		public BaseChat Chat { get; set; }
		public Message MessageData { get; set; }

		public CommandContext(BotCore core, User sender, BaseChat chat, Message messageData)
		{
			Core = core;
			Sender = sender;
			Chat = chat;
			MessageData = messageData;
		}
	}
}
