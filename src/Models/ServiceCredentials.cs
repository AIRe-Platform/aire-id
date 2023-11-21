using Newtonsoft.Json;

namespace Aire.Id.Models
{
    public class ServiceCredentials
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("token", Required = Required.Always)]
        public string Token { get; set; }
    }
}
