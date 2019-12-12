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
        public VkCoreApiBase VkApi { get; set; }

        public MessageHandler(VkCoreApiBase vkApi)
        {
            VkApi = vkApi;
        }

        public virtual void OnMessage(User user, string message, long peerId, Message messageData, long groupId)
        {
            var chat = VkApi.GetChat(peerId);
            if ((message.StartsWith("/") || message.StartsWith(".")) && message.Length != 1)
            {
                try
                {
                    message = message.Replace("ё", "е");
                    VkApi.Core.PluginManager.HandleCommand(user, chat, message, messageData, groupId);

                    message = message.Substring(1);
                    var args = message.Split(' ').ToList();
                    string commandName = args.First();
                    args.Remove(commandName);

                    chat.OnCommand(user, commandName.ToLower(), args.ToArray(), messageData, groupId);
                }
                catch(Exception e)
                {
                    chat.SendMessage("Комманда задана неверно!");
                    VkApi.Core.Log.Error(e.ToString());
                }
                return;
            }

            chat.OnMessasge(user, message, messageData, groupId);
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
            VkApi.Messages.Send(message);
        }

        public void SendSticker(MessagesSendStickerParams message)
        {
            VkApi.Messages.SendSticker(message);
        }
    }
}
