using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using vkBotCore.UI;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace vkBotCore
{
    public class MessageHandler
    {
        public VkCoreApiBase VkApi { get; set; }

        private Dictionary<Chat, Queue<long>> _lastMessages;

        public MessageHandler(VkCoreApiBase vkApi)
        {
            VkApi = vkApi;

            _lastMessages = new Dictionary<Chat, Queue<long>>();
        }

        public virtual void OnMessage(User user, string message, long peerId, Message messageData)
        {
            var chat = VkApi.GetChat(peerId);

            var msgId = messageData.ConversationMessageId.Value;
            if (!_lastMessages.ContainsKey(chat))
                _lastMessages.Add(chat, new Queue<long>());
            else
            {
                if (_lastMessages[chat].Contains(msgId)) return;
                _lastMessages[chat].Enqueue(msgId);
                if (_lastMessages[chat].Count > 10)
                    _lastMessages[chat].Dequeue();
            }

            if (!string.IsNullOrEmpty(messageData.Payload))
            {
                try
                {
                    var payload = JsonConvert.DeserializeObject<TextButtonPayload>(messageData.Payload);
                    var s = payload.Button.Split(':');
                    OnButtonClick(chat, user, message, s[0], s.Length == 1 ? "0" : s[1], messageData);
                    return;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            if ((message.StartsWith("/") || message.StartsWith(".")) && message.Length != 1)
            {
                try
                {
                    message = message.Replace("ё", "е");
                    VkApi.Core.PluginManager.HandleCommand(user, chat, message, messageData);

                    message = message.Substring(1);
                    var args = message.Split(' ').ToList();
                    string commandName = args.First();
                    args.Remove(commandName);

                    chat.OnCommand(user, commandName.ToLower(), args.ToArray(), messageData);
                }
                catch(Exception e)
                {
                    chat.SendMessage("Комманда задана неверно!");
                    VkApi.Core.Log.Error(e.ToString());
                }
                return;
            }

            if (!OnGetMessage(new GetMessageEventArgs(chat, user, message, messageData))) return;
            chat.OnMessasge(user, message, messageData);
        }

        public virtual void OnButtonClick(Chat chat, User user, string message, string keyboardId, string buttonId, Message messageData)
        {
            if (!OnButtonClick(new ButtonClickEventArgs(chat, user, message, keyboardId, buttonId, messageData))) return;
            chat.InvokeButton(user, keyboardId, buttonId);
        }

        public void SendMessage(string message, long peerId, Keyboard keyboard = null)
        {
            SendMessage(new MessagesSendParams
            {
                RandomId = DateTime.Now.Second * DateTime.Now.Millisecond,
                PeerId = peerId,
                Message = message,
                Keyboard = keyboard?.GetKeyboard()
            });
        }

        public bool DeleteMessage(ulong id)
        {
            return VkApi.Messages.Delete(new ulong[] { id }, false, (ulong)VkApi.GroupId, true).First().Value;
        }

        public void SendMessage(MessagesSendParams message)
        {
            VkApi.Messages.Send(message);
        }

        public void SendSticker(MessagesSendStickerParams message)
        {
            VkApi.Messages.SendSticker(message);
        }

        public void SendKeyboard(Keyboard keyboard, long peerId)
        {
            SendMessage(new MessagesSendParams
            {
                RandomId = DateTime.Now.Second * DateTime.Now.Millisecond,
                PeerId = peerId,
                Message = keyboard.Message,
                Keyboard = keyboard.GetKeyboard()
            });
        }

        public event EventHandler<GetMessageEventArgs> GetMessage;

        protected virtual bool OnGetMessage(GetMessageEventArgs e)
        {
            GetMessage?.Invoke(this, e);

            return !e.Cancel;
        }

        public event EventHandler<ButtonClickEventArgs> ButtonClick;

        protected virtual bool OnButtonClick(ButtonClickEventArgs e)
        {
            ButtonClick?.Invoke(this, e);

            return !e.Cancel;
        }
    }

    public class GetMessageEventArgs : EventArgs
    {
        public bool Cancel { get; set; }

        public Chat Chat { get; set; }

        public User Sender { get; set; }

        public string Message { get; set; }

        public Message MessageData {get;set;}

        public GetMessageEventArgs(Chat chat, User sender, string message, Message messageData)
        {
            Chat = chat;
            Sender = sender;
            Message = message;
            MessageData = messageData;
        }
    }

    public class ButtonClickEventArgs : GetMessageEventArgs
    {

        public string KeyboardId { get; set; }

        public string ButtonId { get; set; }

        public ButtonClickEventArgs(Chat chat, User sender, string message, string keyboardId, string buttonId, Message messageData) : base(chat, sender, message, messageData)
        {
            KeyboardId = keyboardId;
            ButtonId = buttonId;
        }
    }
}
