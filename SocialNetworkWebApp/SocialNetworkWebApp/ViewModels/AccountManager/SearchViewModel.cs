using SocialNetworkWebApp.Models.Users;

namespace SocialNetworkWebApp.ViewModels.AccountManager
{
    public class SearchViewModel
    {
        public IEnumerable<UserWithFriendExt> UserList {  get; init; }

        public SearchViewModel()
        {
            UserList = new List<UserWithFriendExt>();
        }
    }
}
