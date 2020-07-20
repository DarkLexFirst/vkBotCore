using System;
using VkNet.Enums.SafetyEnums;

namespace VkBotCore.Subjects
{
	public interface IUser
	{
		VkCoreApiBase VkApi { get; set; }

		long Id { get; set; }

		bool IsChatAdmin(Chat chat);
		string GetMentionLine(NameCase nameCase = null);
	}

	public class UserEventArgs : EventArgs
	{
		public IUser User { get; }

		public UserEventArgs(IUser user)
		{
			User = user;
		}
	}
}
