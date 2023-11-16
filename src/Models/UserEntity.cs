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

        public void SetUserData(User data, string encryptionKey)
        {
            byte[] key = Convert.FromBase64String(encryptionKey);
            byte[] iv = RandomNumberGenerator.GetBytes(16);
            DataBlock = data.ObjectToJson().EncryptString(key, iv);
            DataIV = Convert.ToBase64String(iv);
            EmailHash = Crypto.SHA256Base16(data.Email);
        }

        public User GetUserData(string encryptionKey)
        {
            byte[] key = Convert.FromBase64String(encryptionKey);
            byte[] iv = Convert.FromBase64String(DataIV);
            return DataBlock.DecryptString(key, iv).JsonToObject<User>();
        }

        public bool ChangePassword(string oldPassword, string newPassword)
        {
            if(!string.IsNullOrEmpty(PasswordHash)) // Verify old password
            {
                var oldHash = Crypto.PasswordHash(oldPassword, PasswordSalt, 32, PasswordIter);
                var oldHash64 = Convert.ToBase64String(oldHash);
                if(oldHash64 != PasswordHash)
                    return false;
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