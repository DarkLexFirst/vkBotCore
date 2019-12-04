using System;
using vkBotCore;
using vkBotCore.Plugins;
using vkBotCore.Plugins.Attributes;
using VkNet.Model;
using VkNet.Utils;

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
        public void CallbackHandler(Updates updates, BotCore core)
        {
            var msg = Message.FromJson(new VkResponse(updates.Object));
            core.Log.Debug($"message from Group:{updates.GroupId} and Chat:{msg.ChatId} by {msg.FromId} Action:{msg.Action?.Type?.ToString() ?? "text"}  \"{msg.Text}\"");
        }
    }
}
