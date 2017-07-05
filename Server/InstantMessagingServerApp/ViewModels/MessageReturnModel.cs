namespace InstantMessagingServerApp.ViewModels
{
    public class MessageReturnModel
    {
        public int Id { get; set; }
        public string DateTime { get; set; }
        public string Text { get; set; }
        public string FirstName { get; set; }
        public int SenderId { get; set; }
        public bool New { get; set; }
    }
}