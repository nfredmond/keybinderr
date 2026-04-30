using System.Windows.Threading;
using Keybinderr.Core.Models;
using Keybinderr.Core.Services;

namespace Keybinderr.App.Services;

public sealed class ActiveProfileService : IDisposable
{
    private readonly ProfileRuntime _profileRuntime;
    private readonly DispatcherTimer _timer;

    public ActiveProfileService(ProfileRuntime profileRuntime)
    {
        _profileRuntime = profileRuntime;
        ActiveProfile = _profileRuntime.GetNormalProfile();
        ActiveMappings = ProfileMatcher.BuildMappingDictionary(ActiveProfile);

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _timer.Tick += (_, _) => Refresh();
        _profileRuntime.ProfilesChanged += OnProfilesChanged;
    }

    public event EventHandler<ActiveProfileChangedEventArgs>? ActiveProfileChanged;

    public KeyboardProfile ActiveProfile { get; private set; }

    public ForegroundWindowSnapshot? ForegroundWindow { get; private set; }

    public IReadOnlyDictionary<string, string> ActiveMappings { get; private set; }

    public void Start()
    {
        Refresh(force: true);
        _timer.Start();
    }

    public void Dispose()
    {
        _timer.Stop();
        _profileRuntime.ProfilesChanged -= OnProfilesChanged;
    }

    private void OnProfilesChanged(object? sender, EventArgs e)
    {
        Refresh(force: true);
    }

    private void Refresh(bool force = false)
    {
        ForegroundWindow = WindowsForegroundReader.ReadForegroundWindow();
        var activeProfile = ProfileMatcher.ResolveActiveProfile(_profileRuntime.Document, ForegroundWindow);

        if (!force && activeProfile.Id == ActiveProfile.Id)
        {
            return;
        }

        ActiveProfile = activeProfile;
        ActiveMappings = ProfileMatcher.BuildMappingDictionary(activeProfile);
        ActiveProfileChanged?.Invoke(this, new ActiveProfileChangedEventArgs(activeProfile, ForegroundWindow));
    }
}

