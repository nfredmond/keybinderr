using System.IO;
using System.Windows;
using Keybinderr.App.Services;
using Keybinderr.Core.Services;

namespace Keybinderr.App;

public sealed class App : System.Windows.Application
{
    private readonly string[] _args;
    private Mutex? _singleInstanceMutex;
    private SettingsWindow? _settingsWindow;
    private ProfileRuntime? _profileRuntime;
    private ActiveProfileService? _activeProfileService;
    private KeyboardHookService? _keyboardHookService;
    private TrayIconService? _trayIconService;

    public App(string[] args)
    {
        _args = args;
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstanceMutex = new Mutex(initiallyOwned: true, "Keybinderr.SingleInstance", out var ownsMutex);
        if (!ownsMutex)
        {
            Shutdown();
            return;
        }

        var configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Keybinderr",
            "config.json");
        var firstRun = !File.Exists(configPath);

        _profileRuntime = new ProfileRuntime(new JsonProfileRepository(configPath));
        _profileRuntime.Load();

        _activeProfileService = new ActiveProfileService(_profileRuntime);
        _keyboardHookService = new KeyboardHookService(_activeProfileService);
        _trayIconService = new TrayIconService(
            _profileRuntime,
            _activeProfileService,
            _keyboardHookService,
            ShowSettingsWindow,
            Shutdown);

        _keyboardHookService.Start();
        _activeProfileService.Start();
        _trayIconService.Start();

        if (firstRun || _args.Any(arg => string.Equals(arg, "--settings", StringComparison.OrdinalIgnoreCase)))
        {
            ShowSettingsWindow();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIconService?.Dispose();
        _keyboardHookService?.Dispose();
        _activeProfileService?.Dispose();
        _singleInstanceMutex?.Dispose();

        base.OnExit(e);
    }

    private void ShowSettingsWindow()
    {
        if (_profileRuntime is null)
        {
            return;
        }

        if (_settingsWindow is { IsVisible: true })
        {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow(_profileRuntime);
        _settingsWindow.Closed += (_, _) => _settingsWindow = null;
        _settingsWindow.Show();
    }
}
