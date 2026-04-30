using Keybinderr.Core.Models;

namespace Keybinderr.App.Services;

public sealed class ActiveProfileChangedEventArgs : EventArgs
{
    public ActiveProfileChangedEventArgs(KeyboardProfile activeProfile, ForegroundWindowSnapshot? foregroundWindow)
    {
        ActiveProfile = activeProfile;
        ForegroundWindow = foregroundWindow;
    }

    public KeyboardProfile ActiveProfile { get; }

    public ForegroundWindowSnapshot? ForegroundWindow { get; }
}

