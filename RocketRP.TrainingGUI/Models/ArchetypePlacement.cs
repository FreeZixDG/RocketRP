namespace RocketRP.TrainingGUI.Models
{
	/// <summary>
	/// Placement of the ball or of the car inside a level, on top of the raw fields of its archetype.
	/// X is the width of the field, Y its length, Z the height; Pitch is the up/down rotation and Yaw
	/// the left/right one (roll is not editable in game and stays 0).
	/// </summary>
	public class ArchetypePlacement
	{
		/// <summary>Half a turn in the rotation units of the game (a full turn is 65536).</summary>
		public const int YawHalfTurn = 32768;

		private const string SpeedKey = "VelocityStartSpeed";

		private readonly ArchetypeFields _fields;
		private readonly string _locationPrefix;
		private readonly string _rotationPrefix;

		private ArchetypePlacement(ArchetypeFields fields, string locationPrefix, string rotationPrefix)
		{
			_fields = fields;
			_locationPrefix = locationPrefix;
			_rotationPrefix = rotationPrefix;
		}

		public static ArchetypePlacement ForBall(ArchetypeFields fields) => new(fields, "StartLocation", "VelocityStartRotation");

		public static ArchetypePlacement ForCar(ArchetypeFields fields) => new(fields, "Location", "Rotation");

		public double X
		{
			get => _fields.GetDouble(_locationPrefix + "X");
			set => _fields.SetDouble(_locationPrefix + "X", value);
		}

		public double Y
		{
			get => _fields.GetDouble(_locationPrefix + "Y");
			set => _fields.SetDouble(_locationPrefix + "Y", value);
		}

		public double Z
		{
			get => _fields.GetDouble(_locationPrefix + "Z");
			set => _fields.SetDouble(_locationPrefix + "Z", value);
		}

		public int Pitch
		{
			get => _fields.GetInt(_rotationPrefix + "P");
			set => _fields.SetInt(_rotationPrefix + "P", value);
		}

		public int Yaw
		{
			get => _fields.GetInt(_rotationPrefix + "Y");
			set => _fields.SetInt(_rotationPrefix + "Y", value);
		}

		/// <summary>Starting speed in cm/s.</summary>
		public double Speed
		{
			get => _fields.GetDouble(SpeedKey);
			set => _fields.SetDouble(SpeedKey, value);
		}

		/// <summary>
		/// Mirrors the placement left/right: X changes sign and the yaw becomes 32768 - yaw.
		/// Pitch and height are untouched.
		/// </summary>
		public void Mirror()
		{
			X = -X;
			Yaw = YawHalfTurn - Yaw;
		}
	}
}
