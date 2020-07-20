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

		[JsonProperty("button_type")]
		public string ButtonType { get; set; }

		public string Payload { get; set; }

		[JsonProperty]
		private string payload { get; set; } // HOTFIX FOR CALLBACK BUTTONS!!!

		[JsonIgnore]
		public EventId EventId { get; internal set; }

		public string Serialize()
		{
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;

			return JsonConvert.SerializeObject(this, settings);
		}

		public static KeyboardButtonPayload Deserialize(string json)
		{
			try
			{
				// HOTFIX FOR CALLBACK BUTTONS!!!
				var settings = new JsonSerializerSettings();
				settings.NullValueHandling = NullValueHandling.Ignore;

				var result = JsonConvert.DeserializeObject<KeyboardButtonPayload>(json, settings);
				if(!string.IsNullOrEmpty(result.payload))
				{
					var buttonType = result.ButtonType;
					result = JsonConvert.DeserializeObject<KeyboardButtonPayload>(result.payload, settings);
					result.ButtonType = buttonType;
				}

				return result;
				// HOTFIX FOR CALLBACK BUTTONS!!!


				//return JsonConvert.DeserializeObject<KeyboardButtonPayload>(json);
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

	public sealed class EventId
	{
		private string _value;

		internal EventId(string value)
		{
			_value = value;
		}

		public static implicit operator string(EventId value)
		{
			return value?._value;
		}

		public void Clear()
		{
			_value = null;
		}

		public override string ToString()
		{
			return _value;
		}
	}
}
