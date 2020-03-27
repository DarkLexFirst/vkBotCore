namespace VkBotCore.Plugins
{
	public interface ICommandFilter
	{
		void OnCommandExecuting(User player);
		void OnCommandExecuted();
	}
}