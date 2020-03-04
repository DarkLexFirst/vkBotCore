using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vkBotCore.UI
{
    [Serializable]
    public class TextButtonPayload
    {
        [JsonProperty("button")]
        public string Button { get; set; }
    }
}
