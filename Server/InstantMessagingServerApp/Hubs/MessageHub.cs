using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InstantMessagingServerApp.Models;
using InstantMessagingServerApp.Repositories;
using InstantMessagingServerApp.Services;
using InstantMessagingServerApp.ViewModels;
using Microsoft.AspNet.SignalR;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Utility;

namespace InstantMessagingServerApp.Hubs
{
    //todo: normal authentication
    public class MessageHub : Hub
    {
        public static readonly ConnectionMapping<string> Connections =
            new ConnectionMapping<string>();

        protected readonly IUnitOfWork UnitOfWork;
        protected OnlineUsersWorker OnlineWorker;

        private const int MessagesPerPage = 25;
        private const int ConversationsPerPage = 25;

        public MessageHub(IUnitOfWork unitOfWork, OnlineUsersWorker onlineUsersWorker)
        {
            UnitOfWork = unitOfWork;
            OnlineWorker = onlineUsersWorker;
        }

        public void SendMessage(int userId, string text)
        {
            var principal = JwtManager.GetPrincipal(Context.QueryString.Get("Bearer"));
            string thisUsername = principal.Identity.Name;
            var thisUser = UnitOfWork.UserRepository.GetByName(thisUsername);
            var user = UnitOfWork.UserRepository.GetById(userId);
            if (user == null)
            {
                return;
            }

            Message message = new Message
            {
                DateTime = DateTime.UtcNow,
                Sender = thisUser,
                Receiver = user,
                Text = text,
                New = true
            };
            UnitOfWork.MessageRepository.Add(message);
            UnitOfWork.Save();
            List<string> connectionIds = new List<string>();
            //send to my online clients
            foreach (var connectionId in Connections.GetConnections(thisUsername))
            {
                connectionIds.Add(connectionId);
            }
            //send to online user's clients
            foreach (var connectionId in Connections.GetConnections(user.UserName))
            {
                connectionIds.Add(connectionId);
            }
            var pageCount = UnitOfWork.MessageRepository.
                GetCorrespondencePageCount(thisUser.Id, user.Id, MessagesPerPage);
            Clients.Clients(connectionIds)
                .OnSendMessage(Mapper.Map<Message, MessageReturnModel>(message), pageCount);
        }

        //Call this method only for messages of one user at a time
        //Only receiver of message calls this method
        public void MakeMessagesOld(int[] messagesIds)
        {
            if (messagesIds.Length == 0) return;
            Message message = null;
            foreach (var messageId in messagesIds)
            {
                message = UnitOfWork.MessageRepository.GetById(messageId);
                message.New = false;
                message.Sender = UnitOfWork.UserRepository.GetById(message.SenderId);
                message.Receiver = UnitOfWork.UserRepository.GetById(message.ReceiverId);
                UnitOfWork.MessageRepository.Update(message);
            }
            UnitOfWork.Save();

            List<string> connectionIds = new List<string>();
            //we need only one message

            //sending to message sender's online clients

            foreach (var connectionId in Connections.GetConnections(message.Sender.UserName))
            {
                connectionIds.Add(connectionId);
            }
            Clients.Clients(connectionIds).OnMakeMessagesOld(messagesIds);
        }

        public int GetNewMessagesCount()
        {
            var s = Context.QueryString;
            var principal = JwtManager.GetPrincipal(s.Get("Bearer"));
            string thisUsername = principal.Identity.Name;
            var thisUser = UnitOfWork.UserRepository.GetByName(thisUsername);
            var newMessagesCount = UnitOfWork.MessageRepository.GetNewMessagesCount(thisUser.Id);
            return newMessagesCount;
        }

        public Pair<int, int> GetNewMessagesCountWith(int userId)
        {
            var s = Context.QueryString;
            var principal = JwtManager.GetPrincipal(s.Get("Bearer"));
            string thisUsername = principal.Identity.Name;
            var thisUser = UnitOfWork.UserRepository.GetByName(thisUsername);
            var newMessagesCount = UnitOfWork.MessageRepository.GetNewMessagesCount(userId, thisUser.Id);
            return new Pair<int, int>(userId, newMessagesCount);
        }

        public void GetCorrespondence(int userId, int page)
        {
            var principal = JwtManager.GetPrincipal(Context.QueryString.Get("Bearer"));
            string thisUsername = principal.Identity.Name;
            var thisUser = UnitOfWork.UserRepository.GetByName(thisUsername);
            var user = UnitOfWork.UserRepository.GetById(userId);
            if (user == null)
            {
                return;
            }
            var messages = UnitOfWork.MessageRepository
                .GetCorrespondence(thisUser.Id, user.Id, MessagesPerPage, page)
                .Select(Mapper.Map<Message, MessageReturnModel>);
            var pageCount = UnitOfWork.MessageRepository.
                GetCorrespondencePageCount(thisUser.Id, user.Id, MessagesPerPage);
            Clients.Caller.OnGetCorrespondence(messages, pageCount);
        }

        public void GetConversations(int page)
        {
            var principal = JwtManager.GetPrincipal(Context.QueryString.Get("Bearer"));
            string thisUsername = principal.Identity.Name;
            var thisUser = UnitOfWork.UserRepository.GetByName(thisUsername);
            var messages = UnitOfWork.MessageRepository
                .GetLastMessages(thisUser.Id, ConversationsPerPage, page);
            messages.ForEach(m => UnitOfWork.UserRepository
                .GetById(thisUser.Id != m.SenderId ? m.SenderId : m.ReceiverId));
            var conversations = messages.Select(m =>
            {
                //get other user
                var user = UnitOfWork.UserRepository
                    .GetById(thisUser.Id != m.SenderId ? m.SenderId : m.ReceiverId);
                var newMessagesCount = UnitOfWork.MessageRepository
                    .GetNewMessagesCount(user.Id, thisUser.Id);
                bool userIsOnline = OnlineWorker.IsOnline(user);
                ConversationReturnModel conversation = Mapper
                    .Map<Message, ConversationReturnModel>(m, opt =>
                    {
                        opt.Items["User"] = user;
                        opt.Items["NewMessagesCount"] = newMessagesCount;
                        opt.Items["Online"] = userIsOnline;
                    });
                return conversation;
            });
            var pageCount = UnitOfWork.MessageRepository.
                GetLastMessagesPageCount(thisUser.Id, MessagesPerPage);
            Clients.Caller.OnGetConversations(conversations, pageCount);
        }

        public void RemoveMessage(int messageId)
        {
            try
            {
                var principal = JwtManager.GetPrincipal(Context.QueryString.Get("Bearer"));
                string thisUsername = principal.Identity.Name;
                var thisUser = UnitOfWork.UserRepository.GetByName(thisUsername);
                Message message = UnitOfWork.MessageRepository.GetById(messageId);
                if (message == null)
                {
                    return;
                }
                //if current user is not a sender or receiver of this message he can't remove it
                if (message.SenderId != thisUser.Id && message.ReceiverId != thisUser.Id)
                    return;
                UnitOfWork.MessageRepository.Remove(messageId);
                UnitOfWork.Save();
                List<string> connectionIds = new List<string>();
                //remove messages on my clients
                foreach (var connectionId in Connections.GetConnections(thisUsername))
                {
                    connectionIds.Add(connectionId);
                }
                //remove messages on user's clients
                //we need other user's username so:
                var user = UnitOfWork.UserRepository.GetById(thisUser.Id == message.SenderId ? message.ReceiverId : message.SenderId);
                foreach (var connectionId in Connections.GetConnections(user.UserName))
                {
                    connectionIds.Add(connectionId);
                }
                var pageCount = UnitOfWork.MessageRepository.
                    GetCorrespondencePageCount(thisUser.Id, user.Id, MessagesPerPage);
                Clients.Clients(connectionIds).OnRemoveMessage(messageId, pageCount);
            }
            catch (Exception)
            {
                return;
            }
        }

        public void RemoveMessageU(int messageId)
        {
            try
            {
                var principal = JwtManager.GetPrincipal(Context.QueryString.Get("Bearer"));
                string thisUsername = principal.Identity.Name;
                var thisUser = UnitOfWork.UserRepository.GetByName(thisUsername);
                Message message = UnitOfWork.MessageRepository.GetById(messageId);
                if (message == null)
                {
                    return;
                }
                //if current user is not a sender or receiver of this message he can't remove it
                if (message.SenderId != thisUser.Id && message.ReceiverId != thisUser.Id)
                    return;
                UnitOfWork.MessageRepository.Remove(messageId);
                UnitOfWork.Save();
                List<string> connectionIds = new List<string>();
                //remove messages on my clients
                foreach (var connectionId in Connections.GetConnections(thisUsername))
                {
                    connectionIds.Add(connectionId);
                }
                //remove messages on user's clients
                //we need other user's username so:
                var user = UnitOfWork.UserRepository.GetById(thisUser.Id == message.SenderId ? message.ReceiverId : message.SenderId);
                foreach (var connectionId in Connections.GetConnections(user.UserName))
                {
                    connectionIds.Add(connectionId);
                }
                var pageCount = UnitOfWork.MessageRepository.
                    GetCorrespondencePageCount(thisUser.Id, user.Id, MessagesPerPage);
                Clients.Clients(connectionIds).OnRemoveMessageU(thisUser.Id, messageId, pageCount);
            }
            catch (Exception)
            {
                return;
            }
        }

        public void UserOnline()
        {
            try
            {
                var principal = JwtManager.GetPrincipal(Context.QueryString.Get("Bearer"));
                string thisUsername = principal.Identity.Name;
                var thisUser = UnitOfWork.UserRepository.GetByName(thisUsername);
                Clients.All.onUserOnline(thisUser.Id);
            }
            catch (Exception)
            {
                return;
            }
        }

        public void UserOffline()
        {
            try
            {
                var principal = JwtManager.GetPrincipal(Context.QueryString.Get("Bearer"));
                string thisUsername = principal.Identity.Name;
                var thisUser = UnitOfWork.UserRepository.GetByName(thisUsername);
                Clients.All.onUserOffline(thisUser.Id);
            }
            catch (Exception)
            {
                return;
            }
        }

        public void TypeMessage(int receiverId)
        {
            try
            {
                var principal = JwtManager.GetPrincipal(Context.QueryString.Get("Bearer"));
                string thisUsername = principal.Identity.Name;
                var thisUser = UnitOfWork.UserRepository.GetByName(thisUsername);
                List<string> connectionIds = new List<string>();

                //notify about typing message on other user's clients
                //we need other user's username so:
                var user = UnitOfWork.UserRepository.GetById(receiverId);
                foreach (var connectionId in Connections.GetConnections(user.UserName))
                {
                    connectionIds.Add(connectionId);
                }
                Clients.Clients(connectionIds).OnTypeMessage(thisUser.Id);
            }
            catch (Exception)
            {
                return;
            }
        }



        public void NotTypeMessage(int receiverId)
        {
            try
            {
                var principal = JwtManager.GetPrincipal(Context.QueryString.Get("Bearer"));
                string thisUsername = principal.Identity.Name;
                var thisUser = UnitOfWork.UserRepository.GetByName(thisUsername);
                List<string> connectionIds = new List<string>();

                //notify about typing message on other user's clients
                //we need other user's username so:
                var user = UnitOfWork.UserRepository.GetById(receiverId);
                foreach (var connectionId in Connections.GetConnections(user.UserName))
                {
                    connectionIds.Add(connectionId);
                }
                Clients.Clients(connectionIds).OnNotTypeMessage(thisUser.Id);
            }
            catch (Exception)
            {
                return;
            }
        }

        public override Task OnConnected()
        {
            var principal = JwtManager.GetPrincipal(Context.QueryString.Get("Bearer"));
            string name = principal.Identity.Name;
            Connections.Add(name, Context.ConnectionId);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var principal = JwtManager.GetPrincipal(Context.QueryString.Get("Bearer"));
            string name = principal.Identity.Name;
            Connections.Remove(name, Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var principal = JwtManager.GetPrincipal(Context.QueryString.Get("Bearer"));
            string name = principal.Identity.Name;

            if (!Connections.GetConnections(name).Contains(Context.ConnectionId))
            {
                Connections.Add(name, Context.ConnectionId);
            }
            return base.OnReconnected();
        }

    }
}