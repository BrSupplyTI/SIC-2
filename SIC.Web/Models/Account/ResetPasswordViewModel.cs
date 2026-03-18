using System.ComponentModel.DataAnnotations;

namespace SIC.Web.Models.Account;

public sealed class ResetPasswordViewModel
{
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a nova senha.")]
    [MinLength(8, ErrorMessage = "A nova senha deve ter no mínimo 8 caracteres.")]
    public string NewPassword { get; set; } = string.Empty;

    public string? Message { get; set; }
    public bool Success { get; set; }
}
