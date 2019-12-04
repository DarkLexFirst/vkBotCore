using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vkBotCore
{
    public class User
    {
        public BotCore Core { get; set; }

        public long Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public bool IsAdmin { get => Core.Configuration.GetValue("Config:Admins", new long[0]).Contains(Id); }

        public User(BotCore core, long id)
        {
            Core = core;
            Id = id;
            var u = GetApiUserPyId(core, id);
            FirstName = u.FirstName;
            LastName = u.LastName;
        }

        public User(long id, string firstName, string lastName)
        {
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

        public static VkNet.Model.User GetApiUserPyId(BotCore core, long id)
        {
            return core.VkApi.Users.Get(new long[] { id }).First();
        }

        public override bool Equals(object obj) => obj is User user && Id == user.Id;

        public override int GetHashCode() => (int)Id;
    }
}
