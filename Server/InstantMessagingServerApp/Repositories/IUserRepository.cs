using InstantMessagingServerApp.Models;
using System.Collections.Generic;

namespace InstantMessagingServerApp.Repositories
{
    public interface IUserRepository
    {
        void Add(User entity);
        User GetById(int userId);
        User GetByName(string userName);
        User GetByEmail(string email);
        IEnumerable<User> GetAllUsers();
        int GetUsersPageCount(int usersPerPage);
        IEnumerable<User> GetUsers(int usersPerPage, int page);
        int FindUsersPageCount(ViewModels.UserInfoBindingModel searchUser, int usersPerPage);
        IEnumerable<User> FindUsers(ViewModels.UserInfoBindingModel searchUser, int usersPerPage, int page);
        void Update(User entity);
        void Remove(int userId);
        void Remove(User entity);
        
    }
}
