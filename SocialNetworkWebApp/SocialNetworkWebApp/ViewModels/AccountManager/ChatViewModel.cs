using SocialNetworkWebApp.Models.Users;

namespace SocialNetworkWebApp.ViewModels.AccountManager
{
    public class ChatViewModel
    {
        public User Sender { get; set; }

        public User Recepient {get; set;}

        public List<Message> MessageHistory { get; set;}

        public MessageViewModel NewMessage { get; set; }

        public ChatViewModel()
        {
            NewMessage = new MessageViewModel();
        }
    }
}
