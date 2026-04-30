namespace Keybinderr.Core.Services;

public sealed record ValidationIssue(string ProfileName, string Message, ValidationSeverity Severity)
{
    public static ValidationIssue Error(string profileName, string message)
    {
        return new ValidationIssue(profileName, message, ValidationSeverity.Error);
    }
}

public enum ValidationSeverity
{
    Error
}
