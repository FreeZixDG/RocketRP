using System.IO;
using System.Text;
using System.Windows.Input;
using RocketRP.TrainingGUI.Services;

namespace RocketRP.TrainingGUI.ViewModels
{
	public class MainViewModel : ObservableObject
	{
		private readonly TrainingConverter _converter;
		private readonly IFileDialogService _dialogService;
		private readonly StringBuilder _log = new();

		private string? _jsonPath;
		private string _jsonFileName = string.Empty;

		public MainViewModel(TrainingConverter converter, IFileDialogService dialogService)
		{
			_converter = converter;
			_dialogService = dialogService;

			ImportCommand = new RelayCommand(Import);
			RenameCommand = new RelayCommand(Rename, () => HasJson);
			ConvertToTemCommand = new RelayCommand(ConvertToTem, () => HasJson);
		}

		public ICommand ImportCommand { get; }
		public ICommand RenameCommand { get; }
		public ICommand ConvertToTemCommand { get; }

		/// <summary>Full path of the JSON file currently loaded, null while nothing is loaded.</summary>
		public string? JsonPath
		{
			get => _jsonPath;
			private set
			{
				if (!SetProperty(ref _jsonPath, value)) return;
				OnPropertyChanged(nameof(HasJson));
			}
		}

		public bool HasJson => _jsonPath != null;

		/// <summary>File name of the JSON file, editable by the user and applied by <see cref="RenameCommand"/>.</summary>
		public string JsonFileName
		{
			get => _jsonFileName;
			set => SetProperty(ref _jsonFileName, value);
		}

		public string Log => _log.ToString();

		private void Import()
		{
			var path = _dialogService.PickTrainingFile();
			if (path == null) return;

			try
			{
				if (string.Equals(Path.GetExtension(path), TrainingConverter.TemExtension, StringComparison.OrdinalIgnoreCase))
				{
					AppendLog($"Import de {Path.GetFileName(path)}...");
					SetJsonPath(_converter.TemToJson(path));
					AppendLog($"Converti en JSON : {JsonPath}");
				}
				else
				{
					SetJsonPath(path);
					AppendLog($"JSON chargé : {JsonPath}");
				}
			}
			catch (Exception e)
			{
				AppendLog($"Échec de l'import de {path} : {e.Message}");
			}
		}

		private void Rename()
		{
			if (_jsonPath == null) return;

			var newName = JsonFileName.Trim();
			if (!newName.EndsWith(TrainingConverter.JsonExtension, StringComparison.OrdinalIgnoreCase)) newName += TrainingConverter.JsonExtension;

			var directory = Path.GetDirectoryName(_jsonPath);
			if (string.IsNullOrEmpty(directory) || newName.Intersect(Path.GetInvalidFileNameChars()).Any())
			{
				AppendLog($"Nom de fichier invalide : {newName}");
				return;
			}

			var newPath = Path.Combine(directory, newName);
			if (string.Equals(newPath, _jsonPath, StringComparison.OrdinalIgnoreCase))
			{
				JsonFileName = Path.GetFileName(_jsonPath);
				return;
			}

			if (File.Exists(newPath) && !_dialogService.Confirm($"{newName} existe déjà. L'écraser ?"))
			{
				JsonFileName = Path.GetFileName(_jsonPath);
				return;
			}

			try
			{
				File.Move(_jsonPath, newPath, true);
				SetJsonPath(newPath);
				AppendLog($"Renommé en {newName}");
			}
			catch (Exception e)
			{
				AppendLog($"Échec du renommage : {e.Message}");
				JsonFileName = Path.GetFileName(_jsonPath);
			}
		}

		private void ConvertToTem()
		{
			if (_jsonPath == null) return;

			var temPath = TrainingConverter.GetTemPath(_jsonPath);
			if (File.Exists(temPath) && !_dialogService.Confirm($"{Path.GetFileName(temPath)} existe déjà. L'écraser ?")) return;

			try
			{
				AppendLog($"Conversion de {Path.GetFileName(_jsonPath)} en .Tem...");
				AppendLog($"Converti en .Tem : {_converter.JsonToTem(_jsonPath)}");
			}
			catch (Exception e)
			{
				AppendLog($"Échec de la conversion : {e.Message}");
			}
		}

		private void SetJsonPath(string path)
		{
			JsonPath = path;
			JsonFileName = Path.GetFileName(path);
		}

		private void AppendLog(string message)
		{
			_log.AppendLine(message);
			OnPropertyChanged(nameof(Log));
		}
	}
}
