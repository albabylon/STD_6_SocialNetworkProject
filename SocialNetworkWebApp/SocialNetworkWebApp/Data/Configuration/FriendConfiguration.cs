using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SocialNetworkWebApp.Models.Users;

namespace SocialNetworkWebApp.Data.Configuration
{
    public class FriendConfiguration : IEntityTypeConfiguration<Friend>
    {
        public void Configure(EntityTypeBuilder<Friend> builder)
        {
            builder.ToTable("UserFriends").HasKey(p => p.Id);
            builder.Property(x => x.Id).UseIdentityColumn();

            // Отключаем каскадное удаление
            builder.HasOne(f => f.User)
                  .WithMany()
                  .HasForeignKey(f => f.UserId)
                  .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(f => f.CurrentFriend)
                  .WithMany()
                  .HasForeignKey(f => f.CurrentFriendId)
                  .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
