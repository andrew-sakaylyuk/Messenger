using InstantMessagingServerApp.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using InstantMessagingServerApp.Services;
using Microsoft.Practices.ObjectBuilder2;

namespace InstantMessagingServerApp.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ApplicationDbContext _context;

        protected Pagination Pagination;

        public MessageRepository(ApplicationDbContext context, Pagination pagination)
        {
            _context = context;
            Pagination = pagination;
        }

        public void Add(Message entity)
        {
            _context.Messages.Add(entity);
        }

        public Message GetById(int messageId)
        {
            return _context.Messages.Find(messageId);
        }

        public int GetNewMessagesCount(int userId)
        {
            return _context.Messages.Count(m => m.ReceiverId == userId&&m.New);
        }

        //put userId, you want to find new messages with, under senderId
        public int GetNewMessagesCount(int senderId, int receiverId)
        {
            return _context.Messages
                .Count(m => m.SenderId == senderId && m.ReceiverId == receiverId && m.New);
        }

        public int GetLastMessagesPageCount(int thisUserId, int messagesPerPage)
        {
            return Pagination.CountPages(GetLastMessages(thisUserId).Count(), messagesPerPage);
        }

        public IEnumerable<Message> GetLastMessages(int thisUserId, int messagesPerPage, int page)
        {
            if (page < 1) page = 1;
            var lastMessages = GetLastMessages(thisUserId);
            return lastMessages
                .OrderByDescending(m => m.DateTime)
                .Skip(messagesPerPage * (page - 1))
                .Take(messagesPerPage)
                .OrderBy(m => m.DateTime);
        }

        private IEnumerable<Message> GetLastMessages(int thisUserId)
        {

            //get all messages of this user with other ones (sender or receiver)
            var allMessages = _context.Messages
                .Where(m => m.SenderId == thisUserId || m.ReceiverId == thisUserId);

            HashSet<int> userIds = new HashSet<int>();
            allMessages.ForEach(m =>
            {
                userIds.Add(m.SenderId);
                userIds.Add(m.ReceiverId);
            });
            List<Message> lastMessages = new List<Message>();
            userIds.ForEach(userId =>
            {
                Message lastMessage = allMessages.Where(m =>
                    m.SenderId == thisUserId && m.ReceiverId == userId
                    ||
                    m.SenderId == userId && m.ReceiverId == thisUserId
                ).OrderByDescending(m => m.DateTime).FirstOrDefault();
                if (lastMessage != null) lastMessages.Add(lastMessage);
            });
            
            return lastMessages;
        }

        public int GetCorrespondencePageCount(int senderId, int receiverId, int messagesPerPage)
        {
            return Pagination.CountPages(GetCorrespondence(senderId, receiverId)
                .Count(),messagesPerPage);
        }

        public IEnumerable<Message> GetCorrespondence(
            int senderId, int receiverId,int messagesPerPage,int page)
        {
            if (page < 1) page = 1;
            return GetCorrespondence(senderId, receiverId)
                .OrderByDescending(m=>m.DateTime)
                .Skip(messagesPerPage * (page - 1))
                .Take(messagesPerPage).OrderBy(m => m.DateTime);
        }

        private IQueryable<Message> GetCorrespondence(int senderId, int receiverId)
        {
            return _context.Messages.Where(m => m.SenderId == senderId
                & m.ReceiverId == receiverId || m.SenderId == receiverId
                & m.ReceiverId == senderId);
        }

        public int GetAllMessagesPageCount(int messagesPerPage)
        {
            return Pagination.CountPages(_context.Messages.Count(), messagesPerPage);
        }

        public IEnumerable<Message> GetMessages(int messagesPerPage, int page)
        {
            return _context.Messages
                .OrderByDescending(m => m.DateTime)
                .Skip(messagesPerPage * (page - 1))
                .Take(messagesPerPage).OrderBy(m => m.DateTime);
        }

        public void Update(Message entity)
        {
            _context.Messages.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Remove(int messageId)
        {
            var entity = _context.Messages.Find(messageId);
            Remove(entity);
        }

        public void Remove(Message entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _context.Messages.Attach(entity);
            }
            _context.Messages.Remove(entity);
        }
    }
}