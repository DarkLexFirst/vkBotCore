namespace VkBotCore.Utils
{
	public enum UserPermission : short
	{
		Block = short.MinValue,
		None = 0,
		Admin = 1000,
		Unlimited = short.MaxValue
	}
}
