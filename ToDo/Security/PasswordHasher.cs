using System.Security.Cryptography;

namespace ToDo.API.Security;

public static class PasswordHasher
{
    public static string Hash(string password)
    {
        // format: iterations.salt.hash
        const int iterations = 100_000;
        byte[] salt = RandomNumberGenerator.GetBytes(16);

        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(32);

        return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string stored)
    {
        var parts = stored.Split('.');
        if (parts.Length != 3) return false;

        int iterations = int.Parse(parts[0]);
        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] expected = Convert.FromBase64String(parts[2]);

        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        byte[] actual = pbkdf2.GetBytes(32);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    internal static bool Verify(string password, byte[] passwordHash)
    {
        throw new NotImplementedException();
    }
}
