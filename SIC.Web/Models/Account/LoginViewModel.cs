using System.ComponentModel.DataAnnotations;

namespace SIC.Web.Models.Account;

public sealed class LoginViewModel
{
    public LoginInputViewModel Input { get; set; } = new();
    public string? ReturnUrl { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class LoginInputViewModel
{
    [Required(ErrorMessage = "Informe o login.")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    public string Password { get; set; } = string.Empty;
}
