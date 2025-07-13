using System.ComponentModel.DataAnnotations;

namespace SocialNetworkWebApp.ViewModels.AccountManager
{
    public class UserEditViewModel
    {
        public string UserId { get; set; }

        public string Email { get; set; }

        public string UserName => Email;

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string? MiddleName { get; set; }

        public DateTime BirthDate { get; set; }

        public string? Image { get; set; }

        public string? Status { get; set; }

        public string? About { get; set; }
    }
}
