using System;
using System.Security.Cryptography;
using System.Text;

namespace Aire.Helpers
{
    public static class Crypto
    {
        /// <summary>
        /// Encrypts the given plain text using AES-256
        /// </summary>
        /// <param name="plainText">Text to be encrypted</param>
        /// <param name="key">256-bit key</param>
        /// <param name="iv">Initialization vector</param>
        /// <returns>Tuple of Base64 encoded string containing encrypted data</returns>
        public static string EncryptString(this string plainText, byte[] key, byte[] iv)
        {
            if(key.Length != 32)
                throw new ArgumentException($"Invalid key length, expected 32 bytes (256-bit), got {key.Length}");
                
            byte[] plainData = Encoding.UTF8.GetBytes(plainText);

            using var aes = Aes.Create();
            var encryptor = aes.CreateEncryptor(key, iv);

            byte[] cipherData = encryptor.TransformFinalBlock(plainData, 0, plainData.Length);
            return Convert.ToBase64String(cipherData);
        }

        /// <summary>
        /// Decrypts the given cipher text using AES-256
        /// </summary>
        /// <param name="cipherText">Base64 encoded encrypted data</param>
        /// <param name="key">256-bit key</param>
        /// <param name="iv">Initialization vector</param>
        /// <returns>String presentation of decrypted data</returns>
        public static string DecryptString(this string cipherText, byte[] key, byte[] iv)
        {
            if(key.Length != 32)
                throw new ArgumentException($"Invalid key length, expected 32 bytes (256-bit), got {key.Length}");

            byte[] cipherData = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            var decryptor = aes.CreateDecryptor(key, iv);
            try
            {
                byte[] plainData = decryptor.TransformFinalBlock(cipherData, 0, cipherData.Length);
                return Encoding.UTF8.GetString(plainData);
            }
            catch(Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// RFC2898 (PBKDF2) password hash algorithm
        /// </summary>
        /// <param name="password">Password</param>
        /// <param name="salt">Optional salt</param>
        /// <param name="len">Key length in bytes</param>
        /// <param name="iter">Number of iterations</param>
        /// <returns>Derived key bytes</returns>
        public static byte[] PasswordHash(string password, string salt = null, int len = 32, int iter = 100000)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var saltBytes = Array.Empty<byte>();
            if(salt != null)
                saltBytes = Encoding.UTF8.GetBytes(salt);
            return Rfc2898DeriveBytes.Pbkdf2(passwordBytes, saltBytes, iter, HashAlgorithmName.SHA512, len);
        }

        /// <summary>
        /// Convert input string to UTF-8 encoded bytes, hash using SHA256, encode using Base64
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>SHA256 hash as base64</returns>
        public static string SHA256Base64(string input)
		{
			byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(inputBytes));
        }

        /// <summary>
        /// Convert input string to UTF-8 encoded bytes, hash using SHA256, encode using Base16
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>SHA256 hash as base16</returns>
        public static string SHA256Base16(string input)
		{
			byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(inputBytes));
        }
    }
}
