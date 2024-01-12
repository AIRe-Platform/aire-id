using System.Runtime.Serialization;
using System.Security.Cryptography;
using Aire.Sdk.Azure;
using Aire.Sdk.Helpers;
using Aire.Services.Models;

namespace Aire.Id.Models
{
    [EntityTable("Users")]
    public class UserEntity : BaseTableEntity
    {
        [IgnoreDataMember]
        public string? UUID {
            get => RowKey;
            set { PartitionKey = value; RowKey = value; }
        }

        public string? EmailHash { get; set; }
        public string? Role { get; set; }
        public string? Scopes { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? EulaAccepted { get; set; }
        bool Verified { get; set; }

        // Password properties
        public string? PasswordSalt { get; set; }
        public string? PasswordHash { get; set; }
        public int PasswordIter { get; set; }

        // Encrypted data
        public string? DataIV { get; set; }
        public string? DataBlock { get; set; }
        public string? Encryption { get; set; }

        /// <summary>
        /// Serializes and encrypts the user data
        /// </summary>
        /// <param name="data">Private user data</param>
        /// <param name="encryptionKey">Encryption key</param>
        public void SetPrivateUserData(UserPrivate data, string encryptionKey)
        {
            byte[] key = Convert.FromBase64String(encryptionKey);
            byte[] iv = RandomNumberGenerator.GetBytes(16);
            DataBlock = data.ObjectToJson().EncryptString(key, iv);
            DataIV = Convert.ToBase64String(iv);
            EmailHash = Crypto.SHA256Base16(data.Email!);
        }

        /// <summary>
        /// Decrypts and deserializes the private user data
        /// </summary>
        /// <param name="encryptionKey">Encryption key</param>
        /// <returns>Private user model</returns>
        public UserPrivate? GetPrivateUserData(string encryptionKey)
        {
            if(string.IsNullOrEmpty(encryptionKey))
                return null;
                
            byte[] key = Convert.FromBase64String(encryptionKey);
            byte[] iv = Convert.FromBase64String(DataIV!);
            return DataBlock?.DecryptString(key, iv)?.JsonToObject<User>();
        }

        /// <summary>
        /// Returns both public and private user data
        /// </summary>
        /// <param name="encryptionKey">Encryption key</param>
        /// <returns>User model</returns>
        public User GetUserData(string encryptionKey)
        {
            User? user = (User?) GetPrivateUserData(encryptionKey);
            user ??= new User();
            user.UUID = UUID;
            user.LastLogin = LastLogin;
            user.EulaAccepted = EulaAccepted;
            user.Verified = Verified;
            return user;
        }

        /// <summary>
        /// Check that given password matches the password hash
        /// </summary>
        /// <param name="password">Password</param>
        /// <returns>True if the password is correct. Otherwise, returns false.</returns>
        public bool CheckPassword(string password)
        {
            var hash = Crypto.PasswordHash(password, PasswordSalt, 32, PasswordIter);
            var hash64 = Convert.ToBase64String(hash);
            return hash64 == PasswordHash;
        }

        /// <summary>
        /// Changes the user password and re-encrypts the data.
        /// This effectively revokes the access of any existing access tokens.
        /// Apps should re-login the user to get the updated encryption key.
        /// </summary>
        /// <param name="oldPassword">Old password</param>
        /// <param name="newPassword">New password</param>
        /// <returns>True if the operation was successful. Otherwise, false.</returns>
        public bool ChangePassword(string? oldPassword, string newPassword)
        {
            if(!string.IsNullOrEmpty(PasswordHash))
            {
                // Verify old password
                if(oldPassword == null)
                    return false;

                var oldHash = Crypto.PasswordHash(oldPassword, PasswordSalt, 32, PasswordIter);
                var oldHash64 = Convert.ToBase64String(oldHash);
                if(oldHash64 != PasswordHash)
                    return false;

                // Re-encrypt data key
                var key = GetEncryptionKey(oldPassword);
                var keyBytes = Convert.FromBase64String(key!);
                SetEncryptionKey(UUID!, newPassword, keyBytes);
            }

            PasswordIter = 100000;

            var salt = RandomNumberGenerator.GetBytes(32);
            PasswordSalt = Convert.ToBase64String(salt);

            var hash = Crypto.PasswordHash(newPassword, PasswordSalt, 32, PasswordIter);
            PasswordHash = Convert.ToBase64String(hash);

            return true;
        }

        /// <summary>
        /// Sets a random user data encryption key
        /// which is encrypted using user credentials
        /// </summary>
        /// <param name="uuid">User identifier</param>
        /// <param name="password">User password</param>
        public void GenerateEncryptionKey(string uuid, string password)
        {
            if(!string.IsNullOrWhiteSpace(Encryption))
                throw new InvalidOperationException("Encryption key is already set");

            var encKeyBytes = RandomNumberGenerator.GetBytes(32);
            SetEncryptionKey(uuid, password, encKeyBytes);
        }

        /// <summary>
        /// Reads user data encryption key and decrypts it
        /// </summary>
        /// <param name="password">User password</param>
        /// <returns>Base64 encoded data encryption key</returns>
        public string? GetEncryptionKey(string password)
        {
            var key = Crypto.PasswordHash(UUID + password, null, 32, 100000);
            var parts = Encryption!.Split(".");

            if(parts.Length != 2)
                return null;

            var data = parts[0]; var iv = parts[1];
            return data.DecryptString(key, Convert.FromBase64String(iv));
        }

        /// <summary>
        /// Sets user data encryption key, encrypting it with user credentials
        /// </summary>
        /// <param name="uuid">User identifier</param>
        /// <param name="password">User password</param>
        /// <param name="encKey">256-bit key in base64</param>
        public void SetEncryptionKey(string uuid, string password, byte[] encKeyBytes)
        {
            if(encKeyBytes.Length != 32)
                throw new ArgumentException("Encryption key has to be 256 bits in length", nameof(encKeyBytes));
            var key = Crypto.PasswordHash(uuid + password, null, 32, 100000);
            var iv = RandomNumberGenerator.GetBytes(16);
            var encKey = Convert.ToBase64String(encKeyBytes);
            Encryption = $"{encKey.EncryptString(key, iv)}.{Convert.ToBase64String(iv)}";
        }
    }
}