using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json;

namespace vkBotCore.Plugins
{
	public class CommandSet : Dictionary<string, Command>
	{
	}

	public class Command
	{
		public string Name { get; set; }

        public Dictionary<string, Overload> Overloads { get; set; }
	}

	public class Overload
	{
		public string[] Aliases { get; set; }
		public string Description { get; set; }

		public bool IsHidden { get; set; }

        public MethodInfo Method { get; set; }

        public bool UseFullDescription { get; set; }
    }
}