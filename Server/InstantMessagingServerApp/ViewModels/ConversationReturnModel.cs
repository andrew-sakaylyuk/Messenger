using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InstantMessagingServerApp.ViewModels
{
    public class ConversationReturnModel
    {
        public string Text { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateTime { get; set; }
        public int OtherUserId { get; set; }
        public int NewMessagesCount { get; set; }
        public string AvatarUrl { get; set; }
        public bool Online { get; set; }
    }
}