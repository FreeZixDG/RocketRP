using System.IO;
using System.Text.Json.Nodes;

namespace RocketRP.TrainingGUI.Models
{
	/// <summary>
	/// One level of a training pack. Wraps the JSON node so edits land straight in the document,
	/// and only touches the ball and the car: the third archetype (the edition preview camera)
	/// has no effect in game and is left as-is.
	/// </summary>
	public class TrainingRound
	{
		public const string BallArchetype = "Archetypes.Ball.Ball_GameEditor";
		public const string CarArchetype = "Archetypes.GameEditor.DynamicSpawnPointMesh";
		public const int MinTimeLimit = 1;
		public const int MaxTimeLimit = 60;

		private const string TimeLimitKey = "TimeLimit";
		private const string SerializedArchetypesKey = "SerializedArchetypes";

		private readonly JsonArray _archetypes;
		private readonly ArchetypeFields _ballFields;
		private readonly ArchetypeFields _carFields;
		private readonly int _ballIndex;
		private readonly int _carIndex;

		public TrainingRound(int number, JsonObject round)
		{
			Number = number;
			Node = round;
			_archetypes = round[SerializedArchetypesKey] as JsonArray
				?? throw new InvalidDataException($"Le niveau {number} n'a pas de {SerializedArchetypesKey}.");

			var fields = _archetypes.Select(archetype => ArchetypeFields.Parse(archetype?.GetValue<string>() ?? "{}")).ToList();
			_ballIndex = fields.FindIndex(field => field.ObjectArchetype == BallArchetype);
			_carIndex = fields.FindIndex(field => field.ObjectArchetype == CarArchetype);
			if (_ballIndex < 0 || _carIndex < 0) throw new InvalidDataException($"Le niveau {number} n'a pas de balle et/ou de voiture.");

			_ballFields = fields[_ballIndex];
			_carFields = fields[_carIndex];
			Ball = ArchetypePlacement.ForBall(_ballFields);
			Car = ArchetypePlacement.ForCar(_carFields);
		}

		/// <summary>1-based position of the level inside the pack.</summary>
		public int Number { get; }

		/// <summary>The node of the level inside the document, used to duplicate it.</summary>
		public JsonObject Node { get; }

		public ArchetypePlacement Ball { get; }

		public ArchetypePlacement Car { get; }

		/// <summary>Seconds available to complete the level.</summary>
		public int TimeLimit
		{
			get => Node[TimeLimitKey]?.GetValue<int>() ?? MinTimeLimit;
			set => Node[TimeLimitKey] = Math.Clamp(value, MinTimeLimit, MaxTimeLimit);
		}

		/// <summary>Mirrors the level left/right, ball and car alike.</summary>
		public void Mirror()
		{
			Ball.Mirror();
			Car.Mirror();
		}

		/// <summary>Writes the ball and car fields back into the JSON document.</summary>
		public void Flush()
		{
			_archetypes[_ballIndex] = _ballFields.ToJson();
			_archetypes[_carIndex] = _carFields.ToJson();
		}
	}
}
