using FamFit.Domain.Enums;

namespace FamFit.Domain.Entities;

/// <summary>
/// Пользователь семейного пространства.
/// </summary>
public sealed class User
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public Role Role { get; private set; } = Role.Member;
    public int StreakCount { get; private set; } = 0;
    public bool IsBanned { get; private set; } = false;

    // EF Core parameterless constructor (internal для инкапсуляции)
    private User() { }

    public static User Create(string name, string email, string passwordHash, Role role = Role.Member)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        return new User
        {
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role
        };
    }

    public void PromoteToAdmin()
    {
        Role = Role.Admin;
    }

    public void DemoteToMember()
    {
        if (Role == Role.Admin)
            throw new InvalidOperationException("Cannot demote an admin without a replacement.");

        Role = Role.Member;
    }

    public void IncrementStreak()
    {
        StreakCount += 1;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
    }

    public void Ban()
    {
        IsBanned = true;
    }

    public void Unban()
    {
        IsBanned = false;
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be empty.", nameof(newName));

        Name = newName.Trim();
    }

    public void UpdateEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new ArgumentException("Email cannot be empty.", nameof(newEmail));

        Email = newEmail.Trim().ToLowerInvariant();
    }
}
