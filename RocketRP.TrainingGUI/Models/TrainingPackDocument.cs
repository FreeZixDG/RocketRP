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

		private const string NameKey = "TM_Name";
		private const string MapKey = "MapName";

		private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

		private readonly JsonNode _root;
		private readonly JsonObject _trainingData;
		private readonly JsonArray _roundNodes;
		private readonly List<TrainingRound> _rounds = [];

		private TrainingPackDocument(JsonNode root, JsonObject trainingData, JsonArray roundNodes)
		{
			_root = root;
			_trainingData = trainingData;
			_roundNodes = roundNodes;
			RebuildRounds();
		}

		public static TrainingPackDocument Load(string jsonPath)
		{
			var root = JsonNode.Parse(File.ReadAllText(jsonPath)) ?? throw new InvalidDataException("Le fichier JSON est vide.");

			var objects = root["Objects"] as JsonArray ?? throw new InvalidDataException("Le fichier JSON n'a pas de liste Objects.");
			var trainingData = objects.OfType<JsonObject>().FirstOrDefault(o => o["ObjectName"]?.GetValue<string>() == TrainingDataObjectName)
				?? throw new InvalidDataException($"Le fichier JSON ne contient pas d'objet {TrainingDataObjectName}.");

			var roundNodes = trainingData["Rounds"] as JsonArray ?? throw new InvalidDataException("Le training pack n'a pas de Rounds.");

			return new TrainingPackDocument(root, trainingData, roundNodes);
		}

		public IReadOnlyList<TrainingRound> Rounds => _rounds;

		/// <summary>Title of the pack as shown in game.</summary>
		public string Name
		{
			get => _trainingData[NameKey]?.GetValue<string>() ?? string.Empty;
			set => _trainingData[NameKey] = value;
		}

		/// <summary>
		/// Map the pack is played on. Only keys listed by <see cref="MapCatalog"/> should be written:
		/// the game crashes on an unknown one.
		/// </summary>
		public string MapName
		{
			get => _trainingData[MapKey]?.GetValue<string>() ?? string.Empty;
			set
			{
				if (value == MapName) return;

				// Write the catalog spelling, never what the caller happened to type.
				_trainingData[MapKey] = MapCatalog.KnownMaps.FirstOrDefault(map => string.Equals(map, value, StringComparison.OrdinalIgnoreCase))
					?? throw new ArgumentException($"Map inconnue : {value}", nameof(value));
			}
		}

		/// <summary>
		/// Inserts a copy of <paramref name="round"/> right after it and returns the copy.
		/// Every level is written back to the document first so that no pending edit is lost,
		/// then the whole list is rebuilt because the numbering shifts.
		/// </summary>
		public TrainingRound Duplicate(TrainingRound round)
		{
			var index = IndexOf(round);

			FlushAll();
			_roundNodes.Insert(index + 1, round.Node.DeepClone());
			RebuildRounds();

			return _rounds[index + 1];
		}

		/// <summary>
		/// Moves a level by <paramref name="offset"/> positions (-1 up, +1 down) and returns its new
		/// number. Nothing happens when the level is already at one end of the pack.
		/// </summary>
		public int Move(TrainingRound round, int offset)
		{
			var index = IndexOf(round);
			var target = index + offset;
			if (target < 0 || target >= _roundNodes.Count) return index + 1;

			FlushAll();

			// Removing detaches the node, which is what lets us insert it back somewhere else.
			var node = _roundNodes[index];
			_roundNodes.RemoveAt(index);
			_roundNodes.Insert(target, node);
			RebuildRounds();

			return target + 1;
		}

		/// <summary>Deletes a level. A pack always keeps at least one.</summary>
		public void Remove(TrainingRound round)
		{
			var index = IndexOf(round);
			if (_roundNodes.Count <= 1) throw new InvalidOperationException("Un pack doit garder au moins un niveau.");

			FlushAll();
			_roundNodes.RemoveAt(index);
			RebuildRounds();
		}

		/// <summary>
		/// Inserts a mirrored copy after every level, so 1, 2, 3 becomes 1, 1', 2, 2', 3, 3'.
		/// </summary>
		public void DuplicateAllMirrored()
		{
			FlushAll();

			// Backwards, so the insertions never shift the levels still to be copied.
			for (var index = _roundNodes.Count - 1; index >= 0; index--)
			{
				if (_roundNodes[index] is { } node) _roundNodes.Insert(index + 1, node.DeepClone());
			}

			RebuildRounds();

			// The copies are now every other level, starting at the second one.
			for (var index = 1; index < _rounds.Count; index += 2) _rounds[index].Mirror();
			FlushAll();
		}

		public void Save(string jsonPath)
		{
			FlushAll();
			File.WriteAllText(jsonPath, _root.ToJsonString(WriteOptions));
		}

		private int IndexOf(TrainingRound round)
		{
			var index = _rounds.IndexOf(round);
			if (index < 0) throw new ArgumentException("Ce niveau n'appartient pas au pack.", nameof(round));

			return index;
		}

		private void FlushAll()
		{
			foreach (var round in _rounds) round.Flush();
		}

		private void RebuildRounds()
		{
			_rounds.Clear();
			_rounds.AddRange(_roundNodes.OfType<JsonObject>().Select((node, index) => new TrainingRound(index + 1, node)));
		}
	}
}
