using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DozorBot.Models
{
    [Table("app_users")]
    public class AppUser
    {
        [Key]
        [MaxLength(32)]
        [Column("id")]
        public string Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Required]
        [Column("create_date")]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        [Column("update_date")]
        public DateTime? UpdateDate { get; set; }

        [Column("telegram_user_id")]
        public long? TelegramUserId { get; set; }

        [Column("domain")]
        public int Domain { get; set; }

        [MaxLength(80)]
        [Column("domain_uid")]
        public string? DomainUid { get; set; }

        [Column("legacy_id")]
        public int LegacyUserId { get; set; }

        [ForeignKey("LegacyUserId")]
        public AspNetUser LegacyUser { get; set; }

        [Required]
        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Required]
        [Column("is_blocked")]
        public bool IsBlocked { get; set; } = false;

        [Required]
        [Column("is_manual_role_set")]
        public bool IsManualRoleSet { get; set; } = true;

        [Required]
        [Column("is_autocreated")]
        public bool IsAutoCreated { get; set; } = false;

        [Column("guid")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Guid { get; set; }
    }
}