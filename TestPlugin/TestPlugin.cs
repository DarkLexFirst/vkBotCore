using System;
using vkBotCore;
using vkBotCore.Plugins;
using vkBotCore.Plugins.Attributes;
using VkNet.Utils;
using Message = VkNet.Model.Message;

namespace TestPlugin
{
    public class TestPlugin : Plugin
    {
        [Command]
        private static void Everyone(CommandContext context, params string[] message)
        {
            if (context.Sender.IsAdmin || context.Sender.IsChatAdmin(context.Chat))
                context.Chat.SendMessage($"{context.Chat.GetEveryoneMentionLine("͟")}{string.Join(" ", message)}");
        }

        [Command(IsHidden = true)]
        private static void GetChatId(CommandContext context)
        {
            if (context.Sender.IsAdmin)
                context.Chat.SendMessage("Current chat id: " + context.Chat.PeerId);
        }

        public enum TestCommandCmd
        {
            параметр,
            parameter
        }

        public enum Colors
        {
            Red,
            Blue,
            White,
            Green,
            Black,
            Yellow,
            Orange,

            Transperent
        }

        [Command(IsHidden = true), DefaultCommand]
        private static void TestCommand(CommandContext context, [SubCommand]TestCommandCmd cmd, bool boolVal, int intVal = 30, [HideDefault]Colors color = Colors.Transperent)
        {
            if (context.Sender.IsAdmin)
                context.Chat.SendMessage($"{cmd}   {boolVal}   {intVal}   {color}");
        }

        public enum Test2CommandCmd { AAA }

        [Command(Name = "test", IsHidden = true)]
        private static void Test2Command(CommandContext context, [SubCommand]Test2CommandCmd cmd)
        {
            if (context.Sender.IsAdmin)
                context.Chat.SendMessage($"{cmd}");
        }

        [CallbackReceive("message_new")]
        public Updates CallbackMessageHandler(Updates updates, VkCoreApiBase vkApi)
        {
            var msg = Message.FromJson(new VkResponse(updates.Object));
            vkApi.Core.Log.Debug($"message from Group:{updates.GroupId} and Chat:{msg.ChatId} by {msg.FromId} Action:{msg.Action?.Type?.ToString() ?? "text"}  \"{msg.Text}\"");
            return updates;
        }

        [CallbackReceive("vkpay_transaction")]
        public Updates CallbackVkPayHandler(Updates updates, VkCoreApiBase vkApi)
        {
            var msg = new VkResponse(updates.Object);
            var user = new User(vkApi, msg["from_id"]);
            vkApi.Core.Log.Debug($"amount: {msg["amount"] / 1000}, from: {user.FirstName} {user.LastName}, description: {msg["description"]}");
            return updates;
        }
    }
}
