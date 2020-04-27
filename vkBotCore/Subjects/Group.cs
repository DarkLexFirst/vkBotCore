using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VkBotCore.Subjects
{
	public class Group : IUser, IEquatable<Group>
	{
		/// <summary>
		/// VkApi обработчик управляющего сообщества.
		/// </summary>
		public VkCoreApiBase VkApi { get; set; }

		/// <summary>
		/// Идентификатор сообщества.
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		/// Название сообщества.
		/// </summary>
		public string Name { get; set; }

		internal Group(VkCoreApiBase vkApi, long id)
		{
			VkApi = vkApi;
			Id = id;
		}

		/// <summary>
		/// Опредеяет, является ли сообщество администратором диалога.
		/// </summary>
		public bool IsChatAdmin(Chat chat)
		{
			return chat.GetAllChatAdministrators().Contains(Id);
		}

		/// <summary>
		/// Возвращает строку упоминания.
		/// </summary>
		public string GetMentionLine()
		{
			return GetMentionLine(Id, Name);
		}

		/// <summary>
		/// Возвращает строку упоминания.
		/// </summary>
		public static string GetMentionLine(long id, string value)
		{
			return id >= 0 ? $"[id{id}|{value ?? id.ToString()}]" : string.Empty;
		}

		public VkNet.Model.Group GetApiGroup()
		{
			return GetApiGroupById(VkApi, Id);
		}

		public static VkNet.Model.Group GetApiGroupById(VkCoreApiBase vkApi, long id)
		{
			if (id <= 0) return null;
			return vkApi.Groups.GetById(null, Math.Abs(-id).ToString(), null).First();
		}

		public static async Task<VkNet.Model.Group> GetApiGroupByIdAsync(VkCoreApiBase vkApi, long id)
		{
			if (id <= 0) return null;
			IReadOnlyCollection<VkNet.Model.Group> groups;
			groups = await vkApi.Groups.GetByIdAsync(null, Math.Abs(-id).ToString(), null);
			return groups.First();
		}

		public override bool Equals(object obj) => obj is Group group && Equals(group);
		public bool Equals(Group other) => other.Id == Id;

		public override int GetHashCode() => (int)Id;
	}
}
