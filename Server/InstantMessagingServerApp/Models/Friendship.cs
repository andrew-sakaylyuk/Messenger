using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstantMessagingServerApp.Models
{
    [Table("Friendship")]
    public class Friendship
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("FirstFriendId")]
        public virtual User FirstFriend { get; set; }
        public int FirstFriendId { get; set; }

        [Required]
        [ForeignKey("SecondFriendId")]
        public virtual User SecondFriend { get; set; }
        public int SecondFriendId { get; set; }

        [Required]
        public bool Confirmed { get; set; }
    }
}