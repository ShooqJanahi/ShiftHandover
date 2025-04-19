using System.Security.Cryptography;
using System.Text;

// A static helper class responsible for password hashing operations
public static class PasswordHelper
{
    // Method to hash a plain text password using SHA-256 encryption
    public static string Hash(string password)
    {
        // Create a new instance of SHA256 encryption algorithm
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(password); // Convert the password string into a byte array (UTF-8 encoding)
            var hash = sha256.ComputeHash(bytes);  // Compute the hash (returns a fixed 256-bit/32-byte array)
            return Convert.ToBase64String(hash); // Convert the byte array into a Base64-encoded string (safe for storage)
        }
    }
}

