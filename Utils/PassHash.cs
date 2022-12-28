using System.Security.Cryptography;
using System.Text;

namespace WebAppAuthenticationServer.Utils;


public static class PassHash
{
    private const int keySize = 64;
    private const int iterations = 350000;
    private static HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
    public static string HashPassword(string password, out string salt)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(keySize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            iterations,
            hashAlgorithm,
            keySize);

        var hashString = Convert.ToBase64String(hash);
        salt = Convert.ToBase64String(saltBytes);

        return "HASH$" + hashString;
    }

    private static bool IsHashSupported(string hashString)
    {
        return hashString.Contains("HASH$");
    }

    public static bool VerifyPassword(string password, string hash, string salt)
    {
        if (!IsHashSupported(hash))
        {
            throw new NotSupportedException("There was an error verifying the password.");
        }
        else
        {
            hash = hash.Replace("HASH$", "");
        }

        var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password),
         Convert.FromBase64String(salt), iterations, hashAlgorithm, keySize);

        return hashToCompare.SequenceEqual(Convert.FromBase64String(hash));
    }
}
