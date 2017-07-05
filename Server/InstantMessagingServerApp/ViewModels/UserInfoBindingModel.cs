using System.ComponentModel.DataAnnotations;

namespace InstantMessagingServerApp.ViewModels
{
    public class UserInfoBindingModel : UpdateUserInfoBindingModel
    {
        [Display(Name = "Username")]
        public string UserName { get; set; }
    }
}