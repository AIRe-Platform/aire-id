using System.Security.Cryptography;
using System.Text;
using Aire.Id.Helpers;
using Aire.Sdk.Auth;
using Aire.Sdk.Azure;
using Aire.Sdk.Helpers;
using Aire.Sdk.Models.Admin;
using Aire.Sdk.Models.Identity;

namespace Aire.Id.Models;

/// <summary>
/// UUID: PartitionKey, RowKey
/// </summary>
[EntityTable("Users")]
public class UserEntity : BaseTableEntity
{
    public string? Username { get; set; }
    public string? EmailHash { get; set; }
    public string? PublicName { get; set; }
    public string? Role { get; set; }
    public string? Scopes { get; set; }
    public string? AdditionalScopes { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime? EulaAccepted { get; set; }

    // Verification
    public bool Verified { get; set; }
    public string? VerificationCode { get; set; }
    public DateTime? VerificationCodeExpiry { get; set; }
    public int? VerificationCodeRetryCount { get; set; }

    // Password properties
    public string? PasswordSalt { get; set; }
    public string? PasswordHash { get; set; }
    public int PasswordIter { get; set; }

    // Encrypted data
    public string? DataIV { get; set; }
    public string? DataBlock { get; set; }
    public string? Encryption { get; set; }
    public string? Recovery { get; set; }

    public UserEntity()
    {
        string uuid = Guid.NewGuid().ToString();
        PartitionKey ??= uuid;
        RowKey ??= uuid;
    }

    /// <summary>
    /// Get user identifier
    /// </summary>
    /// <returns>Unique user identifier</returns>
    public string UUID()
    {
        return RowKey ?? "";
    }

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
    }

    /// <summary>
    /// Decrypts and deserializes the private user data
    /// </summary>
    /// <param name="encryptionKey">Encryption key</param>
    /// <returns>Private user model</returns>
    public UserPrivate? GetPrivateUserData(string encryptionKey)
    {
        if (string.IsNullOrEmpty(encryptionKey))
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
        User? user = (User?)GetPrivateUserData(encryptionKey);
        user ??= new User();
        user.UUID = RowKey;
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
        if (!string.IsNullOrEmpty(PasswordHash))
        {
            // Verify old password
            if (oldPassword == null)
                return false;

            var oldHash = Crypto.PasswordHash(oldPassword, PasswordSalt, 32, PasswordIter);
            var oldHash64 = Convert.ToBase64String(oldHash);
            if (oldHash64 != PasswordHash)
                return false;

            // Re-encrypt data key
            var key = GetEncryptionKey(oldPassword);
            var keyBytes = Convert.FromBase64String(key!);
            SetEncryptionKey(RowKey!, newPassword, keyBytes);
        }

        PasswordIter = 100000;

        var salt = RandomNumberGenerator.GetBytes(32);
        PasswordSalt = Convert.ToBase64String(salt);

        var hash = Crypto.PasswordHash(newPassword, PasswordSalt, 32, PasswordIter);
        PasswordHash = Convert.ToBase64String(hash);

        return true;
    }

    /// <summary>
    /// Change the password without old password. This requires that the account has recovery enabled.
    /// </summary>
    /// <param name="newPassword">New password</param>
    /// <returns>True if success, otherwise false.</returns>
    public bool RecoverAccount(string newPassword)
    {
        var key = RecoverEncryptionKey();
        if (key == null)
            return false;
        PasswordHash = null;
        if(!ChangePassword(null, newPassword))
            return false;

        var keyBytes = Convert.FromBase64String(key!);
        SetEncryptionKey(RowKey!, newPassword, keyBytes);
        return true;
    }

    /// <summary>
    /// Sets a random user data encryption key
    /// which is encrypted using user credentials
    /// </summary>
    /// <param name="password">User password</param>
    public void GenerateEncryptionKey(string password)
    {
        if (RowKey == null)
            throw new InvalidOperationException("User identifier is not set");

        if (!string.IsNullOrWhiteSpace(Encryption))
            throw new InvalidOperationException("Encryption key is already set");

        var encKeyBytes = RandomNumberGenerator.GetBytes(32);
        SetEncryptionKey(RowKey, password, encKeyBytes);
    }

    /// <summary>
    /// Sets account unverified and generates 
    /// </summary>
    public string GenerateVerificationCode()
    {
        string code = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
        Verified = false;
        VerificationCode = code;
        VerificationCodeExpiry = DateTime.UtcNow.AddHours(1);
        VerificationCodeRetryCount = 0;
        return code;
    }

    /// <summary>
    /// Try setting account verified
    /// </summary>
    /// <param name="code">Verification code</param>
    /// <returns>True if verification was successful. Otherwise false.</returns>
    public bool VerifyAccount(string code)
    {
        if (code == VerificationCode && !string.IsNullOrWhiteSpace(code) &&
            VerificationCodeRetryCount < AireConstants.MaxVerificationRetryCount &&
            VerificationCodeExpiry.HasValue &&
            VerificationCodeExpiry.Value > DateTime.UtcNow)
        {
            Verified = true;
            VerificationCode = "";
            VerificationCodeExpiry = DateTime.UtcNow;
            VerificationCodeRetryCount = 0;
            return true;
        }

        VerificationCodeRetryCount++;
        return false;
    }

    /// <summary>
    /// Reads user data encryption key and decrypts it
    /// </summary>
    /// <param name="password">User password</param>
    /// <returns>Base64 encoded data encryption key</returns>
    public string? GetEncryptionKey(string password)
    {
        var key = Crypto.PasswordHash(RowKey + password, null, 32, 100000);
        var parts = Encryption!.Split(".");

        if (parts.Length != 2)
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
        if (encKeyBytes.Length != 32)
            throw new ArgumentException("Encryption key has to be 256 bits in length", nameof(encKeyBytes));
        var encKey = Convert.ToBase64String(encKeyBytes);

        // Encryption with password
        {
            var key = Crypto.PasswordHash(uuid + password, null, 32, 100000);
            var iv = RandomNumberGenerator.GetBytes(16);
            Encryption = $"{encKey.EncryptString(key, iv)}.{Convert.ToBase64String(iv)}";
        }

        // Optional global recovery
        if (AireEnvironment.GlobalRecoveryKey != null)
        {
            var recoveryKey = Encoding.UTF8.GetBytes(AireEnvironment.GlobalRecoveryKey);
            if (recoveryKey.Length != 32)
                throw new Exception("Global recovery key is not 256 bits in length");

            var iv = RandomNumberGenerator.GetBytes(16);
            Recovery = $"{encKey.EncryptString(recoveryKey, iv)}.{Convert.ToBase64String(iv)}";
        }
    }

    /// <summary>
    /// If the account has global recovery enabled, the encryption key can be accessed with a master key.
    /// </summary>
    /// <returns>Encryption key in base-64</returns>
    public string? RecoverEncryptionKey()
    {
        string? globalRecoveryKey = AireEnvironment.GlobalRecoveryKey;
        if (globalRecoveryKey == null)
            return null;

        var key = Encoding.UTF8.GetBytes(globalRecoveryKey);
        if (key.Length != 32)
            throw new Exception("Global recovery key is not 256 bits in length");

        if (string.IsNullOrEmpty(Recovery))
            return null;

        var rec = Recovery.Split(".");
        if (rec.Length != 2)
            return null;

        return rec[0].DecryptString(key, Convert.FromBase64String(rec[1]));
    }

    /// <summary>
    /// Construct an account model from the entity
    /// </summary>
    /// <returns>Account model</returns>
    public Account ToAccountModel()
    {
        return new Account
        {
            Id = RowKey,
            Username = Username,
            PublicName = PublicName,
            Verified = Verified,
            EulaAccepted = EulaAccepted,
            LastLogin = LastLogin,
            Role = Role ?? AireRoles.User,
            OverrideScopes = Scopes != null && Scopes.Length > 0,
            Scopes = ScopeHelper.GetScopesForUser(this),
            AdditionalScopes = ScopeHelper.GetAdditionalScopesForUser(this)
        };
    }
}
