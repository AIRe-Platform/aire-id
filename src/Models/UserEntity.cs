// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using System.Security.Cryptography;
using System.Text;
using Aire.Sdk.Auth;
using Aire.Sdk.Azure;
using Aire.Sdk.Helpers;
using Aire.Sdk.Models.Admin;
using Aire.Sdk.Models.Identity;
using Aire.Sdk.Platform;

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
    public DateTime? LastLogin { get; set; }
    public DateTime? EulaAccepted { get; set; }

    // Instance-wide role and scopes
    public string? Role { get; set; }
    public string? Scopes { get; set; }
    public string? AdditionalScopes { get; set; }

    // Access rights data per platform
    public string? AccessRightsData { get; set; }

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

    // Trial user
    public string? TrialNonce { get; set; }

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
        if (!ChangePassword(null, newPassword))
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
        if (AireIdEnvironment.GlobalRecoveryKey != null)
        {
            var recoveryKey = Encoding.UTF8.GetBytes(AireIdEnvironment.GlobalRecoveryKey);
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
        string? globalRecoveryKey = AireIdEnvironment.GlobalRecoveryKey;
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

    public static UserEntity CreateTrialUser(string emailHash, string token, string platform)
    {
        var user = new UserEntity()
        {
            EmailHash = emailHash,
            Verified = true,
            LastLogin = DateTime.UtcNow,
            TrialNonce = RandomNumberGenerator.GetHexString(32),
        };

        user.SetRole(platform, AireRoles.TrialUser);

        string password = user.GetTrialUserPassword(emailHash, token);
        user.ChangePassword(null, password);
        user.GenerateEncryptionKey(password);

        string enc = user.GetEncryptionKey(password)!;
        user.SetPrivateUserData(new UserPrivate(), enc);

        return user;
    }

    public string GetTrialUserPassword(string emailHash, string token)
    {
        string password = string.Join(".", [emailHash, token, TrialNonce]);
        return Crypto.SHA256Base16(password);
    }

    public bool UpgradeTrialUserToRegular(string password, string token, string platform)
    {
        if (!HasRole(AireRoles.TrialUser, platform))
            return false; // cannot upgrade another platform's trial users

        string trialpass = GetTrialUserPassword(EmailHash!, token);
        TrialNonce = null;
        SetRole(platform, AireRoles.User);
        return ChangePassword(trialpass, password);
    }

    /// <summary>
    /// Construct an account model from the entity
    /// </summary>
    /// <returns>Account model</returns>
    public Account ToAccountModel(string platform)
    {
        return new Account
        {
            Id = RowKey,
            Username = Username,
            PublicName = PublicName,
            Verified = Verified,
            EulaAccepted = EulaAccepted,
            LastLogin = LastLogin,
            Role = GetRole(platform),
            OverrideScopes = GetScopeOverride(platform) != null,
            Scopes = GetScopes(platform),
            AdditionalScopes = GetAdditionalScopes(platform)
        };
    }

    public Dictionary<string, AccessRights> GetAccessRights()
    {
        if (!string.IsNullOrWhiteSpace(AccessRightsData))
            return AccessRightsData.JsonToObject<Dictionary<string, AccessRights>>()
                ?? throw new AirePlatformException("Corrupted access rights");
        else
            return [];
    }

    public AccessRights GetAccessRights(string platform)
    {
        if (!string.IsNullOrWhiteSpace(AccessRightsData))
        {
            var dict = GetAccessRights();
            if (dict?.ContainsKey(platform) ?? false)
            {
                return dict[platform];
            }
        }
        return GetFallbackAccessRights();
    }

    public AccessRights GetFallbackAccessRights()
    {
        // Revert to obsolete fields
        return new AccessRights()
        {
            Role = Role ?? AireRoles.User,
            OverrideScopes = string.IsNullOrWhiteSpace(Scopes) ? null : AireScopes.ParseString(Scopes),
            AdditionalScopes = string.IsNullOrWhiteSpace(AdditionalScopes) ? null : AireScopes.ParseString(AdditionalScopes)
        };
    }

    public void SetAccessRights(string platform, AccessRights rights)
    {
        Dictionary<string, AccessRights> dict = [];
        if (!string.IsNullOrWhiteSpace(AccessRightsData))
            dict = AccessRightsData.JsonToObject<Dictionary<string, AccessRights>>()!;

        dict[platform] = rights;
        AccessRightsData = dict.ObjectToJson();
    }

    public bool HasRole(string role, string? platform = null)
    {
        if (platform == null)
        {
            return GetAccessRights().Any(x => x.Value.Role == role);
        }
        else
        {
            return GetRole(platform) == role;
        }
    }

    public string GetRole(string platform)
    {
        return GetAccessRights(platform).Role;
    }

    public void SetRole(string platform, string role)
    {
        var rights = GetAccessRights(platform);
        rights.Role = role;
        SetAccessRights(platform, rights);
    }

    public AireScopes GetScopes(string platform, bool baseScopesOnly = false)
    {
        var rights = GetAccessRights(platform);
        var scopes = rights.GetScopes(baseScopesOnly);

        if (!Verified)
        {
            if (scopes.Contains(AireScopes.PasswordChange))
                return [AireScopes.PasswordChange];
            else
                return [];
        }

        return scopes;
    }

    public void SetScopeOverride(string platform, AireScopes scopes)
    {
        var rights = GetAccessRights(platform);
        rights.OverrideScopes = scopes;
        SetAccessRights(platform, rights);
    }

    public AireScopes? GetScopeOverride(string platform)
    {
        return GetAccessRights(platform).OverrideScopes;
    }

    public AireScopes GetAdditionalScopes(string platform)
    {
        return GetAccessRights(platform).AdditionalScopes ?? [];
    }

    public void SetAdditionalScopes(string platform, AireScopes scopes)
    {
        var rights = GetAccessRights(platform);
        rights.AdditionalScopes = scopes;
        SetAccessRights(platform, rights);
    }
}
