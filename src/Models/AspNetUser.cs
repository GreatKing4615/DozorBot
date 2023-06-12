using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DozorBot.Models
{
    [Table("aspnet_users")]
    public class AspNetUser
    {
        [Key]
        [MaxLength(32)]
        [Column("id")]
        public string Id { get; set; }

        [Required]
        [MaxLength(64)]
        [Column("user_name")]
        public string UserName { get; set; }

        [Required]
        [MaxLength(64)]
        [Column("normalized_user_name")]
        public string NormalizedUserName { get; set; }

        [Required]
        [MaxLength(256)]
        [Column("email")]
        public string Email { get; set; }

        [Required]
        [MaxLength(256)]
        [Column("normalized_email")]
        public string NormalizedEmail { get; set; }

        [Required]
        [Column("email_confirmed")]
        public bool EmailConfirmed { get; set; }

        [MaxLength(32)]
        [Column("phone_number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Column("phone_number_confirmed")]
        public bool PhoneNumberConfirmed { get; set; }

        [Required]
        [Column("lockout_enabled")]
        public bool LockoutEnabled { get; set; }

        [Column("lockout_end_unix_time_milliseconds")]
        public long? LockoutEndUnixTimeMilliseconds { get; set; }

        [MaxLength(256)]
        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Required]
        [Column("access_failed_count")]
        public int AccessFailedCount { get; set; }

        [MaxLength(256)]
        [Column("security_stamp")]
        public string SecurityStamp { get; set; }

        [Column("two_factor_enabled")]
        public bool TwoFactorEnabled { get; set; }

        [MaxLength(36)]
        [Column("concurrency_stamp")]
        public string ConcurrencyStamp { get; set; }
    }
}