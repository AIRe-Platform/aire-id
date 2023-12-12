using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Aire.Id.Helpers
{
    public static class Validation
    {
        public static bool IsValidEmail(string? email)
        {
            try 
            { 
                if(email == null)
                    return false;
                var address = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPassword(string? password)
        {
            if(password == null)
                return false;

            bool digits = Regex.IsMatch(password, @"\d+");
            bool lowercaseLetters = Regex.IsMatch(password, @"[a-z]");
            bool uppercaseLetters = Regex.IsMatch(password, @"[A-Z]");
            
            return !string.IsNullOrWhiteSpace(password) 
                && (password.Length >= 6)
                && (digits && lowercaseLetters && uppercaseLetters);
        }
    }
}