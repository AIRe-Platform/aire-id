// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.


using Aire.Id.Oauth2.Models;
using Newtonsoft.Json;

namespace Aire.Id.Models;

public class InviteAuthResponse
{
    [JsonProperty("user_id")]
    public string? UserId { get; set; }

    [JsonProperty("chat_id")]
    public string? ChatId { get; set; }

    [JsonProperty("platform")]
    public string? Platform { get; set; }

    [JsonProperty("account_upgrade")]
    public bool AccountUpgrade { get; set; }

    [JsonProperty("auth")]
    public OauthTokenResponse? Auth { get; set; }

    [JsonProperty("invitation")]
    public string? Invitation { get; set; }
}
