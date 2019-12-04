using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace vkBotCore
{
    public class MessageHandler
    {
        public BotCore Core { get; set; }

        public MessageHandler(BotCore core)
        {
            Core = core;
        }

        public virtual void OnMessage(User user, string message, long peerId, Message messageData, string url)
        {
            var chat = Core.GetChat(peerId);
            if ((message.StartsWith("/") || message.StartsWith(".")) && message.Length != 1)
            {
                try
                {
                    message = message.Replace("ё", "е");
                    Core.PluginManager.HandleCommand(user, chat, message, messageData, url);

                    message = message.Substring(1);
                    var args = message.Split(' ').ToList();
                    string commandName = args.First();
                    args.Remove(commandName);

                    chat.OnCommand(user, commandName.ToLower(), args.ToArray(), messageData, url);
                }
                catch(Exception e)
                {
                    chat.SendMessage("Комманда задана неверно!");
                    Core.Log.Error(e.ToString());
                }
                return;
            }

            chat.OnMessasge(user, message, messageData, url);
        }

        public void SendMessage(string message, long peerId)
        {
            SendMessage(new MessagesSendParams
            {
                RandomId = new DateTime().Millisecond,
                PeerId = peerId,
                Message = message
            });
        }

        public void SendMessage(MessagesSendParams message)
        {
            Core.VkApi.Messages.Send(message);
        }

        public void SendSticker(MessagesSendStickerParams message)
        {
            Core.VkApi.Messages.SendSticker(message);
        }
    }
}
