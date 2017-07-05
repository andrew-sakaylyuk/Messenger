using System;
using InstantMessagingServerApp.Models;
using System.Data.Entity.Validation;
using InstantMessagingServerApp.Services;

namespace InstantMessagingServerApp.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private IUserRepository _useRepository; 
        private IMessageRepository _messageRepository;
        private IRoleRepository _roleRepository;
        private IFriendshipRepository _friendshipRepository;
        private readonly Pagination _pagination = new Pagination();

        public IUserRepository UserRepository => _useRepository ?? 
            (_useRepository = new UserRepository(_context, _pagination));

        public IMessageRepository MessageRepository => _messageRepository ?? 
            (_messageRepository = new MessageRepository(_context, _pagination));

        public IRoleRepository RoleRepository => _roleRepository ?? 
            (_roleRepository = new RoleRepository(_context));

        public IFriendshipRepository FriendshipRepository => _friendshipRepository ??
            (_friendshipRepository = new FriendshipRepository(_context,_pagination));

        public void Save()
        { 
            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    System.Diagnostics.Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" " +
                        "has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Value: \"{1}\", " +
                            "Error: \"{2}\"", ve.PropertyName,
                            eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName),
                            ve.ErrorMessage);
                    }
                }
                throw;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.InnerException);
            }
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}