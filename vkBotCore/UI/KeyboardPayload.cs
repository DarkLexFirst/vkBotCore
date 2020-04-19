using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VkBotCore.UI
{
	[Serializable]
	public class KeyboardButtonPayload
	{
		[JsonProperty("button")]
		public string Button { get; set; }
	}
}
