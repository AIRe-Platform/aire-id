// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Sdk.Azure;

namespace Aire.Id.Models;

[EntityTable("Users")]
public class DemoUserEntity : UserEntity
{
    public string? DemoGroupId { get; set; }
    public string? DemoAccessCode { get; set; }

    public DemoUserEntity() : base() { }
}
