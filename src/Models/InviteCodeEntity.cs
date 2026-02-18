// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Sdk.Azure;
using Aire.Sdk.Models.Invites;

namespace Aire.Id.Models;

/// <summary>
/// Code = PartitionKey, RowKey
/// </summary>
[EntityTable("InviteCodes")]
public class InviteCodeEntity : BaseTableEntity
{
    public string? Name { get; set; }
    public bool Active { get; set; }
    public DateTime Created { get; set; }
    public DateTime Expiry { get; set; }
    public int TrialDuration { get; set; }
    public int Limit { get; set; }
    public int Used { get; set; }
    public string? OwnerId { get; set; }
    public string? ClientId { get; set; }
    public string? Platform { get; set; }
    public bool AccountUpgrade { get; set; }
    public string? Link { get; set; }

    public InviteCodeEntity()
    {
        string code = Guid.NewGuid().ToString();
        PartitionKey ??= code;
        RowKey ??= code;
        Created = DateTime.UtcNow;
    }

    public InviteCodeEntity(string code)
    {
        PartitionKey = code;
        RowKey = code;
        Created = DateTime.UtcNow;
    }

    public string Code()
    {
        return RowKey ?? "";
    }

    public InviteCode ToModel()
    {
        return new InviteCode
        {
            Code = Guid.Parse(Code()),
            Name = Name,
            Active = Active,
            Created = Created,
            Expiry = Expiry,
            TrialDuration = TrialDuration,
            Limit = Limit,
            Used = Used,
            ClientId = ClientId,
            Platform = Platform,
            Link = Link,
            AccountUpgrade = AccountUpgrade
        };
    }
}
