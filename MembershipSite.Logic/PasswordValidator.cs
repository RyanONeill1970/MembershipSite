namespace MembershipSite.Logic;

public static class PasswordValidator
{
    private static TimeSpan ResetPasswordExpiryTime = TimeSpan.FromDays(7);

    public const int MinimumPasswordLength = 6;

    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public static bool ComparePassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            // Password could not be decrypted as it did not start with the standard blowflish $2[a/x/y]$ prefix.
            return false;
        }
    }

    public static bool PasswordIsSecure(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }
        if (password.Trim().Length < MinimumPasswordLength)
        {
            return false;
        }
        return true;
    }

    public static bool HasPasswordResetWindowExpired(DateTimeOffset currentTime, DateTimeOffset requestedDate)
    {
        if (requestedDate.Add(ResetPasswordExpiryTime) <= currentTime)
        {
            return true;
        }
        return false;
    }
}
