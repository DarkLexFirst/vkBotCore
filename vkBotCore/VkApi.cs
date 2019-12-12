using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkNet;
using VkNet.Abstractions;
using VkNet.Model;

namespace vkBotCore
{
    public class VkCoreApi : VkCoreApiBase
    {
        Dictionary<long, VkCoreApiBase> _vkApi { get; set; }

        public VkCoreApi(BotCore core) : base(core, 0)
        {
            _vkApi = new Dictionary<long, VkCoreApiBase>();

            var accesToken = Core.Configuration.GetValue<string>($"Config:AccessToken", null);
            if (accesToken != null)
                Authorize(new ApiAuthParams { AccessToken = accesToken });
        }

        public VkCoreApiBase Get(long groupId)
        {
            if (_vkApi.ContainsKey(groupId))
                return _vkApi[groupId];
            var api = new VkCoreApiBase(Core, groupId);
            var accesToken = Core.Configuration.GetValue<string>($"Config:Groups:{groupId}:AccessToken", null);
            if (accesToken == null)
                return this;
            api.Authorize(new ApiAuthParams { AccessToken = accesToken });
            _vkApi.Add(groupId, api);
            return api;
        }
    }

    public class VkCoreApiBase : VkApi
    {
        public BotCore Core { get; private set; }

        public long GroupId { get; private set; }

        private Dictionary<long, Chat> Chats { get; set; }
        public MessageHandler MessageHandler { get; private set; }

        public VkCoreApiBase(BotCore core, long groupId)
        {
            Core = core;
            GroupId = groupId;
            Chats = new Dictionary<long, Chat>();
            MessageHandler = new MessageHandler(this);
        }

        public Chat GetChat(long peerId)
        {
            if (!Chats.ContainsKey(peerId))
                Chats.Add(peerId, new Chat(this, peerId));
            return Chats[peerId];
        }
    }
}
