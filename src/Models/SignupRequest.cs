using Newtonsoft.Json;

namespace Aire.Id.Models
{
    public class SignupRequest
    {
        [JsonProperty("credentials")]
        public UserCredentials Credentials { get; set; }
    }
}