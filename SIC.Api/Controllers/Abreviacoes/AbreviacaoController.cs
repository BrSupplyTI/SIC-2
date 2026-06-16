using Microsoft.AspNetCore.Mvc;
using SIC.Domain.Abstractions.Abreviacoes;

namespace SIC.Api.Controllers.Abreviacoes;

[ApiController]
[Route("api/abreviacoes")]
public sealed class AbreviacaoController(IAbreviacaoRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> BuscarDados(CancellationToken cancellationToken)
    {
        var dados = await repository.BuscarDadosAsync(cancellationToken);
        return Ok(dados);
    }

    [HttpPost]
    public async Task<IActionResult> Gravar([FromBody] GravarAbreviacaoRequest request, CancellationToken cancellationToken)
    {
        var ok = await repository.GravarAsync(request.Texto, request.Abreviacao, request.UsuarioId, cancellationToken);
        return Ok(new { Result = ok });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id, CancellationToken cancellationToken)
    {
        var ok = await repository.ExcluirAsync(id, cancellationToken);
        return Ok(new { Result = ok });
    }
}

public sealed record GravarAbreviacaoRequest(string Texto, string Abreviacao, int UsuarioId);
