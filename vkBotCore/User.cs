using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vkBotCore.Configuration;

namespace vkBotCore
{
    public class User
    {
        public VkCoreApiBase VkApi { get; set; }

        public long Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public bool IsAdmin { get => VkApi.Core.Configuration.GetArray<long>("Config:Admins").Contains(Id); }

        public User(VkCoreApiBase vkApi, long id)
        {
            VkApi = vkApi;
            Id = id;
            var u = GetApiUser();
            FirstName = u?.FirstName;
            LastName = u?.LastName;
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

        public string GetMentionLine()
        {
            return GetMentionLine(Id, FirstName);
        }

        public static string GetMentionLine(long id, string value)
        {
            return $"@id{id} ({value})";
        }
        public VkNet.Model.User GetApiUser()
        {
            return GetApiUserPyId(VkApi, Id);
        }

        public static VkNet.Model.User GetApiUserPyId(VkCoreApiBase vkApi, long id)
        {
            if (id < 0) return null;
            return vkApi.Users.Get(new long[] { id }).First();
        }

        public override bool Equals(object obj) => obj is User user && Id == user.Id;

        public override int GetHashCode() => (int)Id;
    }
}
