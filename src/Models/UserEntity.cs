using System;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using Aire.Helpers;
using Aire.Services.Models;

namespace Aire.Id.Models
{
    [EntityTable("Users")]
    public class UserEntity : BaseTableEntity
    {
        [IgnoreDataMember]
        public string UUID {
            get => RowKey;
            set { PartitionKey = value; RowKey = value; }
        }

        public string EmailHash { get; set; }
        public string Role { get; set; }
        public string Scopes { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? EulaAccepted { get; set; }
        bool Verified { get; set; }

        // Password properties
        public string PasswordSalt { get; set; }
        public string PasswordHash { get; set; }
        public int PasswordIter { get; set; }

        // Encrypted data
        public string DataIV { get; set; }
        public string DataBlock { get; set; }

        /// <summary>
        /// Serializes and encrypts the user data
        /// </summary>
        /// <param name="data">Private user data</param>
        /// <param name="encryptionKey">Encryption key</param>
        public void SetUserData(UserPrivate data, string encryptionKey)
        {
            byte[] key = Convert.FromBase64String(encryptionKey);
            byte[] iv = RandomNumberGenerator.GetBytes(16);
            DataBlock = data.ObjectToJson().EncryptString(key, iv);
            DataIV = Convert.ToBase64String(iv);
            EmailHash = Crypto.SHA256Base16(data.Email);
        }

        /// <summary>
        /// Decrypts and deserializes the private user data
        /// </summary>
        /// <param name="encryptionKey">Encryption key</param>
        /// <returns>Private user model</returns>
        public UserPrivate GetPrivateUserData(string encryptionKey)
        {
            byte[] key = Convert.FromBase64String(encryptionKey);
            byte[] iv = Convert.FromBase64String(DataIV);
            return DataBlock.DecryptString(key, iv).JsonToObject<User>();
        }

        /// <summary>
        /// Returns both public and private user data
        /// </summary>
        /// <param name="encryptionKey">Encryption key</param>
        /// <returns>User model</returns>
        public User GetUserData(string encryptionKey)
        {
            User user = (User) GetPrivateUserData(encryptionKey);
            user.LastLogin = LastLogin;
            user.EulaAccepted = EulaAccepted;
            user.Verified = Verified;
            return user;
        }

        /// <summary>
        /// Changes the user password and re-encrypts the data.
        /// This effectively revokes the access of any existing access tokens.
        /// Apps should re-login the user to get the updated encryption key.
        /// </summary>
        /// <param name="oldPassword">Old password</param>
        /// <param name="newPassword">New password</param>
        /// <returns>True if the operation was successful. Otherwise, false.</returns>
        public bool ChangePassword(string oldPassword, string newPassword)
        {
            if(!string.IsNullOrEmpty(PasswordHash))
            {
                // Verify old password
                var oldHash = Crypto.PasswordHash(oldPassword, PasswordSalt, 32, PasswordIter);
                var oldHash64 = Convert.ToBase64String(oldHash);
                if(oldHash64 != PasswordHash)
                    return false;

                // Re-encrypt data
                var oldKey = Crypto.DeriveUserEncryptionKey(UUID, oldPassword);
                var data = GetUserData(oldKey);
                if(data == null)
                    return false;
                var newKey = Crypto.DeriveUserEncryptionKey(UUID, newPassword);
                SetUserData(data, newKey);
            }

            PasswordIter = 100000;

            var salt = RandomNumberGenerator.GetBytes(32);
            PasswordSalt = Convert.ToBase64String(salt);

            var hash = Crypto.PasswordHash(newPassword, PasswordSalt, 32, PasswordIter);
            PasswordHash = Convert.ToBase64String(hash);

            return true;
        }
    }
}