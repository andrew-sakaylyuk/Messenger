using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstantMessagingServerApp.Models
{
    [Table("User")]
    public sealed class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "User name")]
        [Column(TypeName = "VARCHAR")]
        [MaxLength(100)]
        public string UserName { get; set; }

        [Required]
        [StringLength(255, ErrorMessage =
            "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [Column(TypeName = "VARCHAR")]
        public string PasswordHash { get; set; }

        [Required]
        [ForeignKey("RoleId")]
        public Role UserRole { get; set; }
        public int RoleId { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        [Column(TypeName = "VARCHAR")]
        [StringLength(255)]
        public string Email { get; set; }

        [Column(TypeName = "NVARCHAR")]
        [StringLength(100)]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Column(TypeName = "NVARCHAR")]
        [StringLength(100)]
        [MaxLength(100)]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Birth date")]
        [Column(TypeName = "DATE")]
        public DateTime BirthDate { get; set; }

        [Column("Sex")]
        public SexEnum Sex { get; set; }

        [Column(TypeName = "NVARCHAR")]
        [Display(Name = "Avatar")]
        [MaxLength(255)]
        public string AvatarUrl { get; set; }

        [NotMapped]
        public ICollection<Friendship> Friends1 { get; set; }
        [NotMapped]
        public ICollection<Friendship> Friends2 { get; set; }
        [NotMapped]
        public ICollection<Message> Messages1 { get; set; }
        [NotMapped]
        public ICollection<Message> Messages2 { get; set; }

        public User()
        {
            //UserRole = new Role();
            Friends1 = new List<Friendship>();
            Friends2 = new List<Friendship>();
            Messages1 = new List<Message>();
            Messages2 = new List<Message>();
        }
    }

    public enum SexEnum { Male, Female, Unknown }
}