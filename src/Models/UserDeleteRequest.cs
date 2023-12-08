using Newtonsoft.Json;

namespace Aire.Id.Models
{
    public class UserDeleteRequest
    {
        [JsonProperty("password", Required = Required.Always)]
        public string? Password { get; set; }

        [JsonProperty("keep_anonymized_data")]
        public bool KeepAnonymizedData { get; set; }
    }
}