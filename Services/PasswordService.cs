namespace CvParsing.Services;

/// <summary>BCrypt for new passwords; supports legacy plaintext stored in DB until migrated.</summary>
public class PasswordService
{
    private const int WorkFactor = 11;

    public static bool LooksLikeBcrypt(string? stored) =>
        !string.IsNullOrEmpty(stored) && stored.StartsWith("$2", StringComparison.Ordinal);

    public string Hash(string plainPassword) =>
        BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: WorkFactor);

    public bool Verify(string? storedValue, string plainPassword)
    {
        if (string.IsNullOrEmpty(storedValue))
            return false;

        if (LooksLikeBcrypt(storedValue))
            return BCrypt.Net.BCrypt.Verify(plainPassword, storedValue);

        return storedValue == plainPassword;
    }
}
