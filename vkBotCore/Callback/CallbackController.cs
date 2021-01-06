using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Threading;
using VkBotCore.Subjects;
using VkBotCore.Utils;
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

		private long _messageResendBlockTime = 20;
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

						var update = GroupUpdate.FromJson(response);

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
							case CallbackReceive.Group.OfficersEdit:
							{
								var msgEvent = GroupOfficersEdit.FromJson(response);

								var userId = msgEvent.UserId.Value;

								vkApi.Group.Managers.Remove(userId);

								var level = (int) msgEvent.LevelNew.Value;

								if (level > 0)
								{
									var permissionLevel = (int)Math.Pow(10, level + 1);
									vkApi.Group.Managers.Add(userId, (UserPermission)level);
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