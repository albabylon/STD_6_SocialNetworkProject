using Microsoft.EntityFrameworkCore;
using SocialNetworkWebApp.Models.Users;

namespace SocialNetworkWebApp.Data.Repository
{
    public class MessageRepository : Repository<Message>
    {
        public MessageRepository(ApplicationDbContext db) : base(db)
        {

        }

        public async Task<IEnumerable<Message>> GetMessages(User sender, User recipient)
        {
            Set.Include(x => x.Recipient).Include(x => x.Sender);

            var from = await Set.Where(x => x.SenderId == sender.Id && x.RecipientId == recipient.Id).ToListAsync();
            var to = await Set.Where(x => x.SenderId == recipient.Id && x.RecipientId == sender.Id).ToListAsync();

            var result = new List<Message>();
            result.AddRange(from);
            result.AddRange(to);
            result.OrderBy(x => x.Id);
            
            return result;
        }
    }
}
