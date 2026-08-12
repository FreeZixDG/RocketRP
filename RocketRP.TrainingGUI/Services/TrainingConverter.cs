using System.IO;
using RocketRP.Actors.TAGame;
using RocketRP.Serializers;

namespace RocketRP.TrainingGUI.Services
{
	/// <summary>
	/// Converts a training pack between its binary (.Tem) and JSON representations.
	/// Both files always live in the same directory and share the same name.
	/// </summary>
	public class TrainingConverter
	{
		public const string TemExtension = ".Tem";
		public const string JsonExtension = ".json";

		private readonly SaveDataJsonSerializer _serializer = new();

		public static string GetJsonPath(string temPath) => Path.ChangeExtension(temPath, JsonExtension);

		public static string GetTemPath(string jsonPath) => Path.ChangeExtension(jsonPath, TemExtension);

		/// <summary>Deserializes a .Tem file and writes the JSON next to it. Returns the JSON path.</summary>
		public string TemToJson(string temPath, bool prettyPrint = true)
		{
			var jsonPath = GetJsonPath(temPath);
			File.WriteAllText(jsonPath, TemToJsonText(temPath, prettyPrint));
			return jsonPath;
		}

		/// <summary>Serializes a JSON file back into a .Tem file next to it. Returns the .Tem path.</summary>
		public string JsonToTem(string jsonPath)
		{
			var temPath = GetTemPath(jsonPath);
			JsonTextToTem(File.ReadAllText(jsonPath), temPath);
			return temPath;
		}

		/// <summary>Reads a .Tem file and returns its JSON, without writing anything.</summary>
		public string TemToJsonText(string temPath, bool prettyPrint = true)
		{
			var training = SaveData<SaveData_GameEditor_Training_TA>.Deserialize(temPath);
			return _serializer.Serialize(training, prettyPrint);
		}

		/// <summary>Writes JSON held in memory to the given .Tem file, replacing it.</summary>
		public void JsonTextToTem(string json, string temPath)
		{
			var training = _serializer.Deserialize<SaveData_GameEditor_Training_TA>(json);
			training.Serialize(temPath);
		}
	}
}
