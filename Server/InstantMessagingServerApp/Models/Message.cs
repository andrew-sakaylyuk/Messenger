using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InstantMessagingServerApp.Models
{
    [Table("Message")]
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }
        public int SenderId { get; set; }

        [Required]
        [ForeignKey("ReceiverId")]
        public virtual User Receiver { get; set; }
        public int ReceiverId { get; set; } 

        [Required]
        [Column(TypeName = "NVARCHAR")]
        [StringLength(2000)]
        public string Text { get; set; }

        [Required]
        [Column(TypeName = "DATETIME")]
        public DateTime DateTime { get; set; }

        [Required]
        [Column("New")]
        public bool New { get; set; }
    }
}