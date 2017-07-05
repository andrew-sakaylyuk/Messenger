using System.Collections.Generic;
using System.Linq;
using InstantMessagingServerApp.Models;

namespace InstantMessagingServerApp.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Role> GetAll()
        {
            return _context.Roles.ToList();
        }

        public Role Get(string roleName)
        {
            return _context.Roles.FirstOrDefault(t => t.UserRole == roleName);
        }

        public void Add(string roleName)
        {
            _context.Roles.Add(new Role() { UserRole = roleName });
        }

        public void Remove(string roleName)
        {
            var entity = _context.Roles.First(t => t.UserRole == roleName);
            _context.Roles.Remove(entity);
        }
    }
}