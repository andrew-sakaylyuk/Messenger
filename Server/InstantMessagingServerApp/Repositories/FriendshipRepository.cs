using InstantMessagingServerApp.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Data.Entity;
using InstantMessagingServerApp.Services;

namespace InstantMessagingServerApp.Repositories
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly ApplicationDbContext _context;

        protected Pagination Pagination;

        public FriendshipRepository(ApplicationDbContext context, Pagination pagination)
        {
            _context = context;
            Pagination = pagination;
        }

        public void AddFriendshipRequest(int senderId, int receiverId)
        {
            if (AreFriends(senderId, receiverId) || senderId == receiverId) return;
            //if user wants to send friend request twice - return
            if (FriendshipRequestAlreadyExists(senderId, receiverId)) return;
            //if user wants to send friend request, but that other user already sent
            //friend request to this user - confirm
            if (FriendshipRequestAlreadyExists(receiverId, senderId))
            {
                ConfirmFriendshipRequest(receiverId, senderId);
                return;
            }
            var friendship = new Friendship
            {
                FirstFriendId = senderId,
                SecondFriendId = receiverId,
                Confirmed = false
            };
            _context.Friendships.Add(friendship);
        }

        public void ConfirmFriendshipRequest(int senderId, int receiverId)
        {
            if (AreFriends(senderId, receiverId) || senderId == receiverId) return;
            var friendship = _context.Friendships.FirstOrDefault(
                t => t.FirstFriendId == senderId && t.SecondFriendId == receiverId);
            if (friendship == null) return;
            friendship.Confirmed = true;
            _context.Friendships.Attach(friendship);
            _context.Entry(friendship).State = EntityState.Modified;
        }

        public int GetAllFriendshipRequestsPageCount(int receiverId,int usersPerPage)
        {
            return Pagination.CountPages(GetAllFriendshipRequests(receiverId).Count(),usersPerPage);
        }

        public IEnumerable<User> GetAllFriendshipRequests(int receiverId, int usersPerPage, int page)
        {
            if (page < 1) page = 1;
            return GetAllFriendshipRequests(receiverId)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Skip(usersPerPage * (page - 1))
                .Take(usersPerPage);
        }

        private IEnumerable<User> GetAllFriendshipRequests(int receiverId)
        {
            try
            {
                var friendships = _context.Friendships.Where(
                    t => t.SecondFriendId == receiverId && !t.Confirmed).ToList();
                return friendships.Select(friendship => _context.Users.FirstOrDefault(
                    t => t.Id == friendship.FirstFriendId)).ToList();
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }

        public int GetFriendsCount(int userId)
        {
            return GetFriends(userId).Count();
        }

        public int GetFriendsPageCount(int userId, int usersPerPage)
        {
            return Pagination.CountPages(GetFriends(userId).Count(), usersPerPage);
        }

        public IEnumerable<User> GetFriends(int userId, int usersPerPage, int page)
        {
            if (page < 1) page = 1;
            return GetFriends(userId)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Skip(usersPerPage * (page - 1))
                .Take(usersPerPage);
        }

        public IEnumerable<User> GetFriends(int userId)
        {
            try
            {
                var friendships1 = _context.Friendships.Where(
                    t => t.FirstFriendId == userId && t.Confirmed).ToList();
                var friends = friendships1.Select(friendship => _context.Users.FirstOrDefault(
                    t => t.Id == friendship.SecondFriendId)).ToList();
                var friendships2 = _context.Friendships.Where(
                    t => t.SecondFriendId == userId && t.Confirmed).ToList();
                friends.AddRange(friendships2.Select(friendship =>
                    _context.Users.FirstOrDefault(t => t.Id == friendship.FirstFriendId)));
                return friends;
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }

        public int GetMutualFriendsPageCount(int firstUserId, int secondUserId, int usersPerPage)
        {
            return Pagination.CountPages(
                GetMutualFriends(firstUserId, secondUserId).Count(), usersPerPage);

        }

        public IEnumerable<User> GetMutualFriends(
            int firstUserId, int secondUserId, int usersPerPage, int page)
        {
            if (page < 1) page = 1;
            return GetMutualFriends(firstUserId,secondUserId)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Skip(usersPerPage * (page - 1))
                .Take(usersPerPage);
        }

        private IEnumerable<User> GetMutualFriends(int firstUserId, int secondUserId)
        {
            var firstUsersFriends = _context.Friendships.Where(t => 
                (t.FirstFriendId == firstUserId || 
                t.SecondFriendId == firstUserId) && t.Confirmed).ToList();
            var secondUsersFriends = _context.Friendships.Where(t =>
                (t.FirstFriendId == secondUserId || 
                t.SecondFriendId == secondUserId) && t.Confirmed).ToList();
            var friends1 = firstUsersFriends.Select(fr1 => 
                _context.Users.FirstOrDefault(t => 
                t.Id == fr1.FirstFriendId && firstUserId != fr1.FirstFriendId
                || t.Id == fr1.SecondFriendId && firstUserId != fr1.SecondFriendId)).ToList();
            var friends2 = secondUsersFriends.Select(fr2 => 
                _context.Users.FirstOrDefault(t => 
                t.Id == fr2.FirstFriendId && secondUserId != fr2.FirstFriendId
                || t.Id == fr2.SecondFriendId && secondUserId != fr2.SecondFriendId)).ToList();
            return from fr1 in friends1 from fr2 in friends2
                where fr1.Id == fr2.Id select fr1;
        }

        public void RemoveFriendship(int userId, int friendId)
        {
            try
            {
                var friendship = _context.Friendships.FirstOrDefault(t =>
                    t.FirstFriendId == userId && t.SecondFriendId == friendId ||
                    t.FirstFriendId == friendId && t.SecondFriendId == userId);
                if (friendship != null) _context.Friendships.Remove(friendship);
            }
            catch (ArgumentNullException) { }
        }

        public bool AreFriends(int userId, int friendId)
        {
            return _context.Friendships.Any(t =>
                t.FirstFriendId == userId && t.SecondFriendId == friendId && t.Confirmed ||
                t.FirstFriendId == friendId && t.SecondFriendId == userId && t.Confirmed);
        }

        public bool FriendshipRequestAlreadyExists(int senderId, int receiverId)
        {
            return _context.Friendships.Any(t =>
                t.FirstFriendId == senderId && t.SecondFriendId == receiverId && !t.Confirmed);
        }
    }
}