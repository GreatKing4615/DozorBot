using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DozorBot.Models;
[Table("settings")]
public class Settings
{
    [Key]
    [MaxLength(255)]
    [Column("key")]
    public string Key { get; set; }

    [Required]
    [MaxLength(512)]
    [Column("value")]
    public string Value { get; set; }
}
