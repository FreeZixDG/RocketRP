using System.Windows;
using RocketRP.TrainingGUI.Services;
using RocketRP.TrainingGUI.ViewModels;

namespace RocketRP.TrainingGUI.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = new MainViewModel(new TrainingConverter(), new FileDialogService());
		}
	}
}
