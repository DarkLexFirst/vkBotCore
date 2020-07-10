using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using VkBotCore.Subjects;
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

		public IActionResult Callback()
		{
			if (Request.BodyReader.TryRead(out ReadResult result))
			{
				byte[] bytes = result.Buffer.ToArray();

				var body = Encoding.UTF8.GetString(bytes);

				var updates = JsonConvert.DeserializeObject<Updates>(body);

				return OnUpdate(updates);
			}

			return Ok("Body is empty");
		}

		private long _messageResendBlockTime = 10;
		private IActionResult OnUpdate(Updates updates)
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

						switch (updates.Type)
						{
							case CallbackReceive.Message.New:
							{
								var msg = Message.FromJson(new VkResponse(updates.Object));
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
								//TODO
								var userId = updates.Object["user_id"].ToObject<long>();
								var peerId = updates.Object["peer_id"].ToObject<long>();
								var eventId = updates.Object["event_id"].ToObject<string>();
								var payload = updates.Object["payload"].ToString();

								IUser user = vkApi.GetUser(userId);

								if (user is User _user)
								{
									BaseChat chat = vkApi.GetChat(peerId);

									lock (chat)
									{
										vkApi.MessageHandler.ClickButton(chat, _user, eventId, payload);
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