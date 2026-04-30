using Keybinderr.Core.Models;

namespace Keybinderr.Core.Services;

public static class ProfileMatcher
{
    public static KeyboardProfile ResolveActiveProfile(
        ProfileDocument document,
        ForegroundWindowSnapshot? foregroundWindow)
    {
        DefaultProfiles.EnsureDefaults(document);

        var normalProfile = document.Profiles.First(profile => profile.Id == DefaultProfiles.NormalProfileId);
        if (document.Settings.RemappingPaused || foregroundWindow is null)
        {
            return normalProfile;
        }

        return document.Profiles
            .Where(profile => profile.Enabled)
            .Where(profile => profile.Id != DefaultProfiles.NormalProfileId)
            .Where(profile => !string.IsNullOrWhiteSpace(profile.ExecutablePath))
            .FirstOrDefault(profile => Matches(profile, foregroundWindow))
            ?? normalProfile;
    }

    public static IReadOnlyDictionary<string, string> BuildMappingDictionary(KeyboardProfile profile)
    {
        return profile.Mappings
            .Where(mapping => mapping.Enabled)
            .Where(mapping => !string.IsNullOrWhiteSpace(mapping.SourceKey))
            .Where(mapping => !string.IsNullOrWhiteSpace(mapping.TargetKey))
            .GroupBy(mapping => mapping.SourceKeyCanonical)
            .ToDictionary(group => group.Key, group => group.Last().TargetKeyCanonical);
    }

    private static bool Matches(KeyboardProfile profile, ForegroundWindowSnapshot foregroundWindow)
    {
        return MatchesExecutable(profile.ExecutablePath, foregroundWindow)
            && MatchesWindowTitle(profile.WindowTitleMatch, foregroundWindow.WindowTitle);
    }

    private static bool MatchesExecutable(string? configuredExecutable, ForegroundWindowSnapshot foregroundWindow)
    {
        if (string.IsNullOrWhiteSpace(configuredExecutable))
        {
            return false;
        }

        if (LooksLikePath(configuredExecutable) && !string.IsNullOrWhiteSpace(foregroundWindow.ProcessPath))
        {
            return string.Equals(
                NormalizeExecutablePath(configuredExecutable),
                NormalizeExecutablePath(foregroundWindow.ProcessPath),
                StringComparison.OrdinalIgnoreCase);
        }

        var configuredFileName = GetExecutableFileName(configuredExecutable);
        if (!string.IsNullOrWhiteSpace(configuredFileName))
        {
            if (string.Equals(configuredFileName, GetExecutableFileName(foregroundWindow.ProcessPath), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(Path.GetFileNameWithoutExtension(configuredFileName), foregroundWindow.ProcessName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool LooksLikePath(string value)
    {
        return value.IndexOf('\\') >= 0
            || value.IndexOf('/') >= 0;
    }

    private static string NormalizeExecutablePath(string value)
    {
        return value.Trim().Trim('"').Replace('/', '\\').TrimEnd('\\');
    }

    private static string? GetExecutableFileName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim().Trim('"');
        var separatorIndex = trimmed.LastIndexOfAny(new[] { '\\', '/' });
        return separatorIndex >= 0 ? trimmed[(separatorIndex + 1)..] : trimmed;
    }

    private static bool MatchesWindowTitle(string? configuredTitleMatch, string? actualTitle)
    {
        return string.IsNullOrWhiteSpace(configuredTitleMatch)
            || (!string.IsNullOrWhiteSpace(actualTitle)
                && actualTitle.Contains(configuredTitleMatch, StringComparison.OrdinalIgnoreCase));
    }
}
