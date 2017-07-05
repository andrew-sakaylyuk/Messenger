using InstantMessagingServerApp.Models;
using System.Collections.Generic;

namespace InstantMessagingServerApp.Repositories
{
    public interface IRoleRepository
    {
        IEnumerable<Role> GetAll();
        Role Get(string roleName);
        void Add(string roleName);
        void Remove(string roleName);
    }
}
