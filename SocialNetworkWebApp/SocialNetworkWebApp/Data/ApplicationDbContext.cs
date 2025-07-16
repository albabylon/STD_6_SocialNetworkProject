using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using SocialNetworkWebApp.Data.Configuration;
using SocialNetworkWebApp.Models.Users;

namespace SocialNetworkWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

            //if (!Database.GetService<IRelationalDatabaseCreator>().Exists())
            //{
                Database.EnsureCreated();
            //}
            //else
            //{
            //    Database.Migrate();
            //}
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new FriendConfiguration());
        }
    }
}
