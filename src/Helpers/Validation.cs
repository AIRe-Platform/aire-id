using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Aire.Helpers
{
    public static class Validation
    {
        public static bool IsValidEmail(string email)
        {
            try 
            { 
                var address = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPassword(string password)
        {
            bool digits = Regex.IsMatch(password, @"\d+");
            bool lowercaseLetters = Regex.IsMatch(password, @"[a-z]");
            bool uppercaseLetters = Regex.IsMatch(password, @"[A-Z]");
            return !string.IsNullOrWhiteSpace(password) 
                && (password.Length > 6)
                && (digits && lowercaseLetters && uppercaseLetters);
        }
    }
}