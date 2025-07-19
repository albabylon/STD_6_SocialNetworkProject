using Microsoft.AspNetCore.SignalR;

namespace SocialNetworkWebApp
{
    public class ChatHub : Hub
    {
        // Метод для отправки сообщения всем клиентам
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        // Метод для отправки сообщения конкретному пользователю
        public async Task SendPrivateMessage(string sender, string receiver, string message)
        {
            await Clients.User(receiver).SendAsync("ReceivePrivateMessage", sender, message);
        }
    }
}
