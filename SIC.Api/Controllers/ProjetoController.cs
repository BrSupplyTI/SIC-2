using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SIC.Api.Contracts.Projetos;
using SIC.Api.Services;

namespace SIC.Api.Controllers;

[ApiController]
[Route("api/projetos")]
public sealed class ProjetoController(IProjetoService service) : ControllerBase
{
    // ── Leitura ──────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] ProjetoFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await service.ListarProjetosAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("com-tarefas")]
    public async Task<IActionResult> ListarComTarefas([FromQuery] ProjetoFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await service.ListarProjetosComTarefasAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{projetoId:int}")]
    public async Task<IActionResult> Detalhes(int projetoId, CancellationToken cancellationToken)
    {
        var dto = await service.ObterDetalhesAsync(projetoId, cancellationToken);
        if (dto is null) return NotFound();
        return Ok(dto);
    }

    [HttpGet("{projetoId:int}/tarefas")]
    public async Task<IActionResult> ListarTarefas(int projetoId, CancellationToken cancellationToken)
    {
        var result = await service.ListarTarefasAsync(projetoId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{projetoId:int}/participantes")]
    public async Task<IActionResult> ListarParticipantes(int projetoId, CancellationToken cancellationToken)
    {
        var result = await service.ListarParticipantesAsync(projetoId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{projetoId:int}/historico")]
    public async Task<IActionResult> ListarHistorico(int projetoId, CancellationToken cancellationToken)
    {
        var result = await service.ListarHistoricoAsync(projetoId, cancellationToken);
        return Ok(result);
    }

    // ── Lookups ──────────────────────────────────────────────

    [HttpGet("status")]
    public async Task<IActionResult> ListarStatus(CancellationToken cancellationToken)
    {
        var result = await service.ObterStatusListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("tarefa-status")]
    public async Task<IActionResult> ListarTarefaStatus(CancellationToken cancellationToken)
    {
        var result = await service.ObterTarefaStatusListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("tarefa-prioridades")]
    public async Task<IActionResult> ListarPrioridades(CancellationToken cancellationToken)
    {
        var result = await service.ObterPrioridadeListAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("usuarios")]
    public async Task<IActionResult> BuscarUsuarios([FromQuery] string texto = "", CancellationToken cancellationToken = default)
    {
        var result = await service.BuscarUsuariosAsync(texto, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{projetoId:int}/verificar-participante")]
    public async Task<IActionResult> VerificarParticipante(int projetoId, [FromQuery] int usuarioId, CancellationToken cancellationToken)
    {
        var ehParticipante = await service.VerificarParticipanteAsync(projetoId, usuarioId, cancellationToken);
        return Ok(new { ehParticipante });
    }

    // ── Escrita — Projeto ────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ProjetoCriarDto dto, CancellationToken cancellationToken)
    {
        if (ValidarIntervalo(dto.DtInicio, dto.DtPrevisaoFim) is { } erroProjCriar)
            return BadRequest(new { mensagem = erroProjCriar });

        var projetoId = await service.CriarProjetoAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Detalhes), new { projetoId }, new { ProjetoID = projetoId });
    }

    [HttpPut]
    public async Task<IActionResult> Atualizar([FromBody] ProjetoAtualizarDto dto, CancellationToken cancellationToken)
    {
        if (ValidarIntervalo(dto.DtInicio, dto.DtPrevisaoFim) is { } erroProjAtualizar)
            return BadRequest(new { mensagem = erroProjAtualizar });

        var projetoId = await service.AtualizarProjetoAsync(dto, cancellationToken);
        return Ok(new { ProjetoID = projetoId });
    }

    // ── Escrita — Tarefa ─────────────────────────────────────

    [HttpPost("tarefas")]
    public async Task<IActionResult> CriarTarefa([FromBody] TarefaCriarDto dto, CancellationToken cancellationToken)
    {
        if (ValidarIntervalo(dto.DtInicio, dto.DtPrevisaoFim) is { } erroTarCriar)
            return BadRequest(new { mensagem = erroTarCriar });

        var tarefaId = await service.CriarTarefaAsync(dto, cancellationToken);
        return Created(string.Empty, new { ProjetoTarefaID = tarefaId });
    }

    [HttpPut("tarefas")]
    public async Task<IActionResult> AtualizarTarefa([FromBody] TarefaAtualizarDto dto, CancellationToken cancellationToken)
    {
        if (ValidarIntervalo(dto.DtInicio, dto.DtPrevisaoFim) is { } erroTarAtualizar)
            return BadRequest(new { mensagem = erroTarAtualizar });

        try
        {
            var tarefaId = await service.AtualizarTarefaAsync(dto, cancellationToken);
            return Ok(new { ProjetoTarefaID = tarefaId });
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpDelete("tarefas/{projetoTarefaId:int}")]
    public async Task<IActionResult> ExcluirTarefa(int projetoTarefaId, [FromQuery] int usuarioId, CancellationToken cancellationToken)
    {
        if (projetoTarefaId <= 0 || usuarioId <= 0)
            return BadRequest(new { mensagem = "Parâmetros inválidos." });

        try
        {
            var tarefaId = await service.ExcluirTarefaAsync(projetoTarefaId, usuarioId, cancellationToken);
            return Ok(new { ProjetoTarefaID = tarefaId });
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // ── Escrita — Participante ─────────────────────────────────

    [HttpPost("participantes")]
    public async Task<IActionResult> AdicionarParticipante([FromBody] ParticipanteAdicionarDto dto, CancellationToken cancellationToken)
    {
        if (dto.ProjetoID <= 0 || dto.UsuarioID <= 0)
            return BadRequest(new { mensagem = "Parâmetros inválidos." });

        try
        {
            var id = await service.AdicionarParticipanteAsync(dto, cancellationToken);
            return Created(string.Empty, new { ProjetoParticipanteID = id });
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPut("participantes/papel")]
    public async Task<IActionResult> AtualizarPapelParticipante([FromBody] ParticipanteAtualizarPapelDto dto, CancellationToken cancellationToken)
    {
        if (dto.ProjetoParticipanteID <= 0)
            return BadRequest(new { mensagem = "Participante inválido." });

        try
        {
            var id = await service.AtualizarPapelParticipanteAsync(dto, cancellationToken);
            return Ok(new { ProjetoParticipanteID = id });
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpDelete("participantes/{projetoParticipanteId:int}")]
    public async Task<IActionResult> RemoverParticipante(int projetoParticipanteId, [FromQuery] int usuarioLogadoId, CancellationToken cancellationToken)
    {
        if (projetoParticipanteId <= 0 || usuarioLogadoId <= 0)
            return BadRequest(new { mensagem = "Parâmetros inválidos." });

        try
        {
            var id = await service.RemoverParticipanteAsync(projetoParticipanteId, usuarioLogadoId, cancellationToken);
            return Ok(new { ProjetoParticipanteID = id });
        }
        catch (SqlException ex) when (ex.Number == 50000)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // ── Validação ────────────────────────────────────────────

    private static string? ValidarIntervalo(string? dtInicio, string? dtPrevisaoFim)
    {
        if (DateTime.TryParse(dtInicio, out var inicio) && DateTime.TryParse(dtPrevisaoFim, out var fim))
        {
            if (fim < inicio)
                return "A previsão de término não pode ser anterior à data de início.";
        }
        return null;
    }
}
