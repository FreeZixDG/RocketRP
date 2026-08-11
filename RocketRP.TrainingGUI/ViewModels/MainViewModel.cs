using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using RocketRP.TrainingGUI.Models;
using RocketRP.TrainingGUI.Services;

namespace RocketRP.TrainingGUI.ViewModels
{
	public class MainViewModel : ObservableObject
	{
		private readonly TrainingConverter _converter;
		private readonly IFileDialogService _dialogService;
		private readonly StringBuilder _log = new();

		private TrainingPackDocument? _document;
		private RoundViewModel? _selectedRound;
		private IReadOnlyList<string> _availableMaps = MapCatalog.KnownMaps;
		private string? _jsonPath;
		private string _jsonFileName = string.Empty;
		private bool _isModified;
		private bool _isMapUnlocked;

		public MainViewModel(TrainingConverter converter, IFileDialogService dialogService)
		{
			_converter = converter;
			_dialogService = dialogService;

			ImportCommand = new RelayCommand(Import);
			RenameCommand = new RelayCommand(Rename, () => HasJson);
			SaveCommand = new RelayCommand(Save, () => IsModified);
			ConvertToTemCommand = new RelayCommand(ConvertToTem, () => HasJson);
		}

		public ICommand ImportCommand { get; }
		public ICommand RenameCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand ConvertToTemCommand { get; }

		public ObservableCollection<RoundViewModel> Rounds { get; } = [];

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

		public RoundViewModel? SelectedRound
		{
			get => _selectedRound;
			set => SetProperty(ref _selectedRound, value);
		}

		/// <summary>True when the loaded pack has edits that are not written to the JSON file yet.</summary>
		public bool IsModified
		{
			get => _isModified;
			private set => SetProperty(ref _isModified, value);
		}

		/// <summary>Title of the pack as shown in game.</summary>
		public string PackName
		{
			get => _document?.Name ?? string.Empty;
			set
			{
				if (_document == null || _document.Name == value) return;

				_document.Name = value;
				IsModified = true;
				OnPropertyChanged();
			}
		}

		/// <summary>Map of the pack. Only settable once <see cref="IsMapUnlocked"/> has been confirmed.</summary>
		public string PackMap
		{
			get => _document?.MapName ?? string.Empty;
			set
			{
				if (_document == null || !IsMapUnlocked || string.IsNullOrEmpty(value) || _document.MapName == value) return;

				try
				{
					_document.MapName = value;
					IsModified = true;
					AppendLog($"Map changée en {value}. Les positions des niveaux n'ont pas été adaptées.");
				}
				catch (ArgumentException e)
				{
					AppendLog($"Map refusée : {e.Message}");
				}

				OnPropertyChanged();
			}
		}

		/// <summary>Maps offered in the picker: the known ones plus the current map if it isn't listed.</summary>
		public IReadOnlyList<string> AvailableMaps
		{
			get => _availableMaps;
			private set => SetProperty(ref _availableMaps, value);
		}

		/// <summary>
		/// Locked by default: changing the map can crash the game and the round positions stay as they are.
		/// Unlocking asks for an explicit confirmation.
		/// </summary>
		public bool IsMapUnlocked
		{
			get => _isMapUnlocked;
			set
			{
				_isMapUnlocked = value && _document != null
					&& _dialogService.Confirm("Changer la map peut faire planter le jeu et les positions des niveaux ne seront pas adaptées à la nouvelle map.\n\nDéverrouiller quand même ?");

				// Always notify: the checkbox has to go back down when the confirmation is refused.
				OnPropertyChanged();
			}
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
					LoadJson(_converter.TemToJson(path));
					AppendLog($"Converti en JSON : {JsonPath}");
				}
				else
				{
					LoadJson(path);
					AppendLog($"JSON chargé : {JsonPath}");
				}

				AppendLog($"« {PackName} » sur {PackMap} : {Rounds.Count} niveaux.");
			}
			catch (Exception e)
			{
				Unload();
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
				JsonPath = newPath;
				JsonFileName = Path.GetFileName(newPath);
				AppendLog($"Renommé en {newName}");
			}
			catch (Exception e)
			{
				AppendLog($"Échec du renommage : {e.Message}");
				JsonFileName = Path.GetFileName(_jsonPath);
			}
		}

		private void Save()
		{
			if (_document == null || _jsonPath == null) return;

			try
			{
				_document.Save(_jsonPath);
				IsModified = false;
				AppendLog($"JSON enregistré : {_jsonPath}");
			}
			catch (Exception e)
			{
				AppendLog($"Échec de l'enregistrement : {e.Message}");
			}
		}

		private void ConvertToTem()
		{
			if (_jsonPath == null) return;

			var temPath = TrainingConverter.GetTemPath(_jsonPath);
			if (File.Exists(temPath) && !_dialogService.Confirm($"{Path.GetFileName(temPath)} existe déjà. L'écraser ?")) return;

			if (IsModified) Save();
			if (IsModified) return;

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

		private void LoadJson(string path)
		{
			Unload();

			var document = TrainingPackDocument.Load(path);
			foreach (var round in document.Rounds)
			{
				var roundViewModel = new RoundViewModel(round);
				roundViewModel.PropertyChanged += OnRoundChanged;
				Rounds.Add(roundViewModel);
			}

			_document = document;
			JsonPath = path;
			JsonFileName = Path.GetFileName(path);
			SelectedRound = Rounds.FirstOrDefault();
			AvailableMaps = MapCatalog.WithCurrentMap(document.MapName);
			OnPropertyChanged(nameof(PackName));
			OnPropertyChanged(nameof(PackMap));
		}

		private void Unload()
		{
			foreach (var round in Rounds) round.PropertyChanged -= OnRoundChanged;
			Rounds.Clear();

			_document = null;
			SelectedRound = null;
			JsonPath = null;
			JsonFileName = string.Empty;
			IsModified = false;
			AvailableMaps = MapCatalog.KnownMaps;

			// Re-lock the map without asking anything, the property setter is for the user only.
			_isMapUnlocked = false;
			OnPropertyChanged(nameof(IsMapUnlocked));
			OnPropertyChanged(nameof(PackName));
			OnPropertyChanged(nameof(PackMap));
		}

		private void OnRoundChanged(object? sender, PropertyChangedEventArgs e) => IsModified = true;

		private void AppendLog(string message)
		{
			_log.AppendLine(message);
			OnPropertyChanged(nameof(Log));
		}
	}
}
