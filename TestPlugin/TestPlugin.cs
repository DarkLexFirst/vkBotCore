using System;
using vkBotCore;
using vkBotCore.Plugins;
using vkBotCore.Plugins.Attributes;
using vkBotCore.UI;
using vkBotCore.VKPay;
using VkNet.Utils;
using Message = VkNet.Model.Message;

namespace TestPlugin
{
    public class TestPlugin : Plugin
    {
        protected override void OnEnable()
        {
            //при инициализации беседы происходит подгрузка всех панелей
            foreach (var api in AvailableApis)
                api.ChatCreated += (s, e) => LoadKeyboards(e.Chat);
        }

        public void LoadKeyboards(Chat chat)
        {
            //панель с наивысшим приоритетом, коорая обновляется при каждой отправке сообщения ботом
            var keyboard = chat.BaseKeyboard = new Keyboard(null) { Id = "main_keyboard" };
            keyboard.Add(new KeyboardTextButton("Menu", (c, user) => c.SendKeyboard("menu")));

            //вспомогательное меню с привязкой по Id
            var childKeyboard = new Keyboard("Menu opened") { Id = "menu", InMessage = true };
            childKeyboard.Add(new KeyboardTextButton("Test1", (c, u) => c.SendMessage("test1!")) { Color = ButtonColor.Red });
            childKeyboard.Add(new KeyboardTextButton("Test2", (c, u) => c.SendMessage("test2!")) { Color = ButtonColor.Red });
            chat.AddKeyboard(childKeyboard);
            
            //обработчик стандартной кнопки Старт
            var startButton = new Keyboard("Start") { Id = "start" };
            childKeyboard.Add(new KeyboardTextButton("Старт", (c, u) => c.SendMessage("start")));
            chat.AddKeyboard(childKeyboard);
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

        [Command(IsHidden = true)]
        private static void UITest1(CommandContext context)
        {
            if (context.Sender.IsAdmin)
            {
                var k = new Keyboard("test") { OneTime = true };
                k.Add(new KeyboardTextButton("test button 1", (c, u) => c.SendMessage("Used by " + u.GetMentionLine())) { Color = ButtonColor.Red });
                k.AddOnNewLine(new KeyboardTextButton("test button 2", (c, u) => c.SendMessage("Used by " + u.GetMentionLine())) { Color = ButtonColor.Green });
                k.Add(new KeyboardTextButton("test button 3", (c, u) => c.SendMessage("Used by " + u.GetMentionLine())) { Color = ButtonColor.Blue });
                context.Chat.SendKeyboard(k);
            }
        }

        [Command(IsHidden = true)]
        private static void UITest2(CommandContext context)
        {
            if (context.Sender.IsAdmin)
            {
                var k = new Keyboard("test");
                k.Add(new KeyboardTextButton("test button 1", (c, u) => c.SendMessage("Used by " + u.GetMentionLine())) { Color = ButtonColor.Green });
                context.Chat.SendKeyboard(k);
            }
        }

        [Command(IsHidden = true)]
        private static void UITest3(CommandContext context)
        {
            if (context.Sender.IsAdmin)
            {
                var k = new Keyboard("test") { InMessage = true };
                k.Add(new KeyboardTextButton("test button 1", "test_named_button") { Color = ButtonColor.Red });
                k.Add(new KeyboardLinkButton("Click me", "https://github.com/DarkLexFirst/vkBotCore"));
                context.Chat.SendKeyboard(k);
            }
        }

        [Command(IsHidden = true)]
        private static void VkPayTest(CommandContext context)
        {
            if (context.Sender.IsAdmin)
            {
                var k = new Keyboard("vk pay ptg test") { InMessage = true };
                k.Add(new KeyboardVkPayButton(new VkPay(VkPayAction.Pay, VkPayTarget.Group, context.Chat.VkApi.GroupId, 10)));
                context.Chat.SendKeyboard(k);
                k = new Keyboard("vk pay gt test") { InMessage = true };
                k.Add(new KeyboardVkPayButton(new VkPay(VkPayAction.Transfer, VkPayTarget.Group, context.Chat.VkApi.GroupId)));
                context.Chat.SendKeyboard(k);
                k = new Keyboard("vk pay ptu test") { InMessage = true };
                k.Add(new KeyboardVkPayButton(new VkPay(VkPayAction.Pay, VkPayTarget.User, context.Sender.Id, 10)));
                context.Chat.SendKeyboard(k);
                k = new Keyboard("vk pay ut test") { InMessage = true };
                k.Add(new KeyboardVkPayButton(new VkPay(VkPayAction.Transfer, VkPayTarget.User, context.Sender.Id)));
                context.Chat.SendKeyboard(k);
            }
        }
    }
}
