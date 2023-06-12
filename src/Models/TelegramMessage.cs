﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DozorBot.Models;

public class TelegramMessage
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

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

    [MaxLength(128)]
    [Column("additional")]
    public string Additional { get; set; }

    [Required]
    [Column(name: "create_date", TypeName = "timestamp with time zone")]
    public DateTime CreateDate { get; set; }= DateTime.Now;
}