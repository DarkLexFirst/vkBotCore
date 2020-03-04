using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vkBotCore.Configuration;
using VkNet.Model;

namespace vkBotCore
{
    public class User : IEquatable<User>
    {
        public VkCoreApiBase VkApi { get; set; }

        public long Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public bool IsAdmin { get => VkApi.Core.Configuration.GetArray<long>("Config:Admins").Contains(Id); }

        public Storage Storage { get; set; }

        public User(VkCoreApiBase vkApi, long id)
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
        }

        public bool IsChatAdmin(Chat chat)
        {
            return chat.GetAllChatAdministrators().Contains(Id);
        }

        public Chat GetConversation() => VkApi.GetChat(Id);

        public string GetMentionLine()
        {
            return GetMentionLine(Id, FirstName);
        }

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
            if (id < 0) return null;
            return vkApi.Users.Get(new long[] { id }).First();
        }

        public override bool Equals(object obj) => obj is User user && Equals(user);
        public bool Equals(User other) => other.Id == Id;

        public override int GetHashCode() => (int)Id;
    }
}
