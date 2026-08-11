using System.ComponentModel;
using RocketRP.TrainingGUI.Models;

namespace RocketRP.TrainingGUI.ViewModels
{
	/// <summary>Editable view of a single level of the pack.</summary>
	public class RoundViewModel : ObservableObject
	{
		public RoundViewModel(TrainingRound round)
		{
			Round = round;
			Ball = new ArchetypeViewModel(round.Ball);
			Car = new ArchetypeViewModel(round.Car);

			// Bubble up nested edits so the main view model can flag the document as modified.
			Ball.PropertyChanged += OnChildChanged;
			Car.PropertyChanged += OnChildChanged;
		}

		public TrainingRound Round { get; }

		public int Number => Round.Number;

		public string DisplayName => $"Niveau {Round.Number}";

		public ArchetypeViewModel Ball { get; }

		public ArchetypeViewModel Car { get; }

		/// <summary>Seconds available to complete the level, clamped to the range the game accepts.</summary>
		public int TimeLimit
		{
			get => Round.TimeLimit;
			set
			{
				if (Round.TimeLimit == value) return;

				Round.TimeLimit = value;
				OnPropertyChanged();
			}
		}

		/// <summary>Mirrors the level left/right and refreshes every field of the editor.</summary>
		public void Mirror()
		{
			Round.Mirror();
			Ball.Refresh();
			Car.Refresh();
		}

		private void OnChildChanged(object? sender, PropertyChangedEventArgs e) => OnPropertyChanged(sender == Ball ? nameof(Ball) : nameof(Car));
	}
}
