using System;

namespace VkBotCore.Plugins
{
	public abstract class Plugin : IPlugin
	{
		protected PluginManager PluginManager { get; set; }

		[ThreadStatic] public static CommandContext CurrentContext = null;

        /// <summary>
        /// API сообществ, использующих пространство имён, содержащее данный плагин.
        /// </summary>
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