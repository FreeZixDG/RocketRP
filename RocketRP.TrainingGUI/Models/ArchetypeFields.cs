using System.Globalization;
using System.Text.Json;

namespace RocketRP.TrainingGUI.Models
{
	/// <summary>
	/// One entry of a round's SerializedArchetypes: a small JSON object stored as a string.
	/// Values are kept as their raw JSON text so every field we don't edit survives untouched.
	/// </summary>
	public class ArchetypeFields
	{
		public const string ObjectArchetypeKey = "ObjectArchetype";

		private readonly List<KeyValuePair<string, string>> _fields = [];

		public static ArchetypeFields Parse(string json)
		{
			var fields = new ArchetypeFields();
			using var document = JsonDocument.Parse(json);
			foreach (var property in document.RootElement.EnumerateObject())
			{
				fields._fields.Add(new KeyValuePair<string, string>(property.Name, property.Value.GetRawText()));
			}

			return fields;
		}

		public string ToJson() => "{" + string.Join(",", _fields.Select(field => $"\"{field.Key}\":{field.Value}")) + "}";

		/// <summary>Value of <see cref="ObjectArchetypeKey"/>, which tells apart the ball, the car and the camera.</summary>
		public string? ObjectArchetype => TryGetRaw(ObjectArchetypeKey, out var raw) ? JsonSerializer.Deserialize<string>(raw) : null;

		public double GetDouble(string key)
		{
			return TryGetRaw(key, out var raw) && double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0d;
		}

		public int GetInt(string key)
		{
			return TryGetRaw(key, out var raw) && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;
		}

		/// <summary>Writes a decimal value using the 4 decimals the game itself writes.</summary>
		public void SetDouble(string key, double value) => SetRaw(key, value.ToString("0.0000", CultureInfo.InvariantCulture));

		public void SetInt(string key, int value) => SetRaw(key, value.ToString(CultureInfo.InvariantCulture));

		private bool TryGetRaw(string key, out string raw)
		{
			var index = IndexOf(key);
			raw = index < 0 ? string.Empty : _fields[index].Value;
			return index >= 0;
		}

		private void SetRaw(string key, string raw)
		{
			var field = new KeyValuePair<string, string>(key, raw);
			var index = IndexOf(key);
			if (index < 0) _fields.Add(field);
			else _fields[index] = field;
		}

		private int IndexOf(string key) => _fields.FindIndex(field => field.Key == key);
	}
}
