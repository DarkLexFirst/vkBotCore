namespace VkBotCore.Plugins
{
	public interface IPlugin
	{
		/// <summary>
		///     This function will be called on plugin initialization.
		/// </summary>
		void OnEnable(PluginManager pluginManager);

		/// <summary>
		///     This function will be called when the plugin will be disabled.s
		/// </summary>
		void OnDisable();
	}
}