namespace Aire.Id.Oauth2.Models
{
    public class OauthSubject
    {
        public string? Subject { get; set; }
        public string? Role { get; set; }
        public List<string>? Scopes { get; set; }
        public Dictionary<string, object>? Claims { get; set; }
    }
}