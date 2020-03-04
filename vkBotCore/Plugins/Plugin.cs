using System;

namespace vkBotCore.Plugins
{
	public abstract class Plugin : IPlugin
	{
		protected PluginManager PluginManager { get; set; }

		[ThreadStatic] public static CommandContext CurrentContext = null;

        public VkCoreApiBase[] AvailableApis { get; private set; }

		public void OnEnable(PluginManager pluginManager)
		{
            PluginManager = pluginManager;
            AvailableApis = pluginManager.GetAvailableApis(GetType());
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