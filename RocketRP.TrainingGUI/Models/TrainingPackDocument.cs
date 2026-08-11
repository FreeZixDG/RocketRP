using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RocketRP.TrainingGUI.Models
{
	/// <summary>
	/// The JSON file of a training pack, kept as a node tree so that everything we must not touch
	/// (VersionInfo, Code, TM_Guid, CreatedAt, UpdatedAt, ...) is written back exactly as it was read.
	/// </summary>
	public class TrainingPackDocument
	{
		public const string TrainingDataObjectName = "TAGame.TrainingEditorData_TA";

		private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

		private readonly JsonNode _root;
		private readonly JsonObject _trainingData;

		private TrainingPackDocument(JsonNode root, JsonObject trainingData, IReadOnlyList<TrainingRound> rounds)
		{
			_root = root;
			_trainingData = trainingData;
			Rounds = rounds;
		}

		public static TrainingPackDocument Load(string jsonPath)
		{
			var root = JsonNode.Parse(File.ReadAllText(jsonPath)) ?? throw new InvalidDataException("Le fichier JSON est vide.");

			var objects = root["Objects"] as JsonArray ?? throw new InvalidDataException("Le fichier JSON n'a pas de liste Objects.");
			var trainingData = objects.OfType<JsonObject>().FirstOrDefault(o => o["ObjectName"]?.GetValue<string>() == TrainingDataObjectName)
				?? throw new InvalidDataException($"Le fichier JSON ne contient pas d'objet {TrainingDataObjectName}.");

			var roundNodes = trainingData["Rounds"] as JsonArray ?? throw new InvalidDataException("Le training pack n'a pas de Rounds.");
			var rounds = roundNodes.OfType<JsonObject>().Select((node, index) => new TrainingRound(index + 1, node)).ToList();

			return new TrainingPackDocument(root, trainingData, rounds);
		}

		public IReadOnlyList<TrainingRound> Rounds { get; }

		/// <summary>Name given to the pack in game, read-only here.</summary>
		public string? Name => _trainingData["TM_Name"]?.GetValue<string>();

		/// <summary>Name of the map the pack was created on, read-only here.</summary>
		public string? MapName => _trainingData["MapName"]?.GetValue<string>();

		public void Save(string jsonPath)
		{
			foreach (var round in Rounds) round.Flush();
			File.WriteAllText(jsonPath, _root.ToJsonString(WriteOptions));
		}
	}
}
