using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using VkBotCore.Plugins.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Configuration;
using VkBotCore.Plugins.Commands;
using VkBotCore.Callback;
using VkBotCore.Configuration;
using VkBotCore.Subjects;
using Message = VkNet.Model.Message;

namespace VkBotCore.Plugins
{
	public class PluginManager
	{
		public BotCore Core { get; set; }

		private readonly List<object> _plugins = new List<object>();
		private readonly Dictionary<MethodInfo, CommandAttribute> _pluginCommands = new Dictionary<MethodInfo, CommandAttribute>();
		private readonly Dictionary<MethodInfo, string> _callbackHandlers = new Dictionary<MethodInfo, string>();

		public List<object> Plugins
		{
			get { return _plugins; }
		}

		public CommandSet Commands { get; set; } = new CommandSet();

		private string _currentPath = null;

		public PluginManager(BotCore core)
		{
			Core = core;
			LoadCommands(new BaseCommands());
		}

		internal void LoadPlugins()
		{
			if (bool.Parse(Core.Configuration["Config:Plugins:PluginDisabled"])) return;

			// Default it is the directory we are executing, and below.
			string pluginDirectoryPaths = Path.GetDirectoryName(new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath);
			pluginDirectoryPaths = Core.Configuration["Config:Plugins:PluginDirectory"];
			//HACK: Make it possible to define multiple PATH;PATH;PATH

			foreach (string dirPath in pluginDirectoryPaths.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (dirPath == null) continue;

				string pluginDirectory = Path.GetFullPath(dirPath);

				//Log.Debug($"Looking for plugin assemblies in directory {pluginDirectory}");

				if (!Directory.Exists(pluginDirectory)) continue;

				_currentPath = pluginDirectory;

				AppDomain currentDomain = AppDomain.CurrentDomain;
				currentDomain.AssemblyResolve += MyResolveEventHandler;

				List<string> pluginPaths = new List<string>();

				pluginPaths.AddRange(Directory.GetFiles(pluginDirectory, "*.dll", SearchOption.AllDirectories));
				pluginPaths.AddRange(Directory.GetFiles(pluginDirectory, "*.exe", SearchOption.AllDirectories));
				//pluginPaths.ForEach(path => Log.Debug($"Looking for plugins in assembly {path}"));

				foreach (string pluginPath in pluginPaths)
				{
					try
					{
						Assembly newAssembly = Assembly.LoadFile(pluginPath);

						try
						{
							Type[] types = newAssembly.GetExportedTypes();
							foreach (Type type in types)
							{
								try
								{
									// If no PluginAttribute and does not implement IPlugin interface, not a valid plugin
									if (!type.IsDefined(typeof(PluginAttribute), true) && !typeof(IPlugin).IsAssignableFrom(type)) continue;

									// If plugin is already loaded don't load it again
									if (_plugins.Any(l => l.GetType().AssemblyQualifiedName == type.AssemblyQualifiedName))
									{
										//Log.Error($"Tried to load duplicate plugin: {type}");
										continue;
									}

									if (type.IsDefined(typeof(PluginAttribute), true))
									{
										PluginAttribute pluginAttribute = Attribute.GetCustomAttribute(type, typeof(PluginAttribute), true) as PluginAttribute;
										if (pluginAttribute != null)
										{
											if (!bool.Parse(Core.Configuration[$"Plugins:{pluginAttribute.PluginName}:Enabled"] ?? "true")) continue;

										}
									}
									var ctor = type.GetConstructor(Type.EmptyTypes);
									if (ctor != null)
									{
										var plugin = ctor.Invoke(null);
										LoadPlugin(plugin, type);
									}
								}
								catch (Exception ex)
								{
									//Log.WarnFormat("Failed loading plugin type {0} as a plugin.", type);
									Core.Log.Warn($"Plugin \"{type.Name}\" loader caught exception, but is moving on.\n{ex}");
								}
							}
						}
						catch (Exception e)
						{
							//Log.WarnFormat("Failed loading exported types for assembly {0} as a plugin.", newAssembly.FullName);
							//Log.Debug("Plugin loader caught exception, but is moving on.", e);
							Core.Log.Warn($"Plugin loader caught exception, but is moving on.\n{e}");
						}
					}
					catch (Exception e)
					{
						//Log.Warn($"Failed loading assembly at path \"{pluginPath}\"");
						//Log.Debug("Plugin loader caught exception, but is moving on.", e);
						Core.Log.Warn($"Plugin loader caught exception, but is moving on.\n{e}");
					}
				}
			}
		}

		public void LoadPlugin(object plugin)
		{
			Type type = plugin.GetType();

			if (_plugins.Any(l => l.GetType().AssemblyQualifiedName == type.AssemblyQualifiedName))
			{
				Core.Log.Error($"Tried to load duplicate plugin: {type}");
				return;
			}

			if (type.IsDefined(typeof(PluginAttribute), true))
			{
				PluginAttribute pluginAttribute = Attribute.GetCustomAttribute(type, typeof(PluginAttribute), true) as PluginAttribute;
				if (pluginAttribute != null)
				{
					if (!bool.Parse(Core.Configuration[$"Plugins:{pluginAttribute.PluginName}:Enabled"] ?? "true"))
						return;
				}
			}

			LoadPlugin(plugin, type);
		}

		private void LoadPlugin(object plugin, Type type)
		{
			_plugins.Add(plugin);
			LoadCommands(type);
			LoadCallbackHandlers(type);
			Commands = GenerateCommandSet(_pluginCommands.Keys.ToArray());
			Core.Log.Debug($"Loaded plugin {type}");
		}

		public event ResolveEventHandler AssemblyResolve;

		private Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
		{
			var assembly = AssemblyResolve?.Invoke(sender, args);

			if (assembly != null) return assembly;
			if (_currentPath == null) return null;

			try
			{
				AssemblyName name = new AssemblyName(args.Name);
				string assemblyPath = _currentPath + "\\" + name.Name + ".dll";
				return Assembly.LoadFile(assemblyPath);
			}
			catch (Exception)
			{
				try
				{
					AssemblyName name = new AssemblyName(args.Name);
					string assemblyPath = _currentPath + "\\" + name.Name + ".exe";
					return Assembly.LoadFile(assemblyPath);
				}
				catch (Exception)
				{
					return Assembly.LoadFile(args.Name + ".dll");
				}
			}
		}

		public void LoadCommands(object instance)
		{
			if (!_plugins.Contains(instance)) _plugins.Add(instance);
			LoadCommands(instance.GetType());
			Commands = GenerateCommandSet(_pluginCommands.Keys.ToArray());
		}

		public void LoadCommands(Type type)
		{
			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
			foreach (MethodInfo method in methods)
			{
				CommandAttribute commandAttribute = Attribute.GetCustomAttribute(method, typeof(CommandAttribute), false) as CommandAttribute;
				if (commandAttribute == null) continue;

				if (string.IsNullOrEmpty(commandAttribute.Name))
				{
					commandAttribute.Name = method.Name;
				}

				DescriptionAttribute descriptionAttribute = Attribute.GetCustomAttribute(method, typeof(DescriptionAttribute), false) as DescriptionAttribute;
				if (descriptionAttribute != null) commandAttribute.Description = descriptionAttribute.Description;

				try
				{
					_pluginCommands.Add(method, commandAttribute);
				}
				catch (ArgumentException e)
				{
					Core.Log.Debug($"Command already exist {method.Name}, {method}\n{e}");
				}
			}
		}

		public static CommandSet GenerateCommandSet(MethodInfo[] methods)
		{
			CommandSet commands = new CommandSet();

			foreach (MethodInfo method in methods)
			{
				CommandAttribute commandAttribute = Attribute.GetCustomAttribute(method, typeof(CommandAttribute), false) as CommandAttribute;
				if (commandAttribute == null) continue;

				if (string.IsNullOrEmpty(commandAttribute.Name))
				{
					commandAttribute.Name = method.Name;
				}

				var overload = new Overload
				{
					Description = commandAttribute.Description ?? "",
					Aliases = commandAttribute.Aliases ?? new string[0],
					IsHidden = commandAttribute.IsHidden,
					UseFullDescription = commandAttribute.UseFullDescription,
					Method = method,
				};

				string commandName = commandAttribute.Name.ToLowerInvariant();
				string uuid = commandAttribute.Overload ?? Guid.NewGuid().ToString();

				if (commands.ContainsKey(commandName))
				{
					Command command = commands[commandName];
					command.Overloads.Add(uuid, overload);
				}
				else
				{
					commands.Add(commandName, new Command
					{
						Name = commandName,
						Overloads = new Dictionary<string, Overload> { { uuid, overload } }
					});
				}
			}

			return commands;
		}

		public void UnloadCommands(object instance)
		{
			if (!_plugins.Contains(instance)) return;
			_plugins.Remove(instance);

			var methods = _pluginCommands.Keys.Where(info => info.DeclaringType == instance.GetType()).ToArray();
			foreach (var method in methods)
			{
				_pluginCommands.Remove(method);
			}

			Commands = GenerateCommandSet(_pluginCommands.Keys.ToArray());
		}

		public void LoadCallbackHandlers(object instance)
		{
			if (!_plugins.Contains(instance)) _plugins.Add(instance);
			LoadCallbackHandlers(instance.GetType());
		}

		private void LoadCallbackHandlers(Type type)
		{
			var methods = type.GetMethods();
			foreach (MethodInfo method in methods)
			{
				CallbackReceive packetHandlerAttribute = Attribute.GetCustomAttribute(method, typeof(CallbackReceive), false) as CallbackReceive;
				if (packetHandlerAttribute != null)
				{
					_callbackHandlers.Add(method, packetHandlerAttribute.Type.ToLower());
				}
			}
		}

		internal void EnablePlugins()
		{
			foreach (object plugin in _plugins.ToArray())
			{
				IPlugin enablingPlugin = plugin as IPlugin;
				if (enablingPlugin == null) continue;

				try
				{
					enablingPlugin.OnEnable(this);
				}
				catch (Exception ex)
				{
					Core.Log.Warn($"On enable plugin  {ex}");
				}
			}
		}

		internal void DisablePlugins()
		{
			foreach (object plugin in _plugins)
			{
				IPlugin enablingPlugin = plugin as IPlugin;
				if (enablingPlugin == null) continue;

				try
				{
					enablingPlugin.OnDisable();
				}
				catch (Exception ex)
				{
					Core.Log.Warn($"On disable plugin  {ex}");
				}
			}
		}

		public object HandleCommand(User user, BaseChat chat, string cmdline, Message messageData)
		{
			var split = Regex.Split(cmdline, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
			string commandName = split[0].Trim('/').Trim('.').ToLower();
			string[] arguments = split.Skip(1).ToArray();

			Command command = null;
			command = GetCommand(commandName, chat.VkApi.GroupId);

			//if (arguments.Length > 0 && command == null)
			//{
			//	commandName = commandName + " " + arguments[0];
			//	arguments = arguments.Skip(1).ToArray();
			//	command = GetCommand(commandName);
			//}

			bool showErrorLine = Core.Configuration.GetValue("Config:Plugins:Commands:ShowErrorLine", true);

			if (command == null)
			{
				//Log.Warn($"Found no command {commandName}");
				if (showErrorLine)
					chat.SendMessageAsync($"[❗] Неизвестная команда \"/{commandName}\"! Полный список команд /help");
				return null;
			}

			foreach (var overload in command.Overloads.Values.OrderByDescending(o => o.Method.GetParameters().Length))
			{
				try
				{
					MethodInfo method = overload.Method;

					if (!IsAvailable(method, chat.VkApi.GroupId))
						continue;

					if (ExecuteCommand(method, user, chat, arguments, messageData, out object retVal))
					{
						return retVal;
					}

				}
				catch (Exception e)
				{
					Core.Log.Error(chat, e.ToString());
					continue;
				}
				Core.Log.Debug("No result from execution");
			}
			if (showErrorLine)
				chat.SendMessageAsync("[❗] Неверный синтаксис команды! /help чтобы посмотреть полный список команд");

			return null;
		}

		internal bool IsAvailable(MethodInfo method, long groupId)
		{
			if (groupId == 0) return true;
			var parrent = method.DeclaringType;
			string _namespace = GetNamespaceParrant(parrent);
			return _namespace == GetNamespaceParrant(GetType()) || Core.Configuration.GetArray($"Config:Groups:{groupId}:AvailableNamespaces", new string[] { _namespace }).Contains(_namespace);
		}

		public VkCoreApiBase[] GetAvailableApis(Type type)
		{
			return Core.VkApi.GetAvailableApis(GetNamespaceParrant(type));
		}

		private string GetNamespaceParrant(Type type)
		{
			return type.Namespace.Split('.').First();
		}

		private Command GetCommand(string commandName, long groupId)
		{
			Command command = null;
			try
			{
				if (Commands.ContainsKey(commandName))
				{
					command = Commands[commandName];
				}
				else
				{
					command = Commands.Values.FirstOrDefault(cmd => cmd.Overloads.Values.Any(overload => overload.Aliases?.Any(s => s == commandName) ?? false));
				}
				if (command == null)
					return null;
				if (!command.Overloads.Values.Any(o => IsAvailable(o.Method, groupId)))
					return null;
			}
			catch (Exception e) { Console.WriteLine(e); }
			return command;
		}

		public static bool HasProperty(dynamic obj, string name)
		{
			JObject tobj = obj;
			return tobj.Property(name) != null;
		}

		internal static bool IsParams(ParameterInfo param)
		{
			return Attribute.IsDefined(param, typeof(ParamArrayAttribute));
		}

		private bool ExecuteCommand(MethodInfo method, User user, BaseChat chat, string[] args, Message messageData, out object result)
		{
			Core.Log.Info($"Execute command {method}");

			result = new object();
			CommandContext context = new CommandContext(Core, user, chat, messageData);

			var parameters = method.GetParameters();

			int addLenght = 0;
			if (parameters.Length > 0 && typeof(CommandContext).IsAssignableFrom(parameters[0].ParameterType))
			{
				addLenght = 1;
			}

			object[] objectArgs = new object[parameters.Length];

			try
			{
				int i = 0;
				for (int k = 0; k < parameters.Length; k++)
				{
					var parameter = parameters[k];
					if (k == 0 && addLenght == 1)
					{
						if (typeof(CommandContext).IsAssignableFrom(parameter.ParameterType))
						{
							objectArgs[k] = context;
							continue;
						}
						Core.Log.Warn(chat, $"Command method {method.Name} missing Player as first argument.");
						return false;
					}

					bool isStringParam = IsParams(parameter) && parameter.ParameterType == typeof(string[]);

					if ((parameter.IsOptional || isStringParam) && args.Length <= i)
					{

						if (isStringParam)
							objectArgs[k] = new string[0];
						else
							objectArgs[k] = parameter.DefaultValue;
						continue;
					}

					if (args.Length < k && !isStringParam)
					{
						Core.Log.Debug(chat, $"No math {k} arguments");
						return false;
					}

					if (typeof(IParameterSerializer).IsAssignableFrom(parameter.ParameterType))
					{
						var ctor = parameter.ParameterType.GetConstructor(Type.EmptyTypes);
						IParameterSerializer defaultValue = ctor.Invoke(null) as IParameterSerializer;
						defaultValue?.Deserialize(user, args[i++]);

						objectArgs[k] = defaultValue;

						continue;
					}

					//mention groups unsupported.
					if (typeof(IUser).IsAssignableFrom(parameter.ParameterType))
					{
						long id;
						string _id = args[i++].Split(' ', '|').First();
						if (_id.Length < 4 || !long.TryParse(_id.Substring(3), out id)) return false;
						var _user = user.VkApi.GetUser(id);
						objectArgs[k] = parameter.ParameterType.IsAssignableFrom(_user.GetType()) ? _user : null;
						continue;
					}

					if (parameter.ParameterType == typeof(string))
					{
						objectArgs[k] = args[i++];
						continue;
					}
					if (parameter.ParameterType == typeof(byte))
					{
						byte value;
						if (!byte.TryParse(args[i++], out value)) return false;
						objectArgs[k] = value;
						continue;
					}
					if (parameter.ParameterType == typeof(short))
					{
						short value;
						if (!short.TryParse(args[i++], out value)) return false;
						objectArgs[k] = value;
						continue;
					}
					if (parameter.ParameterType == typeof(int))
					{
						int value;
						if (!int.TryParse(args[i++], out value)) return false;
						objectArgs[k] = value;
						continue;
					}
					if (parameter.ParameterType == typeof(long))
					{
						long value;
						if (!long.TryParse(args[i++], out value)) return false;
						objectArgs[k] = value;
						continue;
					}
					if (parameter.ParameterType == typeof(bool))
					{
						bool value;
						if (!bool.TryParse(args[i++], out value)) return false;
						objectArgs[k] = value;
						continue;
					}
					if (parameter.ParameterType == typeof(float))
					{
						float value;
						if (!float.TryParse(args[i++], out value)) return false;
						objectArgs[k] = value;
						continue;
					}
					if (parameter.ParameterType == typeof(double))
					{
						double value;
						if (!double.TryParse(args[i++], out value)) return false;
						objectArgs[k] = value;
						continue;
					}
					if (parameter.ParameterType.IsEnum)
					{
						string val = args[i++];
						if (!Enum.TryParse(parameter.ParameterType, val, true, out object value) || value as Enum == null)
						{
							Core.Log.Warn($"Could not convert to valid enum value: {val}");
							return false;
						}

						objectArgs[k] = value;
						continue;
					}

					if (isStringParam)
					{
						List<string> strings = new List<string>();
						for (; i < args.Length; i++)
						{
							strings.Add(args[i]);
						}
						objectArgs[k] = strings.ToArray();
						continue;
					}

					return false;
				}

				if (i < args.Length) return false;
			}
			catch (Exception e)
			{
				//if (Log.IsDebugEnabled)
				//{
				//	Log.Error("Trying to execute command overload", e);
				//}
				//chat.SendMessage(e);
				Core.Log.Error(chat, e.ToString());

				return false;
			}

			try
			{
				object pluginInstance = _plugins.FirstOrDefault(plugin => method.DeclaringType.IsInstanceOfType(plugin)) ?? method.DeclaringType;
				if (pluginInstance == null) return false;

				ICommandFilter filter = pluginInstance as ICommandFilter;
				if (filter != null)
				{
					filter.OnCommandExecuting(user);
				}

				if (method.IsStatic)
				{
					result = method.Invoke(null, objectArgs);
				}
				else
				{
					if (method.DeclaringType == null) return false;

					Plugin.CurrentContext = context; // Setting thread local for call
					result = method.Invoke(pluginInstance, objectArgs);
					Plugin.CurrentContext = null; // Done with thread local, we using pool to make sure it's reset.
				}

				if (filter != null)
				{
					filter.OnCommandExecuted();
				}

				return true;
			}
			catch (Exception e)
			{
				Core.Log.Error(chat, e.ToString());
				//Log.Error($"Error while executing command {method}", e);
				//chat.SendMessage(e);
			}

			return false;
		}

		internal void PluginCallbackHandler(ref Updates updates, VkCoreApiBase vkApi)
		{
			foreach (var method in _callbackHandlers)
			{
				if (!IsAvailable(method.Key, vkApi.GroupId))
					continue;

				if (method.Value != updates.Type) continue;
				var _method = method.Key;

				object pluginInstance = _plugins.FirstOrDefault(plugin => _method.DeclaringType.IsInstanceOfType(plugin)) ?? _method.DeclaringType;
				if (pluginInstance == null) continue;

				updates = _method.Invoke(pluginInstance, new object[] { updates, vkApi }) as Updates;
			}
		}
	}
}