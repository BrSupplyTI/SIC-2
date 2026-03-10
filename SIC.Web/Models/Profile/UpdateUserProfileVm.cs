using System.ComponentModel.DataAnnotations;

namespace SIC.Web.Models.Profile;

public sealed class UpdateUserProfileVm
{
    public int UsuarioId { get; set; }

    [Display(Name = "Telefone")]
    [StringLength(30)]
    public string? Telefone { get; set; }

    [Display(Name = "Área")]
    public int? AreaId { get; set; }
}
