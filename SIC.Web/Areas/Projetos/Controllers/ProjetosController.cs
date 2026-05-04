using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Web.Services;

namespace SIC.Web.Areas.Projetos.Controllers;

[Area("Projetos")]
[Authorize]
[Route("Projetos")]
public sealed class ProjetosController(ProjetoApiClient apiClient) : Controller
{
    private int UsuarioLogadoID
        => int.TryParse(User.FindFirst("sic_usuarioid")?.Value, out var id) ? id : 0;

    private string NmUsuarioLogado
        => User.FindFirst("sic_nome")?.Value ?? "Usuário";

    private bool IsAdmin
        => User.FindFirst("sic_admin")?.Value == "1";

    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? texto,
        int projetoStatusId = 0,
        string orderBy = "Recentes",
        int page = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        var modo = Request.Cookies["sic_projetos_view"];
        var modoAtivo = modo is "quadro" or "lista" or "kanban" ? modo : "quadro";

        var statusListTask = apiClient.GetStatusListAsync(cancellationToken);
        var projetosTask = apiClient.GetProjetosAsync(
            texto, projetoStatusId, orderBy, page, pageSize, cancellationToken);

        await Task.WhenAll(statusListTask, projetosTask);

        var result = projetosTask.Result;
        result.StatusDisponiveis = statusListTask.Result;
        result.UsuarioLogadoID = UsuarioLogadoID;
        result.NmUsuarioLogado = NmUsuarioLogado;
        result.ModoVisualizacao = modoAtivo;

        // Carregar dados extras para modos Lista e Kanban
        if (modoAtivo is "lista" or "kanban" && result.Itens.Count > 0)
        {
            var comTarefasTask = apiClient.GetProjetosComTarefasAsync(
                texto, projetoStatusId, orderBy, page, pageSize, cancellationToken);
            var tarefaStatusTask = apiClient.GetTarefaStatusListAsync(cancellationToken);
            var prioridadeTask = apiClient.GetPrioridadeListAsync(cancellationToken);

            await Task.WhenAll(comTarefasTask, tarefaStatusTask, prioridadeTask);

            result.ItensComTarefas = comTarefasTask.Result.Itens;
            result.TarefaStatusDisponiveis = tarefaStatusTask.Result;
            result.TarefaPrioridadesDisponiveis = prioridadeTask.Result;
        }

        return View(result);
    }

    [HttpPost("Criar")]
    public async Task<IActionResult> Criar(
        [FromBody] CriarProjetoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NmProjeto))
            return BadRequest(new { mensagem = "O nome do projeto é obrigatório." });

        if (request.NmProjeto.Length > 200)
            return BadRequest(new { mensagem = "O nome do projeto deve ter no máximo 200 caracteres." });

        if (request.DsProjeto?.Length > 2000)
            return BadRequest(new { mensagem = "A descrição deve ter no máximo 2000 caracteres." });

        if (ValidarDatas(request.DtInicio, request.DtPrevisaoFim) is { } erroDatas)
            return BadRequest(new { mensagem = erroDatas });

        var payload = new
        {
            request.NmProjeto,
            request.DsProjeto,
            ProjetoStatusID = 1,
            request.DtInicio,
            request.DtPrevisaoFim,
            UsuarioCriadorID = UsuarioLogadoID
        };

        try
        {
            var projetoId = await apiClient.CriarProjetoAsync(payload, cancellationToken);

            if (projetoId == 0)
                return StatusCode(500, new { mensagem = "Erro ao criar o projeto." });

            return Ok(new { projetoId });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(ex.StatusCode is { } sc ? (int)sc : 500, new { mensagem = ex.Message });
        }
    }

    public sealed class CriarProjetoRequest
    {
        public string NmProjeto { get; set; } = string.Empty;
        public string DsProjeto { get; set; } = string.Empty;
        public string? DtInicio { get; set; }
        public string? DtPrevisaoFim { get; set; }
    }

    [HttpGet("{projetoId:int}")]
    public async Task<IActionResult> Detalhes(int projetoId, CancellationToken cancellationToken = default)
    {
        var vm = await apiClient.GetProjetoDetalhesAsync(projetoId, cancellationToken);
        if (vm is null) return NotFound();

        vm.StatusDisponiveis = await apiClient.GetStatusListAsync(cancellationToken);
        vm.TarefaStatusDisponiveis = await apiClient.GetTarefaStatusListAsync(cancellationToken);
        vm.TarefaPrioridadesDisponiveis = await apiClient.GetPrioridadeListAsync(cancellationToken);
        vm.UsuarioLogadoID = UsuarioLogadoID;
        vm.NmUsuarioLogado = NmUsuarioLogado;
        vm.IsAdmin = IsAdmin;

        return View(vm);
    }

    [HttpPost("Editar")]
    public async Task<IActionResult> Editar(
        [FromBody] EditarProjetoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ProjetoID <= 0)
            return BadRequest(new { mensagem = "Projeto inválido." });

        if (!await PodeEditarProjetoAsync(request.ProjetoID, cancellationToken))
            return StatusCode(403, new { mensagem = "Você não tem permissão para editar este projeto." });

        if (string.IsNullOrWhiteSpace(request.NmProjeto))
            return BadRequest(new { mensagem = "O nome do projeto é obrigatório." });

        if (request.NmProjeto.Length > 200)
            return BadRequest(new { mensagem = "O nome do projeto deve ter no máximo 200 caracteres." });

        if (request.DsProjeto?.Length > 2000)
            return BadRequest(new { mensagem = "A descrição deve ter no máximo 2000 caracteres." });

        if (request.ProjetoStatusID <= 0)
            return BadRequest(new { mensagem = "Status inválido." });

        if (ValidarDatas(request.DtInicio, request.DtPrevisaoFim) is { } erroDatas)
            return BadRequest(new { mensagem = erroDatas });

        var payload = new
        {
            request.ProjetoID,
            request.NmProjeto,
            request.DsProjeto,
            request.ProjetoStatusID,
            request.DtInicio,
            request.DtPrevisaoFim,
            request.DtFimReal,
            UsuarioID = UsuarioLogadoID
        };

        try
        {
            var result = await apiClient.AtualizarProjetoAsync(payload, cancellationToken);

            if (result == 0)
                return StatusCode(500, new { mensagem = "Erro ao atualizar o projeto." });

            return Ok(new { projetoId = result });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(ex.StatusCode is { } sc ? (int)sc : 500, new { mensagem = ex.Message });
        }
    }

    public sealed class EditarProjetoRequest
    {
        public int ProjetoID { get; set; }
        public string NmProjeto { get; set; } = string.Empty;
        public string DsProjeto { get; set; } = string.Empty;
        public int ProjetoStatusID { get; set; }
        public string? DtInicio { get; set; }
        public string? DtPrevisaoFim { get; set; }
        public string? DtFimReal { get; set; }
    }

    [HttpPost("CriarTarefa")]
    public async Task<IActionResult> CriarTarefa(
        [FromBody] CriarTarefaRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ProjetoID <= 0)
            return BadRequest(new { mensagem = "Projeto inválido." });

        if (!await PodeEditarProjetoAsync(request.ProjetoID, cancellationToken))
            return StatusCode(403, new { mensagem = "Você não tem permissão para gerenciar tarefas deste projeto." });

        if (string.IsNullOrWhiteSpace(request.NmTarefa))
            return BadRequest(new { mensagem = "O nome da tarefa é obrigatório." });

        if (request.NmTarefa.Length > 200)
            return BadRequest(new { mensagem = "O nome da tarefa deve ter no máximo 200 caracteres." });

        if (request.DsTarefa?.Length > 2000)
            return BadRequest(new { mensagem = "A descrição deve ter no máximo 2000 caracteres." });

        if (request.ProjetoTarefaStatusID <= 0)
            return BadRequest(new { mensagem = "Status inválido." });

        if (request.ProjetoTarefaPrioridadeID <= 0)
            return BadRequest(new { mensagem = "Prioridade inválida." });

        if (ValidarDatas(request.DtInicio, request.DtPrevisaoFim) is { } erroDatas)
            return BadRequest(new { mensagem = erroDatas });

        var payload = new
        {
            request.ProjetoID,
            request.NmTarefa,
            request.DsTarefa,
            request.ProjetoTarefaStatusID,
            request.ProjetoTarefaPrioridadeID,
            request.DtInicio,
            request.DtPrevisaoFim,
            request.ProjetoTarefaPaiID,
            UsuarioID = UsuarioLogadoID
        };

        try
        {
            var tarefaId = await apiClient.CriarTarefaAsync(payload, cancellationToken);

            if (tarefaId == 0)
                return StatusCode(500, new { mensagem = "Erro ao criar a tarefa." });

            return Ok(new { projetoTarefaId = tarefaId });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(ex.StatusCode is { } sc ? (int)sc : 500, new { mensagem = ex.Message });
        }
    }

    public sealed class CriarTarefaRequest
    {
        public int ProjetoID { get; set; }
        public string NmTarefa { get; set; } = string.Empty;
        public string? DsTarefa { get; set; }
        public int ProjetoTarefaStatusID { get; set; } = 1;
        public int ProjetoTarefaPrioridadeID { get; set; } = 2;
        public string? DtInicio { get; set; }
        public string? DtPrevisaoFim { get; set; }
        public int? ProjetoTarefaPaiID { get; set; }
    }

    [HttpPost("EditarTarefa")]
    public async Task<IActionResult> EditarTarefa(
        [FromBody] EditarTarefaRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ProjetoTarefaID <= 0)
            return BadRequest(new { mensagem = "Tarefa inválida." });

        if (request.ProjetoID <= 0)
            return BadRequest(new { mensagem = "Projeto inválido." });

        if (!await PodeEditarProjetoAsync(request.ProjetoID, cancellationToken))
            return StatusCode(403, new { mensagem = "Você não tem permissão para gerenciar tarefas deste projeto." });

        if (string.IsNullOrWhiteSpace(request.NmTarefa))
            return BadRequest(new { mensagem = "O nome da tarefa é obrigatório." });

        if (request.NmTarefa.Length > 200)
            return BadRequest(new { mensagem = "O nome da tarefa deve ter no máximo 200 caracteres." });

        if (request.DsTarefa?.Length > 2000)
            return BadRequest(new { mensagem = "A descrição deve ter no máximo 2000 caracteres." });

        if (request.ProjetoTarefaStatusID <= 0)
            return BadRequest(new { mensagem = "Status inválido." });

        if (request.ProjetoTarefaPrioridadeID <= 0)
            return BadRequest(new { mensagem = "Prioridade inválida." });

        if (ValidarDatas(request.DtInicio, request.DtPrevisaoFim) is { } erroDatas)
            return BadRequest(new { mensagem = erroDatas });

        var payload = new
        {
            request.ProjetoTarefaID,
            request.NmTarefa,
            request.DsTarefa,
            request.ProjetoTarefaStatusID,
            request.ProjetoTarefaPrioridadeID,
            request.UsuarioResponsavelID,
            request.DtInicio,
            request.DtPrevisaoFim,
            request.DtFimReal,
            UsuarioID = UsuarioLogadoID
        };

        try
        {
            var result = await apiClient.AtualizarTarefaAsync(payload, cancellationToken);

            if (result == 0)
                return StatusCode(500, new { mensagem = "Erro ao atualizar a tarefa." });

            return Ok(new { projetoTarefaId = result });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(ex.StatusCode is { } sc ? (int)sc : 500, new { mensagem = ex.Message });
        }
    }

    public sealed class EditarTarefaRequest
    {
        public int ProjetoTarefaID { get; set; }
        public int ProjetoID { get; set; }
        public string NmTarefa { get; set; } = string.Empty;
        public string? DsTarefa { get; set; }
        public int ProjetoTarefaStatusID { get; set; }
        public int ProjetoTarefaPrioridadeID { get; set; }
        public int? UsuarioResponsavelID { get; set; }
        public string? DtInicio { get; set; }
        public string? DtPrevisaoFim { get; set; }
        public string? DtFimReal { get; set; }
    }

    [HttpPost("ExcluirTarefa")]
    public async Task<IActionResult> ExcluirTarefa(
        [FromBody] ExcluirTarefaRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ProjetoTarefaID <= 0)
            return BadRequest(new { mensagem = "Tarefa inválida." });

        if (request.ProjetoID <= 0)
            return BadRequest(new { mensagem = "Projeto inválido." });

        if (!await PodeEditarProjetoAsync(request.ProjetoID, cancellationToken))
            return StatusCode(403, new { mensagem = "Você não tem permissão para gerenciar tarefas deste projeto." });

        try
        {
            var ok = await apiClient.ExcluirTarefaAsync(request.ProjetoTarefaID, UsuarioLogadoID, cancellationToken);

            if (!ok)
                return StatusCode(500, new { mensagem = "Erro ao excluir a tarefa." });

            return Ok(new { sucesso = true });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(ex.StatusCode is { } sc ? (int)sc : 500, new { mensagem = ex.Message });
        }
    }

    public sealed class ExcluirTarefaRequest
    {
        public int ProjetoTarefaID { get; set; }
        public int ProjetoID { get; set; }
    }

    // ── Participantes ────────────────────────────────────────

    [HttpGet("BuscarUsuarios")]
    public async Task<IActionResult> BuscarUsuarios(
        string texto = "",
        CancellationToken cancellationToken = default)
    {
        var result = await apiClient.BuscarUsuariosAsync(texto, cancellationToken);
        return Ok(result);
    }

    [HttpPost("AdicionarParticipante")]
    public async Task<IActionResult> AdicionarParticipante(
        [FromBody] AdicionarParticipanteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ProjetoID <= 0 || request.UsuarioID <= 0)
            return BadRequest(new { mensagem = "Parâmetros inválidos." });

        if (!await PodeEditarProjetoAsync(request.ProjetoID, cancellationToken))
            return StatusCode(403, new { mensagem = "Você não tem permissão para gerenciar participantes deste projeto." });

        var payload = new
        {
            request.ProjetoID,
            request.UsuarioID,
            request.NmPapel,
            UsuarioLogadoID = UsuarioLogadoID
        };

        try
        {
            var id = await apiClient.AdicionarParticipanteAsync(payload, cancellationToken);
            if (id == 0)
                return StatusCode(500, new { mensagem = "Erro ao adicionar o participante." });
            return Ok(new { projetoParticipanteId = id });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(ex.StatusCode is { } sc ? (int)sc : 500, new { mensagem = ex.Message });
        }
    }

    public sealed class AdicionarParticipanteRequest
    {
        public int ProjetoID { get; set; }
        public int UsuarioID { get; set; }
        public string NmPapel { get; set; } = string.Empty;
    }

    [HttpPost("AtualizarPapelParticipante")]
    public async Task<IActionResult> AtualizarPapelParticipante(
        [FromBody] AtualizarPapelRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ProjetoParticipanteID <= 0)
            return BadRequest(new { mensagem = "Participante inválido." });

        if (request.ProjetoID <= 0)
            return BadRequest(new { mensagem = "Projeto inválido." });

        if (!await PodeEditarProjetoAsync(request.ProjetoID, cancellationToken))
            return StatusCode(403, new { mensagem = "Você não tem permissão para gerenciar participantes deste projeto." });

        var payload = new
        {
            request.ProjetoParticipanteID,
            request.NmPapel,
            UsuarioLogadoID = UsuarioLogadoID
        };

        try
        {
            var id = await apiClient.AtualizarPapelParticipanteAsync(payload, cancellationToken);
            if (id == 0)
                return StatusCode(500, new { mensagem = "Erro ao atualizar o papel." });
            return Ok(new { projetoParticipanteId = id });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(ex.StatusCode is { } sc ? (int)sc : 500, new { mensagem = ex.Message });
        }
    }

    public sealed class AtualizarPapelRequest
    {
        public int ProjetoParticipanteID { get; set; }
        public int ProjetoID { get; set; }
        public string NmPapel { get; set; } = string.Empty;
    }

    [HttpPost("RemoverParticipante")]
    public async Task<IActionResult> RemoverParticipante(
        [FromBody] RemoverParticipanteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ProjetoParticipanteID <= 0)
            return BadRequest(new { mensagem = "Participante inválido." });

        if (request.ProjetoID <= 0)
            return BadRequest(new { mensagem = "Projeto inválido." });

        if (!await PodeEditarProjetoAsync(request.ProjetoID, cancellationToken))
            return StatusCode(403, new { mensagem = "Você não tem permissão para gerenciar participantes deste projeto." });

        try
        {
            var ok = await apiClient.RemoverParticipanteAsync(request.ProjetoParticipanteID, UsuarioLogadoID, cancellationToken);
            if (!ok)
                return StatusCode(500, new { mensagem = "Erro ao remover o participante." });
            return Ok(new { sucesso = true });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(ex.StatusCode is { } sc ? (int)sc : 500, new { mensagem = ex.Message });
        }
    }

    public sealed class RemoverParticipanteRequest
    {
        public int ProjetoParticipanteID { get; set; }
        public int ProjetoID { get; set; }
    }

    private static string? ValidarDatas(string? dtInicio, string? dtPrevisaoFim)
    {
        if (DateTime.TryParse(dtInicio, out var inicio) && DateTime.TryParse(dtPrevisaoFim, out var fim))
        {
            if (fim < inicio)
                return "A previsão de término não pode ser anterior à data de início.";
        }
        return null;
    }

    private async Task<bool> PodeEditarProjetoAsync(int projetoId, CancellationToken ct)
    {
        if (IsAdmin) return true;
        var projeto = await apiClient.GetProjetoDetalhesAsync(projetoId, ct);
        return projeto is not null && projeto.UsuarioCriadorID == UsuarioLogadoID;
    }
}
