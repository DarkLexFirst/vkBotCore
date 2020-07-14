using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Threading;
using VkBotCore.Subjects;
using VkNet.Model.GroupUpdate;
using VkNet.Utils;
using Message = VkNet.Model.Message;

namespace VkBotCore.Callback
{
	[ApiController]
	[Route("api/[controller]")]
	public class CallbackController : ControllerBase
	{
		public BotCore Core { get; set; }

		private readonly string _secretKey;

		public CallbackController(BotCore core)
		{
			try
			{
				Core = core;
				_secretKey = Core.Configuration.GetValue<string>("Config:SecretKey", null);
			}
			catch (Exception e)
			{
				Core.Log.Error(e.ToString());
			}
		}

		private long _messageResendBlockTime = 10;
		public IActionResult Callback([FromBody]Updates updates)
		{
			try
			{
				if (updates.SecretKey != _secretKey)
					return BadRequest("Secret key is incorrect!");

				if (updates.Type == CallbackReceive.Confirmation)
					return Ok(Core.Configuration.GetValue($"Config:Groups:{updates.GroupId}:Confirmation", Core.Configuration["Config:Confirmation"]));

				new Thread(() =>
				{
					Stopwatch stopwatch = new Stopwatch();
					stopwatch.Start();
					try
					{
						VkCoreApiBase vkApi = Core.VkApi.Get(updates.GroupId);
						Core.PluginManager.PluginCallbackHandler(ref updates, vkApi);
						if (updates == null) return;

						var response = new VkResponse(updates.Object);

						switch (updates.Type)
						{
							case CallbackReceive.Message.New:
							{
								var msg = Message.FromJson(response);

								if (msg.Date.HasValue && (DateTime.UtcNow - msg.Date.Value).TotalSeconds > _messageResendBlockTime) return;

								IUser user = vkApi.GetUser(msg.FromId.Value);
								BaseChat chat = vkApi.GetChat(msg.PeerId.Value);

								lock (chat)
								{
									vkApi.MessageHandler.OnMessage(user, msg.Text, chat, msg);
								}
								break;
							}
							case CallbackReceive.Message.Event:
							{
								var msgEvent = MessageEvent.FromJson(response);

								IUser user = vkApi.GetUser(msgEvent.UserId.Value);

								if (user is User _user)
								{
									BaseChat chat = vkApi.GetChat(msgEvent.PeerId.Value);

									lock (chat)
									{
										vkApi.MessageHandler.ClickButton(chat, _user, new UI.EventId(msgEvent.EventId), msgEvent.Payload);
									}
								}
								break;
							}
						}
					}
					catch (Exception e)
					{
						Core.Log.Error(e.ToString());
					}
					Core.Log.Debug(stopwatch.ElapsedMilliseconds.ToString());
					stopwatch.Stop();
				})
				{ IsBackground = true }.Start();
			}
			catch (Exception e)
			{
				Core.Log.Error(e.ToString());
			}

			return Ok("ok");
		}
	}
}