using Microsoft.EntityFrameworkCore;
using SocialNetworkWebApp.Models.Users;

namespace SocialNetworkWebApp.Data.Repository
{
    public class FriendsRepository : Repository<Friend>
    {
        public FriendsRepository(ApplicationDbContext db) : base(db)
        {

        }

        public async Task AddFriend(User target, User Friend)
        {
            var friends = Set.AsEnumerable().FirstOrDefault(x => x.UserId == target.Id && x.CurrentFriendId == Friend.Id);

            if (friends == null)
            {
                var item = new Friend()
                {
                    UserId = target.Id,
                    User = target,
                    CurrentFriend = Friend,
                    CurrentFriendId = Friend.Id,
                };

                await Create(item);
            }
        }

        public async Task<List<User>> GetFriendsByUser(User target)
        {
            //Include для того, чтобы при получении списка сущностей получить все данные.
            //Это «жадная загрузка» (eager loading) в явном виде, сделано это так, потому что нам нужно явно передать именно список друзей, а не только при обращении.
            return await Set
                .Include(x => x.CurrentFriend)
                .Include(x => x.User)
                .Where(x => x.User.Id == target.Id)
                .Select(x => x.CurrentFriend)
                .ToListAsync();
        }

        public async Task DeleteFriend(User target, User Friend)
        {
            var friends = Set.AsEnumerable().FirstOrDefault(x => x.UserId == target.Id && x.CurrentFriendId == Friend.Id);

            if (friends != null)
            {
                await Delete(friends);
            }
        }
    }
}
