using System;
using System.Security.Cryptography;
using System.Text;

namespace InstantMessagingServerApp.Services
{
    /// <summary>
    /// Encrypts a string using the SHA256 algorithm.
    /// </summary>
    public static class PasswordEncoder
    {
        public static string Encode(string passStr)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(passStr));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public static bool Match(string passStr, string passHash)
        {
            var encodedPassStr = Encode(passStr);
            return encodedPassStr.Equals(passHash);
        }

    }
}