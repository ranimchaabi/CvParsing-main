using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CvParsing.Models;

[Table("PasswordResetToken")]
public class PasswordResetToken
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    /// <summary>Uppercase hex SHA-256 of the URL token (64 chars).</summary>
    [MaxLength(64)]
    public string TokenHashHex { get; set; } = "";

    public DateTime ExpiresAtUtc { get; set; }

    public bool Used { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    [ForeignKey(nameof(UserId))]
    public Utilisateur? User { get; set; }
}
