using InstantMessagingServerApp.Services;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace InstantMessagingServerApp.Models
{
    public class ApplicationDbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            IList<Role> defaultUserRoles = new List<Role>();
            defaultUserRoles.Add(new Role() { UserRole = "Admin"});
            defaultUserRoles.Add(new Role() { UserRole = "User" });
            foreach (var role in defaultUserRoles)
                context.Roles.Add(role);
            context.SaveChanges();
            var admin = new User()
            {
                UserName = "admin",
                Email = "admin@gmail.com",
                FirstName = "Білл",
                LastName = "Гейтс",
                PasswordHash = PasswordEncoder.Encode("admin"),
                UserRole = context.Roles.FirstOrDefault(t => t.UserRole == "Admin")
            };
            context.Users.Add(admin);
            context.SaveChanges();
            base.Seed(context);
        }
    }
}