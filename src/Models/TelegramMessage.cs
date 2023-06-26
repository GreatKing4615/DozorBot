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
    [Column("user_id")]
    public int UserId { get; set; }
       
    public AppUser User { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("text")]
    public string Text { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("status")]
    public string Status { get; set; }

    [Column("additional")]
    [MaxLength(128)]
    public string? Additional { get; set; }

    [Required]
    [Column(name: "create_date", TypeName = "timestamp with time zone")]
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;
}