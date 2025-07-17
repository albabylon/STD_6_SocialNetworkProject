using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetworkWebApp.Models.Users;

namespace SocialNetworkWebApp.Data.Configuration
{
    public class MessageConfuiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.ToTable("Messages").HasKey(p => p.Id);
            builder.Property(x => x.Id).UseIdentityColumn();

            // Отключаем каскадное удаление
            builder.HasOne(f => f.Sender)
                  .WithMany()
                  .HasForeignKey(f => f.SenderId)
                  .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(f => f.Recipient)
                  .WithMany()
                  .HasForeignKey(f => f.RecipientId)
                  .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
