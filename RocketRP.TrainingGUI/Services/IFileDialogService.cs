namespace RocketRP.TrainingGUI.Services
{
	/// <summary>User interactions that need a window, kept out of the view models.</summary>
	public interface IFileDialogService
	{
		/// <summary>Asks the user for a .Tem or .json training pack. Returns null if cancelled.</summary>
		string? PickTrainingFile();

		/// <summary>Asks the user a yes/no question. Returns true if confirmed.</summary>
		bool Confirm(string message);
	}
}
