using System.ComponentModel.DataAnnotations;

namespace DozorBot.Models;

public class Setting
{
    [Key]
    [MaxLength(255)]
    public string Key { get; set; }

    [Required]
    [MaxLength(512)]
    public string Value { get; set; }
}
