using System;

namespace InstantMessagingServerApp.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepository { get; }
        IMessageRepository MessageRepository { get; }
        IRoleRepository RoleRepository { get; }
        IFriendshipRepository FriendshipRepository { get; }
        void Save();
    }
}
