namespace VkBotCore.Utils
{
	public enum UserPermission : short
	{
		Block = short.MinValue,
		None = 0,
		Modarator = 100,
		Editor = 1000,
		Admin = 10000,
		Unlimited = short.MaxValue
	}
}
