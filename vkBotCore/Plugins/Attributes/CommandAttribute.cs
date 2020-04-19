using System;

namespace VkBotCore.Plugins.Attributes
{
	/// <summary>
	/// Помечает метод как команду.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public class CommandAttribute : Attribute
	{
		/// <summary>
		/// Название команды.
		/// </summary>
		public string Name;
		public string Overload;

		/// <summary>
		/// Вспомогательные имена для обращения к команде.
		/// </summary>
		public string[] Aliases;

		/// <summary>
		/// Описание.
		/// </summary>
		public string Description;

		/// <summary>
		/// Скрывать команду в /help.
		/// </summary>
		public bool IsHidden = false;

		/// <summary>
		/// Отображать развёрнутое описание.
		/// </summary>
		public bool UseFullDescription = false;
	}

	/// <summary>
	/// Задаёт перегрузку команды, как стандартную в /help.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = true)]
	public class DefaultCommand : Attribute
	{

	}

	/// <summary>
	/// Задаёт кастомное название параметра, отображаемого в /help.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, Inherited = true)]
	public class ParamName : Attribute
	{
		public string Name { get; }
		public ParamName(string name) => Name = name;
	}

	/// <summary>
	/// Отображает параметр, как подкоманда в /help. (приминимо только к enum типам).
	/// <code>&lt;param> → enumValName</code>
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, Inherited = true)]
	public class SubCommand : Attribute
	{

	}

	/// <summary>
	/// Скрывает стандартное значение опционального параметка в /help.
	/// <code>[param=defval] → [param]</code>
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, Inherited = true)]
	public class HideDefault : Attribute
	{

	}

	/// <summary>
	/// Отображает параметр, как опциональный в /help.
	/// <code>&lt;param> → [param]</code>
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, Inherited = true)]
	public class Optimal : Attribute
	{

	}
}