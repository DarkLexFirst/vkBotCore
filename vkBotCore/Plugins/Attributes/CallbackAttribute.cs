using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vkBotCore.Plugins.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class CallbackReceive : Attribute
    {
        public string Type { get; } 
        public CallbackReceive(string type) => Type = type;
    }

    //[AttributeUsage(AttributeTargets.Method, Inherited = true)]
    //public class MessageReceive : Attribute
    //{
    //    public string ActionType { get; } 
    //    public MessageReceive(string actionType) => ActionType = actionType;
    //}
}
