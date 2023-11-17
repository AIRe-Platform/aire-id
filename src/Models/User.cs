using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Aire.Services.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Aire.Id.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Gender
    {
        [EnumMember(Value = "male")]
        Male,

        [EnumMember(Value = "female")]
        Female,

        [EnumMember(Value =  "other")]
        Other
    }

    public class UserPrivate
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("gender")]
        public Gender? Gender { get; set; }

        [JsonProperty("age")]
        public int? Age { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("connected_services")]
        public List<Service> ConnectedServices { get; set; }
    }

    public class User : UserPrivate
    {
        [JsonProperty("last_login")]
        public DateTime? LastLogin { get; set; }

        [JsonProperty("eula_accepted")]
        public DateTime? EulaAccepted { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }
    }
}