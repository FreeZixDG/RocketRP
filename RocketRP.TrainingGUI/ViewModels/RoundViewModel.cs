using System.ComponentModel;
using RocketRP.TrainingGUI.Models;

namespace RocketRP.TrainingGUI.ViewModels
{
	/// <summary>Editable view of a single level of the pack.</summary>
	public class RoundViewModel : ObservableObject
	{
		private readonly TrainingRound _round;

		public RoundViewModel(TrainingRound round)
		{
			_round = round;
			Ball = ArchetypeViewModel.ForBall(round.Ball);
			Car = ArchetypeViewModel.ForCar(round.Car);

			// Bubble up nested edits so the main view model can flag the document as modified.
			Ball.PropertyChanged += OnChildChanged;
			Car.PropertyChanged += OnChildChanged;
		}

		public int Number => _round.Number;

		public string DisplayName => $"Niveau {_round.Number}";

		public ArchetypeViewModel Ball { get; }

		public ArchetypeViewModel Car { get; }

		/// <summary>Seconds available to complete the level, clamped to the range the game accepts.</summary>
		public int TimeLimit
		{
			get => _round.TimeLimit;
			set
			{
				if (_round.TimeLimit == value) return;

				_round.TimeLimit = value;
				OnPropertyChanged();
			}
		}

		private void OnChildChanged(object? sender, PropertyChangedEventArgs e) => OnPropertyChanged(sender == Ball ? nameof(Ball) : nameof(Car));
	}
}
