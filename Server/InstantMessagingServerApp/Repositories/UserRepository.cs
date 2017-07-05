using System;
using System.Linq;
using InstantMessagingServerApp.Models;
using System.Collections.Generic;
using System.Data.Entity;
using InstantMessagingServerApp.Services;

namespace InstantMessagingServerApp.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        protected Pagination Pagination;

        public UserRepository(ApplicationDbContext context, Pagination pagination)
        {
            _context = context;
            Pagination = pagination;
        }

        public void Add(User entity)
        {
            _context.Users.Add(entity);
        }

        public User GetById(int userId)
        {
            return _context.Users.Find(userId);
        }

        public User GetByName(string userName)
        {
            return _context.Users.FirstOrDefault(t => t.UserName == userName);
        }

        public User GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(t => t.Email == email);
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }

        public int GetUsersPageCount(int usersPerPage)
        {
            return Pagination.CountPages(_context.Users.Count(), usersPerPage);
        }

        public IEnumerable<User> GetUsers(int usersPerPage, int page)
        {
            return _context.Users
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Skip(usersPerPage * (page - 1))
                .Take(usersPerPage);
        }

        public int FindUsersPageCount(ViewModels.UserInfoBindingModel searchUser, int usersPerPage)
        {
            return Pagination.CountPages(FindUsers(searchUser).Count(),usersPerPage);
        }

        public IEnumerable<User> FindUsers(ViewModels.UserInfoBindingModel searchUser, int usersPerPage, int page)
        {
            if (page < 1) page = 1;
            return FindUsers(searchUser)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Skip(usersPerPage * (page - 1))
                .Take(usersPerPage);
        }

        private IQueryable<User> FindUsers(ViewModels.UserInfoBindingModel searchUser)
        {
            var sex = SexEnum.Unknown;
            switch (searchUser.Sex)
            {
                case "Male":
                    sex = SexEnum.Male;
                    break;
                case "Female":
                    sex = SexEnum.Female;
                    break;
            }
            var birthDate = new DateTime();
            if (searchUser.BirthDate != null)
            {
                birthDate = DateTime.ParseExact(searchUser.BirthDate, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture);
            }
            return from users in _context.Users
                where (searchUser.UserName == null || users.UserName.StartsWith(searchUser.UserName)) 
                && (searchUser.FirstName == null || users.FirstName.StartsWith(searchUser.FirstName)) 
                && (searchUser.LastName == null || users.LastName.StartsWith(searchUser.LastName)) 
                && (searchUser.Email == null || users.Email == searchUser.Email)
                && (searchUser.Sex == null || users.Sex == sex)
                && (searchUser.BirthDate == null || users.BirthDate == birthDate)
                select users;
        }

        public void Update(User entity)
        {
            _context.Users.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Remove(int userId)
        {
            var entity = _context.Users.Find(userId);
            Remove(entity);

            var filePath = System.Web.HttpContext.Current.Server
                .MapPath("~/wwwroot/Avatars/" + entity.UserName + ".png");
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            try
            {
                var friendships = _context.Friendships.Where(t => 
                    t.FirstFriendId == userId || t.SecondFriendId == userId).ToList();
                if (friendships != null)
                {
                    foreach(var friendship in friendships)
                        _context.Friendships.Remove(friendship);
                }
                var conversations = _context.Messages.Where(t => 
                    t.SenderId == userId || t.ReceiverId == userId);
                if (conversations != null)
                {
                    foreach (var conversation in conversations)
                        _context.Messages.Remove(conversation);
                }
            }
            catch (ArgumentNullException) { }
        }

        public void Remove(User entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _context.Users.Attach(entity);
            }
            _context.Users.Remove(entity);
        }

    }
}