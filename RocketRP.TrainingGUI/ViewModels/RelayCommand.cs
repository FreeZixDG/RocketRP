using System.Windows.Input;

namespace RocketRP.TrainingGUI.ViewModels
{
	public class RelayCommand : ICommand
	{
		private readonly Action _execute;
		private readonly Func<bool>? _canExecute;

		public RelayCommand(Action execute, Func<bool>? canExecute = null)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		/// <summary>Hooked to the CommandManager so the buttons refresh on their own after each interaction.</summary>
		public event EventHandler? CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

		public void Execute(object? parameter) => _execute();
	}
}
