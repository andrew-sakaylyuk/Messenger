using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstantMessagingServerApp.Models
{
    [Table("Role")]
    public sealed class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Role")]
        [Column(TypeName = "VARCHAR")]
        [StringLength(255)]
        public string UserRole { get; set; }

        public ICollection<User> UsersWithThisRole { get; set; } 
        public Role()
        {
            UsersWithThisRole = new List<User>();
        }

    }
}