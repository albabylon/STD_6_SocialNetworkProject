using SocialNetworkWebApp.Models.Users;

namespace SocialNetworkWebApp.ViewModels.AccountManager
{
    public class UserViewModel
    {
        public User User { get; set; }

        public UserViewModel(User user)
        {
            User = user;
        }
    }
}
