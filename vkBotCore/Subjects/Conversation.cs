using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VkBotCore.Subjects
{
	/// <summary>
	/// Класс для взаимодействия с диалогом пользователя.
	/// </summary>
	public class Conversation : BaseChat
	{
		/// <summary>
		/// Пользовател, с которым ведётся диалог.
		/// </summary>
		public User User { get; set; }

		public Conversation(VkCoreApiBase vkApi, long peerId) : base(vkApi, peerId)
		{
			User = vkApi.GetUser<User>(peerId);
		}
	}
}
