using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CvParsing.Models.Dtos;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

public class ResetPasswordRequest
{
    [Required]
    [JsonPropertyName("token")]
    public string? Token { get; set; }

    [Required]
    [MinLength(8)]
    [JsonPropertyName("newPassword")]
    public string? NewPassword { get; set; }

    [Required]
    [JsonPropertyName("confirmPassword")]
    public string? ConfirmPassword { get; set; }
}
