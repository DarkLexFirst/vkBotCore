using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VkBotCore.Plugins.Attributes;

namespace VkBotCore.Plugins.Commands
{
	public class BaseCommands
	{
		[Command(Name = "помощь", Aliases = new string[] { "help" }, Description = "список всех команд")]
		public static void Help(CommandContext context, [ParamName("command name")]string commandName = null)
		{
			if (commandName == null)
			{
				string helpInfo = "❗Все команды для взаимодействия с ботом должны начинаться с символов . или /\n" +
														  "❗Команды могут иметь подкоманды и аргументы.\n" +
														  "❗Команды, подкоманды и аргументы могут быть на русском и на английском языках: оба варианта представлены ниже\n\n" +
														  "📜Список команд с их описанием: \n";

				foreach (var command in context.Core.PluginManager.Commands.Values)
				{
					string line = "";

					var overloadsCount = command.Overloads.Count(o => IsAvailabled(o.Value, context));
					if (overloadsCount == 0)
						continue;
					else if (overloadsCount == 1)
						line = $"/{command.Name}{GenerateOverloadDescription(command.Overloads.Values.First(o => IsAvailabled(o, context)))}";
					else
					{
						var defaultOverloads = command.Overloads.Values.Where(o => Attribute.GetCustomAttribute(o.Method, typeof(DefaultCommand), false) as DefaultCommand != null);
						if (defaultOverloads == null)
							line = GenerateFullCommandList(context, command);
						else
							line = GenerateFullCommandList(context, command.Name, defaultOverloads);
					}

					helpInfo += $"\n{line}";
				}

				context.Chat.SendMessageAsync(helpInfo);
			}
			else
			{
				commandName.ToLower();
				Command command = context.Core.PluginManager.Commands.Values.FirstOrDefault(c => c.Name == commandName || c.Overloads.Values.Any(o => o.Aliases.Contains(commandName)));
				if (command == null)
				{
					context.Chat.SendMessageAsync($"Команда \"{commandName}\" не найдена!");
					return;
				}

				string helpInfo = $"/{command.Name}\n\n";
				helpInfo += GenerateFullCommandList(context, command);

				context.Chat.SendMessageAsync(helpInfo);
			}
		}

		private static bool IsAvailabled(Overload overload, CommandContext context)
		{
			return !overload.IsHidden && context.Core.PluginManager.IsAvailable(overload.Method, context.Chat.VkApi.GroupId);
		}

		private static string GenerateFullCommandList(CommandContext context, Command command)
		{
			return GenerateFullCommandList(context, command.Name, command.Overloads.Values);
		}

		private static string GenerateFullCommandList(CommandContext context, string commandName, IEnumerable<Overload> overloads)
		{
			return $"/{commandName}{string.Join($"\n/{commandName}", overloads.Where(o => IsAvailabled(o, context)).Select(o => GenerateOverloadDescription(o)))}";
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
				var values = useFullDescription ? string.Join("|", _values) : _values.First();
				if (Attribute.GetCustomAttribute(parameter, typeof(SubCommand), false) as SubCommand != null)
					return values;
				else
					return string.Format(format, name ?? values, parameter.DefaultValue);
			}

			return string.Format(format, name ?? parameter.Name, parameter.DefaultValue);
		}

		[Command(IsHidden = true)]
		private static void Everyone(CommandContext context, params string[] message)
		{
			bool baseCommandsAllowed = context.Core.Configuration.GetValue("Config:Plugins:BaseCommandsAllowed", false);
			if (context.Sender.IsAdmin || (baseCommandsAllowed && context.Sender.IsChatAdmin(context.Chat)))
			{
				var mentions = context.Chat.GetEveryoneMentions();
				int k = 100;
				for (var i = 0; i < mentions.Count(); i += k)
					context.Chat.SendMessageAsync($"{string.Join(" ", message)}{string.Join("", mentions.Skip(i).Take(k))}");
			}
		}
	}
}
