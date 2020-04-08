using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Utils;

namespace VkBotCore.Callback
{
    [ApiController]
    [Route("api/[controller]")]
    public class CallbackController : ControllerBase
    {
        public BotCore Core { get; set; }

        private readonly IConfiguration _configuration;

        private readonly IVkApi _vkApi;

        private readonly string _secretKey;

        public CallbackController(BotCore core)
        {
            try
            {
                Core = core;
                _configuration = Core.Configuration;
                _vkApi = Core.VkApi;
                _secretKey = _configuration.GetValue<string>("Config:SecretKey", null);
            }
            catch (Exception e)
            {
                Core.Log.Error(e.ToString());
            }
        }

        public IActionResult Callback([FromBody]Updates updates)
        {
            try
            {
                if (updates.SecretKey != _secretKey)
                    return BadRequest("Secret key is incorrect!");

                if(updates.Type == CallbackReceive.Confirmation)
                    return Ok(_configuration.GetValue($"Config:Groups:{updates.GroupId}:Confirmation", _configuration["Config:Confirmation"]));

                new Thread(() =>
                {
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
                                Chat chat = vkApi.GetChat(msg.PeerId.Value);
                                lock (chat)
                                {
                                    User user = null;
                                    try { user = new User(vkApi, msg.FromId.Value); } catch { return; }

                                    vkApi.MessageHandler.OnMessage(user, msg.Text, chat, msg);
                                }
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Core.Log.Error(e.ToString());
                    }
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