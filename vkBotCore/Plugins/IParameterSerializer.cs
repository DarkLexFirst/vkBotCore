namespace VkBotCore.Plugins
{
	public interface IParameterSerializer
	{
		void Deserialize(User player, string input);
	}
}