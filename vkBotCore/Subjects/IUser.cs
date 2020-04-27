namespace VkBotCore.Subjects
{
	public interface IUser
	{
		VkCoreApiBase VkApi { get; set; }

		long Id { get; set; }

		bool IsChatAdmin(Chat chat);
		string GetMentionLine();
	}
}
