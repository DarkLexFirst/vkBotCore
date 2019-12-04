using System;

namespace vkBotCore.Plugins
{
	public abstract class Plugin : IPlugin
	{
		protected PluginManager PluginManager { get; set; }

		[ThreadStatic] public static CommandContext CurrentContext = null;

		public void OnEnable(PluginManager pluginManager)
		{
            PluginManager = pluginManager;
			OnEnable();
		}

		protected virtual void OnEnable()
		{
		}

		public virtual void OnDisable()
		{
		}
	}
}