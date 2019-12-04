using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkNet.Model;

namespace vkBotCore.Plugins
{
    public class CommandContext
    {
        public BotCore Core { get; set; }
        public User Sender { get; set; }
        public Chat Chat { get; set; }
        public Message MessageData { get; set; }

        public CommandContext(BotCore core, User sender, Chat chat, Message messageData)
        {
            Core = core;
            Sender = sender;
            Chat = chat;
            MessageData = messageData;
        }
    }
}
