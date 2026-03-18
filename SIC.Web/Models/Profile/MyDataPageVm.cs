using System.ComponentModel.DataAnnotations;

namespace SIC.Web.Models.Profile;

public sealed class MyDataPageVm : IValidatableObject
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

    [Display(Name = "Dia do Aniversário")]
    [Range(1, 31)]
    public int? DiaAniversario { get; set; }

    [Display(Name = "Mês do Aniversário")]
    [Range(1, 12)]
    public int? MesAniversario { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DiaAniversario.HasValue && MesAniversario.HasValue)
        {
            var maxDia = MaxDaysInMonth(MesAniversario.Value);
            if (DiaAniversario.Value > maxDia)
            {
                yield return new ValidationResult(
                    $"O mês selecionado possui no máximo {maxDia} dias.",
                    [nameof(DiaAniversario)]);
            }
        }
        else if (DiaAniversario.HasValue && !MesAniversario.HasValue)
        {
            yield return new ValidationResult(
                "Selecione o mês do aniversário.",
                [nameof(MesAniversario)]);
        }
        else if (!DiaAniversario.HasValue && MesAniversario.HasValue)
        {
            yield return new ValidationResult(
                "Selecione o dia do aniversário.",
                [nameof(DiaAniversario)]);
        }
    }

    private static int MaxDaysInMonth(int month) => month switch
    {
        2 => 29,
        4 or 6 or 9 or 11 => 30,
        _ => 31
    };
}
