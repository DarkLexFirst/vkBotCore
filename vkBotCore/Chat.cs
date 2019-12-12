using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vkBotCore.Plugins;
using VkNet.Abstractions;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Attachments;

namespace vkBotCore
{
    public class Chat
    {
        public VkCoreApiBase VkApi { get; set; }
        public long PeerId { get; set; }

        public Chat(VkCoreApiBase vkApi, long peerId)
        {
            VkApi = vkApi;
            PeerId = peerId;
        }

        public virtual void OnMessasge(User user, string message, Message messageData, long groupId)
        {

        }

        public virtual void OnCommand(User user, string command, string[] args, Message messageData, long groupId)
        {
            
        }

        public virtual void SendMessage(string message)
        {
            VkApi.MessageHandler.SendMessage(message, PeerId);
        }

        public void SendMessage(object obj)
        {
            SendMessage(obj.ToString());
        }

        public long[] GetAllChatAdministrators()
        {
            var members = VkApi.Messages.GetConversationMembers(PeerId, new List<string>()).Items;
            return members.Where(m => m.IsAdmin).Select(m => m.MemberId).ToArray();
        }

        public long[] GetAllChatMembers()
        {
            var members = VkApi.Messages.GetConversationMembers(PeerId, new List<string>()).Profiles;
            return members.Select(m => m.Id).ToArray();
        }

        public string GetEveryoneMentionLine(string val)
        {
            return string.Join("", GetAllChatMembers().Select(m => User.GetMentionLine(m, val)));
        }
    }
}
