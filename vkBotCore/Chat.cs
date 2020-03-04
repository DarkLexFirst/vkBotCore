using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vkBotCore.Plugins;
using vkBotCore.UI;
using VkNet.Abstractions;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Attachments;

namespace vkBotCore
{
    public class Chat : IEquatable<Chat>
    {
        public VkCoreApiBase VkApi { get; set; }
        public long PeerId { get; set; }

        public bool IsUserConversation { get => PeerId < 2000000000; }

        private Dictionary<string, Keyboard> _cachedKeyboards { get; set; }

        public Keyboard BaseKeyboard { get; set; }

        public Chat(VkCoreApiBase vkApi, long peerId)
        {
            VkApi = vkApi;
            PeerId = peerId;
            _cachedKeyboards = new Dictionary<string, Keyboard>();
        }

        public virtual void OnMessasge(User user, string message, Message messageData)
        {

        }

        public virtual void OnCommand(User user, string command, string[] args, Message messageData)
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
            if (!string.IsNullOrEmpty(message))
                VkApi.MessageHandler.SendMessage(message, PeerId, BaseKeyboard);
        }

        public void SendMessage(object obj)
        {
            SendMessage(obj?.ToString());
        }

        public void SendKeyboard(string keyboardId)
        {
            if (_cachedKeyboards.ContainsKey(keyboardId))
                SendKeyboard(_cachedKeyboards[keyboardId]);
        }

        public void SendKeyboard(Keyboard keyboard)
        {
            AddKeyboard(keyboard);
            VkApi.MessageHandler.SendKeyboard(keyboard, PeerId);
        }

        public void AddKeyboard(Keyboard keyboard)
        {
            if (!_cachedKeyboards.ContainsKey(keyboard.Id))
                _cachedKeyboards.Add(keyboard.Id, keyboard);
            else
                _cachedKeyboards[keyboard.Id] = keyboard;
        }

        public void InvokeButton(User user, string keyboardId, string buttonId)
        {
            if(BaseKeyboard?.Id == keyboardId)
            {
                BaseKeyboard.TryInvokeButton(this, user, buttonId);
                return;
            }
            if (_cachedKeyboards.ContainsKey(keyboardId))
            {
                var keyboard = _cachedKeyboards[keyboardId];

                if (keyboard.OneTime)
                    _cachedKeyboards.Remove(keyboardId);

                keyboard.TryInvokeButton(this, user, buttonId);
            }
        }

        public bool RemoveMessage(ulong id)
        {
            return VkApi.MessageHandler.DeleteMessage(id);
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

        public override bool Equals(object obj) => obj is Chat user && Equals(user);
        public bool Equals(Chat other) => other.PeerId == PeerId && other.VkApi.Equals(VkApi);

        public override int GetHashCode() => (int)PeerId;
    }

    public class ChatEventArgs : EventArgs
    {
        public Chat Chat { get; }

        public ChatEventArgs(Chat chat)
        {
            Chat = chat;
        }
    }
}
