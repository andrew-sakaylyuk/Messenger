namespace InstantMessagingServerApp.ViewModels
{
    public class UserReturnModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string BirthDate { get; set; }
        public string Sex { get; set; }
        public string AvatarUrl { get; set; }
        public bool Online { get; set; }
    }
}