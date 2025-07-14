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
        }
    }
}
