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

        public void Pin(long messageId)
        {
            VkApi.Messages.Pin(PeerId, (ulong)messageId);
        }

        public void Unpin()
        {
            VkApi.Messages.Unpin(PeerId);
        }

        public bool TryKick(User user)
        {
            return TryKick(user.Id);
        }

        public bool TryKick(long id)
        {
            try { Kick(id); } catch { return false; }
            return true;
        }

        public void Kick(long id)
        {
            VkApi.Messages.RemoveChatUser((ulong)PeerId % 2000000000, id);
        }

        public virtual void SendMessage(string message)
        {
            VkApi.MessageHandler.SendMessage(message, PeerId);
        }

        public void SendMessage(object obj)
        {
            SendMessage(obj.ToString());
        }

        public bool HavePermissions()
        {
            return GetAllChatAdministrators().Contains(VkApi.GroupId);
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

        public IEnumerable<string> GetEveryoneMentions(string val)
        {
            return GetAllChatMembers().Select(m => User.GetMentionLine(m, val));
        }
    }
}
