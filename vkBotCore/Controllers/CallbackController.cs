using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using VkNet;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace vkBotCore.Controllers
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

        [HttpPost]
        public IActionResult Callback([FromBody] Updates updates)
        {
            return Callback(null, updates);
        }

        [HttpPost("{url}")]
        public IActionResult Callback(string url, [FromBody] Updates updates)
        {
            try
            {
                if (updates.SecretKey != _secretKey)
                    return BadRequest("Secret key is incorrect!");
                if(!Core.PluginManager.Plugins.Any(p => p.GetType().Name == url))
                    return BadRequest("Incorrent url! Possible option: /api/callback or /api/callback/{plugin name}");

                Core.PluginManager.PluginCallbackHandler(updates, url);

                switch (updates.Type)
                {
                    case "confirmation":
                        return Ok(_configuration["Config:Confirmation"]);
                    case "message_new":
                        {


                            var msg = Message.FromJson(new VkResponse(updates.Object));
                            var user = new User(Core, msg.FromId.Value);

                            Core.MessageHandler.OnMessage(user, msg.Text, msg.PeerId.Value, msg, url);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Core.Log.Error(e.ToString());
            }

            return Ok("ok");
        }
    }
}