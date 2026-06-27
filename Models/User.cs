using System;

namespace GymFinanceBillingWpf.Models;

public enum UserRole
{
    Admin,
    Receptionist
}

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // SHA256 hash
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Receptionist;
    public bool IsActive { get; set; } = true;

    /// <summary>Returns the two-letter initials for the avatar badge.</summary>
    public string Initials
    {
        get
        {
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            if (parts.Length == 1)
                return parts[0][..Math.Min(2, parts[0].Length)].ToUpper();
            return "??";
        }
    }
}
