using System.ComponentModel.DataAnnotations;

namespace SIC.Web.Models.Profile;

public sealed class MyDataPageVm
{
    public int UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Foto { get; set; }
    public IReadOnlyList<UserPermissionVm> Permissoes { get; set; } = [];
    public IReadOnlyList<AreaVm> Areas { get; set; } = [];

    [Display(Name = "Telefone")]
    [StringLength(30)]
    public string? Telefone { get; set; }

    [Display(Name = "Ramal")]
    [StringLength(30)]
    public string? Ramal { get; set; }

    [Display(Name = "Matrícula")]
    public int? Matricula { get; set; }

    [Display(Name = "Cargo")]
    [StringLength(100)]
    public string? Cargo { get; set; }

    [Display(Name = "Setor")]
    [StringLength(100)]
    public string? Setor { get; set; }

    [Display(Name = "Área")]
    public int? AreaId { get; set; }
}
