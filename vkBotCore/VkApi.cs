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
    public class VkCoreApi : VkApi
    {
        Dictionary<long, IVkApi> _vkApi { get; set; }
        public IConfiguration Configuration { get; private set; }

        public VkCoreApi(IConfiguration configuration)
        {
            _vkApi = new Dictionary<long, IVkApi>();

            var accesToken = Configuration.GetValue<string>($"Config:AccessToken", null);
            if (accesToken != null)
                Authorize(new ApiAuthParams { AccessToken = accesToken });
        }

        public IVkApi Get(long groupId)
        {
            if (_vkApi.ContainsKey(groupId))
                return _vkApi[groupId];
            var api = new VkApi();
            var accesToken = Configuration.GetValue<string>($"Config:AccessToken:{groupId}", null);
            if (accesToken == null)
                return this;
            api.Authorize(new ApiAuthParams { AccessToken = accesToken });
            _vkApi.Add(groupId, api);
            return api;
        }
    }
}
