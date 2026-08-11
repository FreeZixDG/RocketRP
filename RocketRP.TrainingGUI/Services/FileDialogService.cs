using System.Windows;
using Microsoft.Win32;

namespace RocketRP.TrainingGUI.Services
{
	public class FileDialogService : IFileDialogService
	{
		public string? PickTrainingFile()
		{
			var dialog = new OpenFileDialog
			{
				Title = "Choisir un training pack",
				Filter = "Training pack (*.Tem;*.json)|*.Tem;*.json|Training pack binaire (*.Tem)|*.Tem|JSON (*.json)|*.json",
				CheckFileExists = true,
			};

			return dialog.ShowDialog() == true ? dialog.FileName : null;
		}

		public bool Confirm(string message)
		{
			return MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
		}
	}
}
