using System.ComponentModel.DataAnnotations;

namespace SIC.Web.Models.Account;

public sealed class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Informe o login ou e-mail.")]
    public string Identifier { get; set; } = string.Empty;
    public string? Message { get; set; }
    public bool Success { get; set; }
}
