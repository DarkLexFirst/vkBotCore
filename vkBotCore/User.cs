using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkBotCore.Configuration;
using VkNet.Model;

namespace VkBotCore
{
    public class User : IEquatable<User>
    {
        /// <summary>
        /// VkApi обработчик управляющего сообщества.
        /// </summary>
        public VkCoreApiBase VkApi { get; set; }

        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public long Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        /// <summary>
        /// Определяет, есть ли у пользователя права администратора приложения.
        /// </summary>
        public bool IsAdmin { get => VkApi.Core.Configuration.GetArray<long>("Config:Admins").Contains(Id); }

        /// <summary>
        /// Хранилище.
        /// </summary>
        public Storage Storage { get; set; }

        internal User(VkCoreApiBase vkApi, long id)
        {
            VkApi = vkApi;
            Id = id;
            var u = GetApiUser();
            FirstName = u?.FirstName;
            LastName = u?.LastName;

            Storage = new Storage(this);
        }

        public User(VkCoreApiBase vkApi, long id, string firstName, string lastName)
        {
            VkApi = vkApi;
            FirstName = firstName;
            LastName = lastName;
            Id = id;

            Storage = new Storage(this);
        }

        /// <summary>
        /// Опредеяет, является ли пользователь администратором диалога.
        /// </summary>
        public bool IsChatAdmin(Chat chat)
        {
            return chat.GetAllChatAdministrators().Contains(Id);
        }

        /// <summary>
        /// Возвращает личную переписку сообщества с данным пользователем.
        /// </summary>
        public Chat GetConversation() => VkApi.GetChat(Id);

        /// <summary>
        /// Возвращает строку упоминания.
        /// </summary>
        public string GetMentionLine()
        {
            return GetMentionLine(Id, FirstName);
        }

        /// <summary>
        /// Возвращает строку упоминания.
        /// </summary>
        public static string GetMentionLine(long id, string value)
        {
            return id >= 0 ? $"[id{id}|{value ?? id.ToString()}]" : string.Empty;
        }

        public VkNet.Model.User GetApiUser()
        {
            return GetApiUserById(VkApi, Id);
        }

        public static VkNet.Model.User GetApiUserById(VkCoreApiBase vkApi, long id)
        {
            if (id <= 0) return null;
            return vkApi.Users.Get(new long[] { id }).First();
        }

        public static async Task<VkNet.Model.User> GetApiUserByIdAsync(VkCoreApiBase vkApi, long id)
        {
            if (id <= 0) return null;
            IReadOnlyCollection<VkNet.Model.User> users;
            users = await vkApi.Users.GetAsync(new long[] { id });
            return users.First();
        }

        public override bool Equals(object obj) => obj is User user && Equals(user);
        public bool Equals(User other) => other.Id == Id;

        public override int GetHashCode() => (int)Id;
    }
}
