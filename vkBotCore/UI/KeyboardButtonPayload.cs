using Newtonsoft.Json;

namespace VkBotCore.UI
{
	public class KeyboardButtonPayload
	{
		public long GroupId { get; set; }
		public string KeyboardId { get; set; }
		public string ButtonId { get; set; }

		[JsonProperty("command")]
		private string _command
		{
			set
			{
				KeyboardId = value;
				ButtonId = value;
			}
		}

		public string Payload { get; set; }

		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}

		public static KeyboardButtonPayload Deserialize(string json)
		{
			try
			{
				return JsonConvert.DeserializeObject<KeyboardButtonPayload>(json);
			}
			catch
			{
				return null;
			}
		}

		public bool IsValid()
		{
			return KeyboardId != null && ButtonId != null;
		}
	}
}
