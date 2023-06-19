using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DozorBot.Models;

[Table("telegram_messages")]
public class TelegramMessage
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [ForeignKey("user_id")]
    public AppUser User { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("text")]
    public string Text { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("status")]
    public string Status { get; set; }

    [MaxLength]
    [Column("additional")]
    public string Additional { get; set; }

    [Required]
    [Column(name: "create_date", TypeName = "timestamp without time zone")]
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;
}