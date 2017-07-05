using InstantMessagingServerApp.Models;
using System.Collections.Generic;

namespace InstantMessagingServerApp.Repositories
{
    public interface IMessageRepository
    {
        void Add(Message entity);
        Message GetById(int messageId);
        int GetNewMessagesCount(int userId);
        int GetNewMessagesCount(int senderId, int receiverId);
        int GetLastMessagesPageCount(int thisUserId, int messagesPerPage);
        IEnumerable<Message> GetLastMessages(int thisUserId, int messagesPerPage, int page);
        int GetCorrespondencePageCount(int senderId, int receiverId, int messagesPerPage);
        IEnumerable<Message> GetCorrespondence(int senderId, int receiverId,int messagesPerPage,int page);
        int GetAllMessagesPageCount(int messagesPerPage);
        IEnumerable<Message> GetMessages(int messagesPerPage, int page);
        void Update(Message entity);
        void Remove(int messageId);
        void Remove(Message entity);
    }
}
