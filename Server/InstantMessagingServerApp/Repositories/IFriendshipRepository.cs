using System.Collections.Generic;
using InstantMessagingServerApp.Models;

namespace InstantMessagingServerApp.Repositories
{
    public interface IFriendshipRepository
    {
        void AddFriendshipRequest(int senderId, int receiverId);
        void ConfirmFriendshipRequest(int senderId, int receiverId);
        int GetAllFriendshipRequestsPageCount(int receiverId, int usersPerPage);
        IEnumerable<User> GetAllFriendshipRequests(int receiverId, int usersPerPage, int page);
        int GetFriendsCount(int userId);
        int GetFriendsPageCount(int userId, int usersPerPage);
        IEnumerable<User> GetFriends(int userId, int usersPerPage, int page);
        IEnumerable<User> GetFriends(int userId);
        int GetMutualFriendsPageCount(int firstUserId, int secondUserId, int usersPerPage);
        IEnumerable<User> GetMutualFriends(int firstUserId, int secondUserId, int usersPerPage, int page); 
        void RemoveFriendship(int userId, int friendId);
        bool AreFriends(int userId, int friendId);
        bool FriendshipRequestAlreadyExists(int senderId, int receiverId); 
    }
}
