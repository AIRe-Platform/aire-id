using Newtonsoft.Json;

namespace Aire.Id.Models
{
    public class UserCredentials
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}