using System.ComponentModel.DataAnnotations;

namespace HungryHub.Validators
{
    public class AuthorizedEmailAttribute : ValidationAttribute
    {
       
        private static readonly HashSet<string> AllowedDomains =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Google
            "gmail.com",
            "googlemail.com",

            // Microsoft
            "outlook.com",
            "hotmail.com",
            "hotmail.co.uk",
            "hotmail.fr",
            "live.com",
            "live.co.uk",
            "msn.com",
            "windowslive.com",

            // Yahoo
            "yahoo.com",
            "yahoo.co.uk",
            "yahoo.co.in",
            "yahoo.fr",
            "yahoo.de",
            "yahoo.com.au",
            "ymail.com",

            // Apple
            "icloud.com",
            "me.com",
            "mac.com",

            // ProtonMail
            "protonmail.com",
            "proton.me",
            "pm.me",

            // Others
            "zoho.com",
            "aol.com",
            "mail.com",
            "gmx.com",
            "gmx.net",
            "tutanota.com",
            "imap.com",
        };

        protected override ValidationResult? IsValid(
            object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return new ValidationResult("Email address is required.");

            string email = value.ToString()!.Trim();

         
            if (!email.Contains('@'))
                return new ValidationResult(
                    "Please enter a valid email address.");

            string domain = email.Split('@').Last().ToLower();

            if (!AllowedDomains.Contains(domain))
            {
                return new ValidationResult(
                    $"'{domain}' is not an authorized email provider. " +
                    "Please use Gmail, Outlook, Yahoo, or another " +
                    "recognized email service.");
            }

            return ValidationResult.Success;
        }
    }
}
