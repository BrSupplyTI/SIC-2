using System.ComponentModel.DataAnnotations;

namespace SIC.Api.Contracts.Projetos;

public sealed class ProjetoAtualizarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Projeto inválido.")]
    public int ProjetoID { get; set; }

    [Required(ErrorMessage = "O nome do projeto é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O nome do projeto deve ter no máximo 200 caracteres.")]
    public string NmProjeto { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "A descrição deve ter no máximo 2000 caracteres.")]
    public string DsProjeto { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Status inválido.")]
    public int ProjetoStatusID { get; set; }

    public string? DtInicio { get; set; }
    public string? DtPrevisaoFim { get; set; }
    public string? DtFimReal { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Usuário inválido.")]
    public int UsuarioID { get; set; }
}
