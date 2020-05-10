using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Linq;
using ApiAuthParams = VkNet.Model.ApiAuthParams;

namespace VkBotCore
{
	public class VkCoreApi : VkCoreApiBase
	{
		internal ConcurrentDictionary<long, VkCoreApiBase> _vkApi { get; private set; }

		public VkCoreApi(BotCore core) : base(core, 0)
		{
			_vkApi = new ConcurrentDictionary<long, VkCoreApiBase>();

			var accesToken = Core.Configuration.GetValue<string>($"Config:AccessToken", null);
			if (accesToken != null)
				Authorize(new ApiAuthParams { AccessToken = accesToken });

			LoadAll();
		}

		public void LoadAll()
		{
			foreach (var a in Core.Configuration.GetSection("Config:Groups").GetChildren())
				Get(long.Parse(a.Key));
		}

		public VkCoreApiBase Get(long groupId)
		{
			if (groupId == GroupId) return this;
			return _vkApi.GetOrAdd(groupId, _groupId =>
			{
				var accesToken = Core.Configuration.GetValue<string>($"Config:Groups:{groupId}:AccessToken", null);
				if (accesToken == null)
					return this;
				var api = new VkCoreApiBase(Core, _groupId);
				api.Authorize(new ApiAuthParams { AccessToken = accesToken });
				return api;
			});
		}

		public VkCoreApiBase[] GetAvailableApis(string _namespace)
		{
			var apis = _vkApi.Values.Where(a => a.AvailableNamespaces.Contains(_namespace));
			if (AvailableNamespaces.Contains(_namespace))
			{
				var _apis = apis.ToList();
				_apis.Add(this);
				return _apis.ToArray();
			}
			return apis.ToArray();
		}
	}
}
