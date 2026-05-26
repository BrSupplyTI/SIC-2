using System.ComponentModel.DataAnnotations;

namespace SIC.Api.Contracts.Projetos;

public sealed class TarefaAtualizarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Tarefa inválida.")]
    public int ProjetoTarefaID { get; set; }

    [Required(ErrorMessage = "O nome da tarefa é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O nome da tarefa deve ter no máximo 200 caracteres.")]
    public string NmTarefa { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "A descrição deve ter no máximo 2000 caracteres.")]
    public string? DsTarefa { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Status da tarefa inválido.")]
    public int ProjetoTarefaStatusID { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Prioridade da tarefa inválida.")]
    public int ProjetoTarefaPrioridadeID { get; set; }

    public int? UsuarioResponsavelID { get; set; }
    public string? DtInicio { get; set; }
    public string? DtPrevisaoFim { get; set; }
    public string? DtFimReal { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Usuário inválido.")]
    public int UsuarioID { get; set; }
}
