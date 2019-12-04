using System;

namespace vkBotCore.Plugins.Attributes
{
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public class CommandAttribute : Attribute
	{
		public string Name;
		public string Overload;
		public string[] Aliases;
		public string Description;

        public bool IsHidden = false;
        public bool UseFullDescription = false;
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class DefaultCommand : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = true)]
    public class ParamName : Attribute
    {
        public string Name { get; }
        public ParamName(string name) => Name = name;
    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = true)]
    public class SubCommand : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = true)]
    public class HideDefault : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = true)]
    public class Optimal : Attribute
    {

    }
}