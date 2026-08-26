using System;
using System.Security.Cryptography;

namespace BLL
{
    public static class PasswordHasher
    {
        public const int Iterations = 100000;
        private const int SaltSizeBytes = 16;
        private const int KeySizeBytes = 32;
        private const string Prefix = "pbkdf2-sha256";

        public static string Hash(string password)
        {
            byte[] salt = new byte[SaltSizeBytes];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(salt);
            return Encode(password, salt, Iterations);
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored))
                return false;

            string[] parts = stored.Split(':');
            if (parts.Length != 4 || parts[0] != Prefix)
                return false;

            int iterations;
            if (!int.TryParse(parts[1], out iterations) || iterations <= 0)
                return false;

            byte[] salt;
            try { salt = Convert.FromBase64String(parts[2]); }
            catch (FormatException) { return false; }

            string candidate = Encode(password ?? string.Empty, salt, iterations);
            return SlowEquals(candidate, stored);
        }

        private static string Encode(string password, byte[] salt, int iterations)
        {
            using (var derive = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] key = derive.GetBytes(KeySizeBytes);
                return Prefix + ":" + iterations + ":"
                     + Convert.ToBase64String(salt) + ":"
                     + Convert.ToBase64String(key);
            }
        }

        private static bool SlowEquals(string left, string right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null || left.Length != right.Length) return false;
            int diff = 0;
            for (int i = 0; i < left.Length; i++)
                diff |= left[i] ^ right[i];
            return diff == 0;
        }
    }
}
