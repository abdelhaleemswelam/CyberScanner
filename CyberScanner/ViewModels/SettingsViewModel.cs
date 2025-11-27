using System.Windows.Input;

namespace CyberScanner.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private int _defaultTimeout = 1000;
    private int _defaultMaxThreads = 50;
    private bool _autoSaveResults = true;
    private bool _playSounds = false;
    private string _theme = "Cyberpunk";
    private string _resultsFolder;

    public SettingsViewModel()
    {
        Title = "Settings";

        SaveCommand = new Command(ExecuteSave);
        ResetCommand = new Command(ExecuteReset);
        BrowseFolderCommand = new Command(async () => await ExecuteBrowseFolder());

        LoadSettings();
    }

    public int DefaultTimeout
    {
        get => _defaultTimeout;
        set => SetProperty(ref _defaultTimeout, value);
    }

    public int DefaultMaxThreads
    {
        get => _defaultMaxThreads;
        set => SetProperty(ref _defaultMaxThreads, value);
    }

    public bool AutoSaveResults
    {
        get => _autoSaveResults;
        set => SetProperty(ref _autoSaveResults, value);
    }

    public bool PlaySounds
    {
        get => _playSounds;
        set => SetProperty(ref _playSounds, value);
    }

    public string Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }

    public string ResultsFolder
    {
        get => _resultsFolder;
        set => SetProperty(ref _resultsFolder, value);
    }

    public List<string> Themes { get; } = new List<string>
    {
        "Cyberpunk",
        "Dark",
        "Light",
        "Blue"
    };

    public ICommand SaveCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand BrowseFolderCommand { get; }

    private void LoadSettings()
    {
        // Load from preferences
        DefaultTimeout = Preferences.Get("DefaultTimeout", 1000);
        DefaultMaxThreads = Preferences.Get("DefaultMaxThreads", 50);
        AutoSaveResults = Preferences.Get("AutoSaveResults", true);
        PlaySounds = Preferences.Get("PlaySounds", false);
        Theme = Preferences.Get("Theme", "Cyberpunk");
        ResultsFolder = Preferences.Get("ResultsFolder", FileSystem.CacheDirectory);
    }

    private void ExecuteSave()
    {
        // Save to preferences
        Preferences.Set("DefaultTimeout", DefaultTimeout);
        Preferences.Set("DefaultMaxThreads", DefaultMaxThreads);
        Preferences.Set("AutoSaveResults", AutoSaveResults);
        Preferences.Set("PlaySounds", PlaySounds);
        Preferences.Set("Theme", Theme);
        Preferences.Set("ResultsFolder", ResultsFolder);

        Application.Current.MainPage.DisplayAlert("Success", "Settings saved successfully!", "OK");
    }

    private void ExecuteReset()
    {
        var result = Application.Current.MainPage.DisplayAlert(
            "Reset Settings",
            "Are you sure you want to reset all settings to defaults?",
            "Yes", "No").Result;

        if (result)
        {
            Preferences.Clear();
            LoadSettings();
            Application.Current.MainPage.DisplayAlert("Success", "Settings reset to defaults!", "OK");
        }
    }

    private async Task ExecuteBrowseFolder()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select Results Folder"
            });

            if (result != null)
            {
                ResultsFolder = result.FullPath;
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to select folder: {ex.Message}", "OK");
        }
    }
}