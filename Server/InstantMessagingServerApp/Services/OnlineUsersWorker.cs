using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using InstantMessagingServerApp.Hubs;
using InstantMessagingServerApp.Models;
using InstantMessagingServerApp.ViewModels;
using Microsoft.Practices.ObjectBuilder2;

namespace InstantMessagingServerApp.Services
{
    public class OnlineUsersWorker
    {
        public bool IsOnline(User user)
        {
            if (MessageHub.Connections.GetConnections(user.UserName).Count() != 0)
                return true;
            return false;
        }

        public UserReturnModel[] MarkIfOnline(IEnumerable<User> users)
        {
            //use Dictionary for efficient search online users
            Dictionary<string, UserReturnModel> userModels = new Dictionary<string, UserReturnModel>();
            //fill this Dictionary with UserName-UserReturnModel
            foreach (var user in users)
            {
                try
                {
                    userModels.Add(user.UserName, Mapper.Map<User, UserReturnModel>(user));
                }
                catch { }
            }
            //users.ForEach(u => userModels.Add(u.UserName, Mapper.Map<User, UserReturnModel>(u)));
            var onlineUserNames = MessageHub.Connections.GetUsersList();
            foreach (string userName in onlineUserNames)
            {
                UserReturnModel uModel;
                if (userModels.TryGetValue(userName, out uModel))
                {
                    uModel.Online = true;
                }
            }
            //return only UserReturnModels
            return userModels.Values.ToArray();
        }
    }
}