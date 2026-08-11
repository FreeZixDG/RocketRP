using System.Runtime.CompilerServices;
using RocketRP.TrainingGUI.Models;

namespace RocketRP.TrainingGUI.ViewModels
{
	/// <summary>Editable view of the placement of a ball or of a car.</summary>
	public class ArchetypeViewModel : ObservableObject
	{
		private readonly ArchetypePlacement _placement;

		public ArchetypeViewModel(ArchetypePlacement placement)
		{
			_placement = placement;
		}

		public double X
		{
			get => _placement.X;
			set => Set(_placement.X, value, v => _placement.X = v);
		}

		public double Y
		{
			get => _placement.Y;
			set => Set(_placement.Y, value, v => _placement.Y = v);
		}

		public double Z
		{
			get => _placement.Z;
			set => Set(_placement.Z, value, v => _placement.Z = v);
		}

		public int Pitch
		{
			get => _placement.Pitch;
			set => Set(_placement.Pitch, value, v => _placement.Pitch = v);
		}

		public int Yaw
		{
			get => _placement.Yaw;
			set => Set(_placement.Yaw, value, v => _placement.Yaw = v);
		}

		/// <summary>Starting speed in cm/s.</summary>
		public double Speed
		{
			get => _placement.Speed;
			set => Set(_placement.Speed, value, v => _placement.Speed = v);
		}

		/// <summary>Signals that every field may have changed, after the model was edited as a whole.</summary>
		public void Refresh() => OnPropertyChanged(string.Empty);

		private void Set<T>(T current, T value, Action<T> apply, [CallerMemberName] string? propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(current, value)) return;

			apply(value);
			OnPropertyChanged(propertyName);
		}
	}
}
