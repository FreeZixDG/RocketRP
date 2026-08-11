using System.Runtime.CompilerServices;
using RocketRP.TrainingGUI.Models;

namespace RocketRP.TrainingGUI.ViewModels
{
	/// <summary>
	/// Editable view of a ball or car placement. X is the width of the field, Y its length, Z the height;
	/// Pitch is the up/down rotation, Yaw the left/right one (roll is not editable in game, always 0).
	/// </summary>
	public class ArchetypeViewModel : ObservableObject
	{
		private const string SpeedKey = "VelocityStartSpeed";

		private readonly ArchetypeFields _fields;
		private readonly string _locationPrefix;
		private readonly string _rotationPrefix;

		private ArchetypeViewModel(ArchetypeFields fields, string locationPrefix, string rotationPrefix)
		{
			_fields = fields;
			_locationPrefix = locationPrefix;
			_rotationPrefix = rotationPrefix;
		}

		public static ArchetypeViewModel ForBall(ArchetypeFields fields) => new(fields, "StartLocation", "VelocityStartRotation");

		public static ArchetypeViewModel ForCar(ArchetypeFields fields) => new(fields, "Location", "Rotation");

		public double X
		{
			get => _fields.GetDouble(_locationPrefix + "X");
			set => SetDouble(_locationPrefix + "X", value);
		}

		public double Y
		{
			get => _fields.GetDouble(_locationPrefix + "Y");
			set => SetDouble(_locationPrefix + "Y", value);
		}

		public double Z
		{
			get => _fields.GetDouble(_locationPrefix + "Z");
			set => SetDouble(_locationPrefix + "Z", value);
		}

		public int Pitch
		{
			get => _fields.GetInt(_rotationPrefix + "P");
			set => SetInt(_rotationPrefix + "P", value);
		}

		public int Yaw
		{
			get => _fields.GetInt(_rotationPrefix + "Y");
			set => SetInt(_rotationPrefix + "Y", value);
		}

		/// <summary>Starting speed in cm/s.</summary>
		public double Speed
		{
			get => _fields.GetDouble(SpeedKey);
			set => SetDouble(SpeedKey, value);
		}

		private void SetDouble(string key, double value, [CallerMemberName] string? propertyName = null)
		{
			if (_fields.GetDouble(key).Equals(value)) return;

			_fields.SetDouble(key, value);
			OnPropertyChanged(propertyName);
		}

		private void SetInt(string key, int value, [CallerMemberName] string? propertyName = null)
		{
			if (_fields.GetInt(key) == value) return;

			_fields.SetInt(key, value);
			OnPropertyChanged(propertyName);
		}
	}
}
