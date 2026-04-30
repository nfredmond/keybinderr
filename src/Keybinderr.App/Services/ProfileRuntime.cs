using Keybinderr.Core.Models;
using Keybinderr.Core.Services;

namespace Keybinderr.App.Services;

public sealed class ProfileRuntime
{
    private readonly JsonProfileRepository _repository;

    public ProfileRuntime(JsonProfileRepository repository)
    {
        _repository = repository;
        Document = DefaultProfiles.CreateDocument();
    }

    public event EventHandler? ProfilesChanged;

    public ProfileDocument Document { get; private set; }

    public string ConfigPath => _repository.FilePath;

    public void Load()
    {
        Document = _repository.Load();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Save(ProfileDocument document)
    {
        var issues = ProfileValidator.Validate(document)
            .Where(issue => issue.Severity == ValidationSeverity.Error)
            .ToList();

        if (issues.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, issues.Select(issue => $"{issue.ProfileName}: {issue.Message}")));
        }

        _repository.Save(document);
        Document = document;
        ProfilesChanged?.Invoke(this, EventArgs.Empty);
    }

    public KeyboardProfile GetNormalProfile()
    {
        DefaultProfiles.EnsureDefaults(Document);
        return Document.Profiles.First(profile => profile.Id == DefaultProfiles.NormalProfileId);
    }
}

