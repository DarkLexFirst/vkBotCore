using System;
using VkBotCore;
using VkBotCore.Callback;
using VkBotCore.Plugins;
using VkBotCore.Plugins.Attributes;
using VkBotCore.Subjects;
using VkBotCore.UI;
using VkBotCore.VKPay;
using VkNet.Utils;
using Message = VkNet.Model.Message;

namespace TestPlugin
{
    public class TestPlugin : Plugin
    {
        protected override void OnEnable()
        {
            //при инициализации беседы происходит подгрузка всех клавиатур
            foreach (var api in AvailableApis)
                api.ChatCreated += (s, e) => LoadKeyboards(e.Chat);
        }

        public void LoadKeyboards(BaseChat chat)
        {
            //клавиатура с наивысшим приоритетом, коорая обновляется при каждой отправке сообщения ботом
            var keyboard = chat.BaseKeyboard = new Keyboard(null) { Id = "main_keyboard" };
            keyboard.Add(new KeyboardTextButton("Menu", (c, u, b) => c.SendKeyboardAsync("menu")));

            //вспомогательное меню с привязкой по Id
            var childKeyboard = new Keyboard("Menu opened") { Id = "menu", InMessage = true };
            childKeyboard.Add(new KeyboardTextButton("Test1", (c, u, b) => c.SendMessageAsync("test1!")) { Color = ButtonColor.Red });
            childKeyboard.Add(new KeyboardTextButton("Test2", (c, u, b) => c.SendMessageAsync("test2!")) { Color = ButtonColor.Red });
            chat.AddKeyboard(childKeyboard);
            
            //обработчик стандартной кнопки Начать
            chat.AddKeyboard(new StartKeyboard((c, u, b) => c.SendMessageAsync("start")));
        }

        [Command(IsHidden = true)]
        private static void GetChatId(CommandContext context)
        {
            if (context.Sender.IsAppAdmin)
                context.Chat.SendMessageAsync("Current chat id: " + context.Chat.PeerId);
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
            if (context.Sender.IsAppAdmin)
                context.Chat.SendMessageAsync($"{cmd}   {boolVal}   {intVal}   {color}");
        }

        public enum Test2CommandCmd { AAA }

        [Command(Name = "test", IsHidden = true)]
        private static void Test2Command(CommandContext context, [SubCommand]Test2CommandCmd cmd)
        {
            if (context.Sender.IsAppAdmin)
                context.Chat.SendMessageAsync($"{cmd}");
        }

        //обработчик входящих сообщений
        [CallbackReceive(CallbackReceive.Message.New)]
        public Updates CallbackMessageHandler(Updates updates, VkCoreApiBase vkApi)
        {
            var msg = Message.FromJson(new VkResponse(updates.Object));
            vkApi.Core.Log.Debug($"message from Group:{updates.GroupId} and Chat:{msg.ChatId} by {msg.FromId} Action:{msg.Action?.Type?.ToString() ?? "text"}  \"{msg.Text}\"");
            return updates;
        }

        //обработчик платежей
        [CallbackReceive(CallbackReceive.VkPay.Transaction)]
        public Updates CallbackVkPayHandler(Updates updates, VkCoreApiBase vkApi)
        {
            var msg = new VkResponse(updates.Object);
            var user = vkApi.GetUser<User>(msg["from_id"]);
            vkApi.Core.Log.Debug($"amount: {msg["amount"] / 1000}, from: {user.FirstName} {user.LastName}, description: {msg["description"]}");
            return updates;
        }

		//клавиатуры
        [Command(IsHidden = true)]
        private static void UITest1(CommandContext context)
        {
            if (context.Sender.IsAppAdmin)
            {
                var k = new Keyboard("test") { OneTime = true };
                k.Add(new KeyboardTextButton("test button 1", (c, u, b) => c.SendMessageAsync("Used by " + u.GetMentionLine())) { Color = ButtonColor.Red });
                k.AddOnNewLine(new KeyboardTextButton("test button 2", (c, u, p) => c.SendMessageAsync("Used by " + u.GetMentionLine())) { Color = ButtonColor.Green });
                k.Add(new KeyboardTextButton("test button 3", (c, u, b) => c.SendMessageAsync("Used by " + u.GetMentionLine())) { Color = ButtonColor.Blue });
                context.Chat.SendKeyboardAsync(k);
            }
        }

        [Command(IsHidden = true)]
        private static void UITest2(CommandContext context)
        {
            if (context.Sender.IsAppAdmin)
            {
                var k = new Keyboard("test");
                k.Add(new KeyboardTextButton("test button 1", (c, u, b) => c.SendMessageAsync("Used by " + u.GetMentionLine())) { Color = ButtonColor.Green });
                context.Chat.SendKeyboardAsync(k);
            }
        }

        [Command(IsHidden = true)]
        private static void UITest3(CommandContext context)
        {
            if (context.Sender.IsAppAdmin)
            {
                var k = new Keyboard("test") { InMessage = true };
                k.Add(new KeyboardTextButton("test button 1", "test_named_button") { Color = ButtonColor.Red });
                k.Add(new KeyboardLinkButton("Click me", "https://github.com/DarkLexFirst/vkBotCore"));
                context.Chat.SendKeyboardAsync(k);
            }
        }

        [Command(IsHidden = true)]
        private static void VkPayTest(CommandContext context)
        {
            if (context.Sender.IsAppAdmin)
            {
                var k = new Keyboard("vk pay ptg test") { InMessage = true };
                k.Add(new KeyboardVkPayButton(new VkPay(VkPayAction.Pay, VkPayTarget.Group, context.Chat.VkApi.GroupId, 10)));
                context.Chat.SendKeyboardAsync(k);
                k = new Keyboard("vk pay gt test") { InMessage = true };
                k.Add(new KeyboardVkPayButton(new VkPay(VkPayAction.Transfer, VkPayTarget.Group, context.Chat.VkApi.GroupId)));
                context.Chat.SendKeyboardAsync(k);
                k = new Keyboard("vk pay ptu test") { InMessage = true };
                k.Add(new KeyboardVkPayButton(new VkPay(VkPayAction.Pay, VkPayTarget.User, context.Sender.Id, 10)));
                context.Chat.SendKeyboardAsync(k);
                k = new Keyboard("vk pay ut test") { InMessage = true };
                k.Add(new KeyboardVkPayButton(new VkPay(VkPayAction.Transfer, VkPayTarget.User, context.Sender.Id)));
                context.Chat.SendKeyboardAsync(k);
            }
        }

		//удалённое хранилище игрока
        [Command(IsHidden = true)]
        private static void StorageSet(CommandContext context, string dataTag, string data, bool forced = false)
        {
            if (context.Sender.IsAppAdmin)
            {
                if (forced)
                    context.Sender.Storage.ForcedSet(dataTag, data);
                else
                    context.Sender.Storage[dataTag] = data;
            }
        }

        [Command(IsHidden = true)]
        private static void StorageGet(CommandContext context, string dataTag)
        {
            if (context.Sender.IsAppAdmin)
            {
                context.Chat.SendMessageAsync(context.Sender.Storage[dataTag]);
            }
        }

        [Command(IsHidden = true)]
        private static void StorageRemove(CommandContext context, string dataTag)
        {
            if (context.Sender.IsAppAdmin)
            {
                context.Sender.Storage[dataTag] = null;
            }
        }

        [Command(IsHidden = true)]
        private static void GetAllStorageKeys(CommandContext context)
        {
            if (context.Sender.IsAppAdmin)
            {
                context.Chat.SendMessageAsync(string.Join(", ", context.Sender.Storage.Keys));
            }
		}

		[Command(IsHidden = true)]
		private static void poolTest(CommandContext context, params string[] message)
		{
			if (context.Sender.IsAppAdmin)
			{
				for (var i = 0; i < 10; i++)
					context.Chat.SendMessageWithPool(i);
			}
		}

		//хранилище чата
		[Command(IsHidden = true)]
		private static void SetСStorage(CommandContext context, string key, string val = null)
		{
			if (context.Chat is Chat chat)
			{
				if (context.Sender.IsAppAdmin)
				{
					chat.Storage.Variables[key] = val;
				}
			}
		}

		[Command(IsHidden = true)]
		private static void GetСStorage(CommandContext context, string key)
		{
			if (context.Chat is Chat chat)
			{
				if (context.Sender.IsAppAdmin)
				{
					chat.SendMessage($"{key} = {chat.Storage.Variables[key]}");
				}
			}
		}

		[Command(IsHidden = true)]
		private static void SetUСStorage(CommandContext context, IUser user, string key, string val = null)
		{
			if (context.Chat is Chat chat)
			{
				if (context.Sender.IsAppAdmin)
				{
					chat.Storage.UsersStorage[user][key] = val;
				}
			}
		}

		[Command(IsHidden = true)]
		private static void GetUСStorage(CommandContext context, IUser user, string key)
		{
			if (context.Chat is Chat chat)
			{
				if (context.Sender.IsAppAdmin)
				{
					chat.SendMessage($"{key} = {chat.Storage.UsersStorage[user][key]}");
				}
			}
		}

		[Command(IsHidden = true)]
		private static void SaveСStorage(CommandContext context)
		{
			if (context.Chat is Chat chat)
			{
				if (context.Sender.IsAppAdmin)
				{
					chat.Storage.Save(true);
				}
			}
		}

		[Command(IsHidden = true)]
		private static async void testm(CommandContext context)
		{
			context.Chat.SendMessageAsync("write any message...");
			var message = await context.Chat.WaitMessageAsync();
			if (message == null)
			{
				context.Chat.SendMessageAsync("no message");
				return;
			}
			context.Chat.SendMessageAsync($"message: {message.Message}");
		}
	}
}
