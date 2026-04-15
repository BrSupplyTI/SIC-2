using System.ComponentModel.DataAnnotations;

namespace SIC.Api.Contracts.Projetos;

public sealed class TarefaCriarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Projeto inválido.")]
    public int ProjetoID { get; set; }

    [Required(ErrorMessage = "O nome da tarefa é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O nome da tarefa deve ter no máximo 200 caracteres.")]
    public string NmTarefa { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "A descrição deve ter no máximo 2000 caracteres.")]
    public string? DsTarefa { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Status da tarefa inválido.")]
    public int ProjetoTarefaStatusID { get; set; } = 1;

    [Range(1, int.MaxValue, ErrorMessage = "Prioridade da tarefa inválida.")]
    public int ProjetoTarefaPrioridadeID { get; set; } = 2;

    public int? UsuarioResponsavelID { get; set; }
    public string? DtInicio { get; set; }
    public string? DtPrevisaoFim { get; set; }
    public int? ProjetoTarefaPaiID { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Usuário inválido.")]
    public int UsuarioID { get; set; }
}
