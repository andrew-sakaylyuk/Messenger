using System.ComponentModel.DataAnnotations;

namespace InstantMessagingServerApp.ViewModels
{
    public class UpdateUserInfoBindingModel
    {
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Date of Birth")]
        public string BirthDate { get; set; }

        [Display(Name = "Sex")]
        public string Sex { get; set; }
    }
}