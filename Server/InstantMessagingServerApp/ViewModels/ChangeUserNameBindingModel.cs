using System.ComponentModel.DataAnnotations;

namespace InstantMessagingServerApp.ViewModels
{
    public class ChangeUserNameBindingModel
    {
        [Display(Name = "Username")]
        public string UserName { get; set; }
    }
}