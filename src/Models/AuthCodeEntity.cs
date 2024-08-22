// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Sdk.Azure;

namespace Aire.Id.Models;

/// <summary>
/// Code = PartitionKey (first 5 chars), RowKey
/// </summary>
[EntityTable("AuthCodes")]
public class AuthCodeEntity : BaseTableEntity
{
    public string? ClientId { get; set; }
    public string? ClientSecretHash { get; set; }
    public string? UserId { get; set; }
    public string? UserKey { get; set; }
    public string? Scopes { get; set; }
    public string? State { get; set; }
    public string? RedirectUri { get; set; }
    public DateTime? Expires { get; set; }
    public string? Verifier { get; set; }

    public AuthCodeEntity() { }
    public AuthCodeEntity(string code)
    {
        PartitionKey = code[..5];
        RowKey = code;
    }
}
