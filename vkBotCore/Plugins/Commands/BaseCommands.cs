using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VkBotCore.Plugins.Attributes;
using VkBotCore.Subjects;
using VkBotCore.Utils;

namespace VkBotCore.Plugins.Commands
{
	public class BaseCommands
	{
		#region //HELP
		[Command(Name = "помощь", Aliases = new string[] { "help" }, Description = "список всех команд")]
		public static void Help(CommandContext context, [ParamName("command name")]string commandName = null)
		{
			var senderPermission = context.Core.PluginManager.GetUserPermission(context.Sender, context.Chat as Chat);

			if (commandName == null)
			{
				string helpInfo = "❗Все команды для взаимодействия с ботом должны начинаться с символов . или /\n" +
														  "❗Команды могут иметь подкоманды и аргументы.\n" +
														  "❗Команды, подкоманды и аргументы могут быть на русском и на английском языках: оба варианта представлены ниже\n\n" +
														  "📜Список команд с их описанием: \n";

				foreach (var command in context.Core.PluginManager.Commands.Values)
				{
					string line = "";

					var permittedOverloads = GetPermittedOverload(context, command, senderPermission);

					var overloadsCount = permittedOverloads.Count();
					if (overloadsCount == 0)
						continue;
					else if (overloadsCount == 1)
						line = $"/{command.Name}{GenerateOverloadDescription(permittedOverloads.First())}";
					else
					{
						var defaultOverloads = permittedOverloads.Where(overload => Attribute.GetCustomAttribute(overload.Method, typeof(DefaultCommand), false) as DefaultCommand != null);
						if (defaultOverloads == null)
							line = GenerateFullCommandList(context, command, senderPermission);
						else
							line = GenerateFullCommandList(command.Name, defaultOverloads);
					}

					helpInfo += $"\n{line}";
				}

				context.Chat.SendMessageAsync(helpInfo);
			}
			else
			{
				commandName = commandName.ToLower();
				Command command = context.Core.PluginManager.Commands.Values.FirstOrDefault(c => c.Name == commandName || c.Overloads.Values.Any(o => o.Aliases.Contains(commandName)));
				if (command == null)
				{
					context.Chat.SendMessageAsync($"Команда \"{commandName}\" не найдена!");
					return;
				}

				string commandList = GenerateFullCommandList(context, command, senderPermission);

				if (commandList == null)
				{
					context.Chat.SendMessageAsync($"Команда \"{commandName}\" не найдена!");
					return;
				}

				string helpInfo = $"/{command.Name}\n\n{commandList}";

				context.Chat.SendMessageAsync(helpInfo);
			}
		}

		private static IEnumerable<Overload> GetPermittedOverload(CommandContext context, Command command)
		{
			var availableOverloads = command.Overloads.Values.Where(overload => IsAvailable(overload, context));
			return context.Core.PluginManager.PermittedOverloads(availableOverloads, context.Sender, context.Chat as Chat);
		}

		private static IEnumerable<Overload> GetPermittedOverload(CommandContext context, Command command, short permission)
		{
			return command.Overloads.Values.Where(overload => IsAvailable(overload, context, permission));
		}

		private static bool IsAvailable(Overload overload, CommandContext context)
		{
			var command = overload.Method;
			var chat = context.Chat;
			var pluginManager = context.Core.PluginManager;

			return !overload.IsHidden
				&& pluginManager.IsAvailable(command, chat.VkApi.GroupId)
				&& pluginManager.CorrectlyUsage(command, chat);
		}

		private static bool IsAvailable(Overload overload, CommandContext context, short permission)
		{
			var command = overload.Method;
			var chat = context.Chat;
			var pluginManager = context.Core.PluginManager;

			return !overload.IsHidden
				&& pluginManager.IsAvailable(command, chat.VkApi.GroupId)
				&& pluginManager.CorrectlyUsage(command, chat)
				&& permission >= pluginManager.GetCommandPermission(chat as Chat, command);
		}

		private static string GenerateFullCommandList(CommandContext context, Command command, short permission)
		{
			var permittedOverloads = GetPermittedOverload(context, command, permission);
			if (permittedOverloads.Count() == 0)
				return null;
			return GenerateFullCommandList(command.Name, permittedOverloads);
		}

		private static string GenerateFullCommandList(string commandName, IEnumerable<Overload> overloads)
		{
			return $"/{commandName}{string.Join($"\n/{commandName}", overloads.Select(o => GenerateOverloadDescription(o)))}";
		}

		private static string GenerateOverloadDescription(Overload overload)
		{
			string description = overload.Aliases.Length == 0 || !overload.UseFullDescription ? "" : $"/{string.Join("/", overload.Aliases)}";
			foreach (var param in overload.Method.GetParameters())
				if (!typeof(CommandContext).IsAssignableFrom(param.ParameterType))
					description += " " + GetParamDescription(param, overload.UseFullDescription);

			if (!string.IsNullOrEmpty(overload.Description))
				description += " - " + overload.Description;
			return description;
		}

		private static string GetParamDescription(ParameterInfo parameter, bool useFullDescription)
		{
			string format = (parameter.IsOptional || Attribute.GetCustomAttribute(parameter, typeof(Optimal), false) as Optimal != null) ? (parameter.DefaultValue == null ? "[{0}]" : "[{0}={1}]") : "<{0}>";
			if (PluginManager.IsParams(parameter) || Attribute.GetCustomAttribute(parameter, typeof(HideDefault), false) as HideDefault != null)
				format = "[{0}]";
			string name = (Attribute.GetCustomAttribute(parameter, typeof(ParamName), false) as ParamName)?.Name;

			if (parameter.ParameterType.IsEnum)
			{
				List<string> _values = new List<string>();
				foreach (var val in Enum.GetValues(parameter.ParameterType))
					_values.Add(val.ToString());
				var values = useFullDescription ? string.Join("|", _values) : parameter.Name;
				if (Attribute.GetCustomAttribute(parameter, typeof(SubCommand), false) as SubCommand != null)
					return values;
				else
					return string.Format(format, name ?? values, parameter.DefaultValue);
			}

			return string.Format(format, name ?? parameter.Name, parameter.DefaultValue);
		}
		#endregion

		#region //PERMISSIONS
		[Command(Permission = 1, Usage = CommandUsage.Chat)]
		private static void PermsList(CommandContext context)
		{
			if (context.Chat is Chat chat)
			{
				bool baseCommandsAllowed = context.Core.Configuration.GetValue("Config:Plugins:BaseCommandsAllowed", false);
				if (baseCommandsAllowed)
				{
					var permsList = string.Join("; ", context.Core.PluginManager.Permissions.Select(p => p.Value.ToString() + " = " + p.Key));
					context.Chat.SendMessageAsync($"Зарегистрированные права: {permsList}");
				}
			}
		}

		[Command(Permission = (short) UserPermission.Admin, Usage = CommandUsage.Chat)]
		private static void SetPerms(CommandContext context, User user, short permission)
		{
			if (context.Chat is Chat chat)
			{
				bool baseCommandsAllowed = context.Core.Configuration.GetValue("Config:Plugins:BaseCommandsAllowed", false);
				if (baseCommandsAllowed)
				{
					var senderPermission = context.Core.PluginManager.GetUserPermission(context.Sender, chat);
					if (senderPermission < permission || senderPermission < chat.GetUserPermission(user)) return;

					chat.SetUserPermission(user, permission);
					string level = permission.ToString();
					if (context.Core.PluginManager.Permissions.TryGetValue(permission, out Enum value))
						level = value.ToString();
					context.Chat.SendMessageAsync($"Выданы права уровня: {level}");
				}
			}
		}

		[Command(Permission = (short) UserPermission.Admin, Usage = CommandUsage.Chat)]
		private static void SetPerms(CommandContext context, User user, string permission)
		{
			if (context.Sender == user) return;

			permission = permission.ToLower();
			var permissions = context.Core.PluginManager.Permissions;
			if (permissions.Any(p => p.Value.ToString().ToLower() == permission))
			{
				var _permission = permissions.First(p => p.Value.ToString().ToLower() == permission);
				SetPerms(context, user, _permission.Key);
			}
		}

		[Command(Permission = (short) UserPermission.Admin, Usage = CommandUsage.Chat)]
		private static void Perms(CommandContext context, User user)
		{
			if (context.Chat is Chat chat)
			{
				bool baseCommandsAllowed = context.Core.Configuration.GetValue("Config:Plugins:BaseCommandsAllowed", false);
				if (baseCommandsAllowed)
				{
					short permission = chat.GetUserPermission(user);
					string level = permission.ToString();
					if (context.Core.PluginManager.Permissions.TryGetValue(permission, out Enum value))
						level = value.ToString();
					context.Chat.SendMessageAsync($"Уровень прав: {level}");
				}
			}
		}

		[Command(Permission = 1, Usage = CommandUsage.Chat)]
		private static void Perms(CommandContext context)
		{
			Perms(context, context.Sender);
		}

		[Command(Permission = (short) UserPermission.Admin, Usage = CommandUsage.Chat)]
		private static async void SetCmdPerms(CommandContext context, string commandName, short permission)
		{
			try
			{
				if (context.Chat is Chat chat)
				{
					bool baseCommandsAllowed = context.Core.Configuration.GetValue("Config:Plugins:BaseCommandsAllowed", false);
					if (baseCommandsAllowed)
					{
						var pluginManager = context.Core.PluginManager;

						var senderPermission = pluginManager.GetUserPermission(context.Sender, chat);
						if (senderPermission < permission || senderPermission < pluginManager.GetCommandPermission(chat, context.Command))
							return;

						var commands = await GetCommands(context, commandName.ToLower());
						if (commands == null)
							return;
						foreach (var command in commands)
							pluginManager.SetCommandPermission(chat, command, permission);

						string level = permission.ToString();
						if (pluginManager.Permissions.TryGetValue(permission, out Enum value))
							level = value.ToString();
						context.Chat.SendMessageAsync($"Установлены права уровня: {level}");
					}
				}
			}
			catch (Exception e)
			{
				context.Core.Log.Error(context.Chat, e.ToString());
			}
		}

		[Command(Permission = (short) UserPermission.Admin, Usage = CommandUsage.Chat)]
		private static void SetCmdPerms(CommandContext context, string commandName, string permission)
		{
			permission = permission.ToLower();
			var permissions = context.Core.PluginManager.Permissions;
			if (permissions.Any(p => p.Value.ToString().ToLower() == permission))
			{
				var _permission = permissions.First(p => p.Value.ToString().ToLower() == permission);
				SetCmdPerms(context, commandName, _permission.Key);
			}
		}

		[Command(Permission = 1, Usage = CommandUsage.Chat)]
		private static void CmdPerms(CommandContext context, string commandName)
		{
			if (context.Chat is Chat chat)
			{
				bool baseCommandsAllowed = context.Core.Configuration.GetValue("Config:Plugins:BaseCommandsAllowed", false);
				if (baseCommandsAllowed)
				{
					commandName = commandName.ToLower();
					var commandsList = GeneratePermsCommandList(context, commandName, out MethodInfo[] commands, false);
					if (commands.Length == 0)
					{
						context.Chat.SendMessageAsync("Запрос отменён. Команда не найдена.");
						return;
					}
					context.Chat.SendMessageAsync($"Установлены права для команды /{commandName}.\n\n" + commandsList);
				}
			}
		}

		private static async Task<MethodInfo[]> GetCommands(CommandContext context, string name)
		{
			var commandsList = GeneratePermsCommandList(context, name, out MethodInfo[] commands, true);
			if (commands.Length == 0)
			{
				context.Chat.SendMessageAsync("Запрос отменён. Команда не найдена.");
				return null;
			}
			else if (commands.Length == 1)
			{
				return commands;
			}

			context.Chat.SendMessage("В течение 15ти секунд введите номер команды, для изменения прав или All, для изменения прав у всех команд.\n\n" + commandsList);

			var secondsTimeout = 15;
			var delay = 50;
			var timeout = secondsTimeout * 1000 / delay;

			var value = await context.Chat.WaitMessageAsync(15, e => e.Sender == context.Sender);
			var message = value.Message.ToLower();

			if (message == "all") return commands;

			try
			{
				return new MethodInfo[] { commands[int.Parse(message)] };
			}
			catch
			{
				context.Chat.SendMessageAsync("Запрос отменён. Введено неверное значение.");
			}
			return null;
		}

		private static string GeneratePermsCommandList(CommandContext context, string commandName, out MethodInfo[] commands, bool counting)
		{
			commands = new MethodInfo[0];
			Command command;
			if (!context.Core.PluginManager.Commands.TryGetValue(commandName, out command))
				return null;

			var overloads = GetPermittedOverload(context, command);
			commands = overloads.Select(overload => overload.Method).ToArray();

			string output = "";
			int i = 1;
			foreach (var overload in overloads)
			{
				if (counting) output += i++ + "  ";
				output += $"/{commandName}{GenerateOverloadDescription(overload)}  >>УРОВЕНЬ ПРАВ>>  {context.Core.PluginManager.GetCommandPermission(context.Chat as Chat, overload.Method)}\n";
			}
			return output;
		}
		#endregion


		[Command(Permission = (short) UserPermission.Admin, Usage = CommandUsage.Chat)]
		private static void Everyone(CommandContext context, params string[] message)
		{
			if (context.Chat is Chat chat)
			{
				bool baseCommandsAllowed = context.Core.Configuration.GetValue("Config:Plugins:BaseCommandsAllowed", false);
				if (context.Sender.IsAppAdmin || (baseCommandsAllowed && context.Sender.IsChatAdmin(chat)))
				{
					var mentions = chat.GetEveryoneMentions();
					int k = 100;
					for (var i = 0; i < mentions.Count(); i += k)
						context.Chat.SendMessageAsync($"{string.Join(" ", message)}{string.Join("", mentions.Skip(i).Take(k))}");
				}
			}
		}
	}
}
