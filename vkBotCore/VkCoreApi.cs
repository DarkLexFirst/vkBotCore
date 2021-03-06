﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using ApiAuthParams = VkNet.Model.ApiAuthParams;

namespace VkBotCore
{
	public class VkCoreApi : VkCoreApiBase
	{
		internal ConcurrentDictionary<long, VkCoreApiBase> _vkApi { get; private set; }

		public VkCoreApi(BotCore core) : base(core)
		{
			_vkApi = new ConcurrentDictionary<long, VkCoreApiBase>();

			var accesToken = Core.Configuration.GetValue<string>($"Config:AccessToken", null);
			if (accesToken != null)
			{
				GroupId = Groups.GetById(null, null, null).First().Id;
				Authorize(new ApiAuthParams { AccessToken = accesToken });
			}
		}

		internal override void Initialize()
		{
			base.Initialize();
			LoadAll();
		}

		public void LoadAll()
		{
			foreach (var a in Core.Configuration.Base.GetSection("Config:Groups").GetChildren())
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
				api.Initialize();
				return api;
			});
		}

		public VkCoreApiBase Get(string accessToken)
		{
			if (Token == accessToken)
				return this;

			var api = _vkApi.Values.FirstOrDefault(_api => _api.Token == accessToken);

			if(api == null)
			{
				api = new VkCoreApiBase(Core);
				api.Authorize(new ApiAuthParams { AccessToken = accessToken });
				api.GroupId = api.Groups.GetById(null, null, null).First().Id;
				api.Initialize();
			}

			return api;
		}

		public VkCoreApiBase[] GetAvailableApis(string _namespace)
		{
			var apis = _vkApi.Values.Where(a => a.AvailableNamespaces.Contains(_namespace) || a.AvailableNamespaces.Length == 0);
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
