using Keybinderr.Core.Models;

namespace Keybinderr.Core.Services;

public static class ProfileValidator
{
    public static IReadOnlyList<ValidationIssue> Validate(ProfileDocument document)
    {
        DefaultProfiles.EnsureDefaults(document);

        var issues = new List<ValidationIssue>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var profile in document.Profiles)
        {
            if (string.IsNullOrWhiteSpace(profile.Id))
            {
                issues.Add(ValidationIssue.Error(profile.Name, "Profile is missing an id."));
            }
            else if (!ids.Add(profile.Id))
            {
                issues.Add(ValidationIssue.Error(profile.Name, $"Duplicate profile id '{profile.Id}'."));
            }

            if (string.IsNullOrWhiteSpace(profile.Name))
            {
                issues.Add(ValidationIssue.Error(profile.Id, "Profile name is required."));
            }

            var sources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var mapping in profile.Mappings.Where(mapping => mapping.Enabled))
            {
                if (string.IsNullOrWhiteSpace(mapping.SourceKey) || string.IsNullOrWhiteSpace(mapping.TargetKey))
                {
                    issues.Add(ValidationIssue.Error(profile.Name, "Enabled mappings must include both a source and target key."));
                    continue;
                }

                if (!sources.Add(mapping.SourceKeyCanonical))
                {
                    issues.Add(ValidationIssue.Error(profile.Name, $"Duplicate enabled mapping for '{mapping.SourceKey}'."));
                }
            }
        }

        return issues;
    }
}

