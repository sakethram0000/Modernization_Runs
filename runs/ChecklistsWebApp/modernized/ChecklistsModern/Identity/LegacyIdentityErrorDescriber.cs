using Microsoft.AspNetCore.Identity;

namespace ChecklistsModern.Identity;

// Texts from the ASP.NET Identity 2.2.1 resources used by the legacy app.
public class LegacyIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateUserName(string userName) =>
        Error(nameof(DuplicateUserName), $"Name {userName} is already taken.");

    public override IdentityError DuplicateEmail(string email) =>
        Error(nameof(DuplicateEmail), $"Email '{email}' is already taken.");

    public override IdentityError InvalidUserName(string? userName) =>
        Error(nameof(InvalidUserName), $"User name {userName} is invalid, can only contain letters or digits.");

    public override IdentityError InvalidEmail(string? email) =>
        Error(nameof(InvalidEmail), $"Email '{email}' is invalid.");

    public override IdentityError InvalidToken() =>
        Error(nameof(InvalidToken), "Invalid token.");

    public override IdentityError PasswordMismatch() =>
        Error(nameof(PasswordMismatch), "Incorrect password.");

    public override IdentityError UserAlreadyHasPassword() =>
        Error(nameof(UserAlreadyHasPassword), "User already has a password set.");

    public override IdentityError LoginAlreadyAssociated() =>
        Error(nameof(LoginAlreadyAssociated), "A user with that external login already exists.");

    public override IdentityError UserLockoutNotEnabled() =>
        Error(nameof(UserLockoutNotEnabled), "Lockout is not enabled for this user.");

    public override IdentityError PasswordTooShort(int length) =>
        Error(nameof(PasswordTooShort), $"Passwords must be at least {length} characters.");

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        Error(nameof(PasswordRequiresNonAlphanumeric), "Passwords must have at least one non letter or digit character.");

    public override IdentityError PasswordRequiresDigit() =>
        Error(nameof(PasswordRequiresDigit), "Passwords must have at least one digit ('0'-'9').");

    public override IdentityError PasswordRequiresLower() =>
        Error(nameof(PasswordRequiresLower), "Passwords must have at least one lowercase ('a'-'z').");

    public override IdentityError PasswordRequiresUpper() =>
        Error(nameof(PasswordRequiresUpper), "Passwords must have at least one uppercase ('A'-'Z').");

    private static IdentityError Error(string code, string description) =>
        new() { Code = code, Description = description };
}
