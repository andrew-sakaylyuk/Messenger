using System.Data.Entity;

namespace InstantMessagingServerApp.Models
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<Role> Roles { get; set; }

        public ApplicationDbContext()
            : base("InstantMessagingDb")
        {
            Configuration.ProxyCreationEnabled = false;
            Configuration.LazyLoadingEnabled = false;
            Database.SetInitializer(new ApplicationDbInitializer());
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Message>()
                .HasRequired(t => t.Receiver)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Message>()
                .HasRequired(t => t.Sender)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Friendship>()
                .HasRequired(t => t.FirstFriend)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Friendship>()
                .HasRequired(t => t.SecondFriend)
                .WithMany()
                .WillCascadeOnDelete(false);
        }

    }
}