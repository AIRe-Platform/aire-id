// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Sdk.Azure;
using Aire.Sdk.Models.Invites;

namespace Aire.Id.Models;

/// <summary>
/// PK = First 5 characters of token
/// RK = Token
/// </summary>
[EntityTable("InviteTokens")]
public class InviteTokenEntity : BaseTableEntity
{
    public DateTime Expiry { get; set; }
    public bool Active { get; set; }
    public string? UserId { get; set; }
    public string? ChatId { get; set; }
    public string? EmailHash { get; set; }
    public string? Platform { get; set; }
    public string? ClientId { get; set; }
    public string? Code { get; set; }
    public bool AccountUpgrade { get; set; }

    public InviteTokenEntity()
    {
        string id = Guid.NewGuid().ToString();
        PartitionKey ??= id;
        RowKey ??= id;
    }

    public InviteTokenEntity(string token)
    {
        PartitionKey = token[..5]; ;
        RowKey = token;
    }

    public string Token()
    {
        return RowKey ?? "";
    }

    public InviteToken ToModel()
    {
        return new InviteToken
        {
            Token = Token(),
            Expiry = Expiry,
            UserId = UserId,
            ChatId = ChatId,
            EmailHash = EmailHash,
            Platform = Platform,
            ClientId = ClientId,
            AccountUpgrade = AccountUpgrade,
        };
    }
}
