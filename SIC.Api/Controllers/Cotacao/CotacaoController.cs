using Microsoft.AspNetCore.Mvc;
using SIC.Api.Contracts.Cotacao;
using SIC.Api.Services.Cotacao;

namespace SIC.Api.Controllers.Cotacao;

/// <summary>
/// Endpoints de consulta e escrita da Cotação (Proposta).
/// </summary>
[ApiController]
[Route("api/cotacao")]
public sealed class CotacaoController(
    ICotacaoQueryService queryService,
    ICotacaoCommandService commandService) : ControllerBase
{
    // -- Queries ---------------------------------------------------------------

    [HttpGet("{propostaId:int}/buscar-catalogo")]
    public async Task<IActionResult> BuscarCatalogo(
        int propostaId,
        [FromQuery] string descricao,
        [FromQuery] int clienteId,
        [FromQuery] int tblPrecoId,
        [FromQuery] int estabelecimentoId,
        CancellationToken cancellationToken)
    {
        var result = await queryService.BuscarCatalogoAsync(
            descricao, clienteId, tblPrecoId, estabelecimentoId, propostaId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("lista")]
    public async Task<IActionResult> GetLista(
        [FromQuery] int? usuarioId,
        [FromQuery] int filtroCotacao,
        [FromQuery] string? cdExtCliente,
        [FromQuery] int? propostaId,
        [FromQuery] string? cnpj,
        [FromQuery] int? estabelecimentoId,
        [FromQuery] int? statusId,
        [FromQuery] DateTime dataInicial,
        [FromQuery] DateTime dataFinal,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetListAsync(
            usuarioId, filtroCotacao, cdExtCliente, propostaId, cnpj,
            estabelecimentoId, statusId, dataInicial, dataFinal, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{propostaId:int}")]
    public async Task<IActionResult> GetDetalhe(int propostaId, CancellationToken cancellationToken)
    {
        var result = await queryService.GetByPropostaIdAsync(propostaId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{propostaId:int}/itens")]
    public async Task<IActionResult> GetItens(int propostaId, CancellationToken cancellationToken)
    {
        var result = await queryService.GetItensByPropostaIdAsync(propostaId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("options/estabelecimentos")]
    public async Task<IActionResult> GetEstabelecimentoOptions(CancellationToken cancellationToken)
    {
        var result = await queryService.GetEstabelecimentoOptionsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("options/status")]
    public async Task<IActionResult> GetStatusOptions(CancellationToken cancellationToken)
    {
        var result = await queryService.GetStatusOptionsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("options/condicoes-pagamento")]
    public async Task<IActionResult> GetCondicoesPagamento(
        [FromQuery] int estabelecimentoId,
        [FromQuery] decimal valorTotal,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetCondicoesPagamentoAsync(
            estabelecimentoId, valorTotal, cancellationToken);
        return Ok(result);
    }

    [HttpGet("options/formas-pagamento")]
    public async Task<IActionResult> GetFormasPagamento(CancellationToken cancellationToken)
    {
        var result = await queryService.GetFormasPagamentoAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("options/tipos-cotacao")]
    public async Task<IActionResult> GetTiposCotacao(
        [FromQuery] int usuarioId,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetTiposCotacaoAsync(usuarioId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("options/motivos-bonificacao")]
    public async Task<IActionResult> GetMotivosBonificacao(
        [FromQuery] int usuarioId,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetMotivosBonificacaoAsync(usuarioId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("options/estabelecimentos-add")]
    public async Task<IActionResult> GetEstabelecimentos(CancellationToken cancellationToken)
    {
        var result = await queryService.GetEstabelecimentosAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("options/ufs")]
    public async Task<IActionResult> GetUfs(CancellationToken cancellationToken)
    {
        var result = await queryService.GetUfsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("options/cidades")]
    public async Task<IActionResult> GetCidadesByUf(
        [FromQuery] string cdUf,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetCidadesByUfAsync(cdUf, cancellationToken);
        return Ok(result);
    }

    [HttpGet("options/tipos-ordem")]
    public async Task<IActionResult> GetTiposOrdem(
        [FromQuery] int cotacaoTipoId,
        [FromQuery] int usuarioId,
        CancellationToken cancellationToken)
    {
        var result = await queryService.GetTiposOrdemAsync(cotacaoTipoId, usuarioId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("clientes/buscar")]
    public async Task<IActionResult> SearchClientes(
        [FromQuery] string termo,
        [FromQuery] int estabelecimentoId,
        CancellationToken cancellationToken)
    {
        var result = await queryService.SearchClientesAsync(termo, estabelecimentoId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("clientes/{clienteId:int}/enderecos")]
    public async Task<IActionResult> GetEnderecosByCliente(
        int clienteId, CancellationToken cancellationToken)
    {
        var result = await queryService.GetEnderecosByClienteAsync(clienteId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("clientes/{clienteId:int}/tabela-preco")]
    public async Task<IActionResult> GetTabelaPrecoByCliente(
        int clienteId, CancellationToken cancellationToken)
    {
        var result = await queryService.GetTabelaPrecoByClienteAsync(clienteId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("clientes/{clienteId:int}/forma-pagamento")]
    public async Task<IActionResult> GetFormaPagamentoByCliente(
        int clienteId, CancellationToken cancellationToken)
    {
        var result = await queryService.GetFormaPagamentoByClienteAsync(clienteId, cancellationToken);
        return Ok(new { FormaPagamentoSAP = result });
    }

    [HttpGet("clientes/{clienteId:int}/contratos")]
    public async Task<IActionResult> GetContratosByCliente(
        int clienteId, CancellationToken cancellationToken)
    {
        var result = await queryService.GetContratosAsync(clienteId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("enderecos/{clienteEnderecoId:int}/locais-entrega")]
    public async Task<IActionResult> GetLocaisEntregaByEndereco(
        int clienteEnderecoId, CancellationToken cancellationToken)
    {
        var result = await queryService.GetLocaisEntregaByEnderecoAsync(clienteEnderecoId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("enderecos/{clienteEnderecoId:int}/tipo-ovsap")]
    public async Task<IActionResult> GetTipoOVSAPByEndereco(
        int clienteEnderecoId, CancellationToken cancellationToken)
    {
        var result = await queryService.GetTipoOVSAPByEnderecoAsync(clienteEnderecoId, cancellationToken);
        return Ok(new { TipoOVSAP = result });
    }

    [HttpGet("enderecos/{clienteEnderecoId:int}/frete-inicial")]
    public async Task<IActionResult> BuscarFreteInicial(
        int clienteEnderecoId,
        [FromQuery] int clienteId,
        [FromQuery] string? ufDestino,
        CancellationToken cancellationToken)
    {
        var result = await queryService.BuscarFreteInicialAsync(
            clienteEnderecoId, clienteId, ufDestino, cancellationToken);
        return Ok(result);
    }

    [HttpGet("clientes/{clienteId:int}/executivo-vendas")]
    public async Task<IActionResult> GetExecutivoVendas(
        int clienteId, CancellationToken cancellationToken)
    {
        var result = await queryService.GetExecutivoVendasAsync(clienteId, cancellationToken);
        return Ok(new { Executivo = result });
    }

    [HttpGet("{propostaId:int}/calcular-frete")]
    public async Task<IActionResult> CalcularFreteProposta(
        int propostaId, CancellationToken cancellationToken)
    {
        var result = await queryService.CalcularFretePropostaAsync(propostaId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{propostaId:int}/itens/{propostaItemId:int}/impostos")]
    public async Task<IActionResult> GetImpostosItem(
        int propostaId, int propostaItemId, CancellationToken cancellationToken)
    {
        var result = await queryService.GetImpostosItemAsync(propostaItemId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{propostaId:int}/validar-itens-importacao")]
    public async Task<IActionResult> ValidarItensImportacao(
        int propostaId, CancellationToken cancellationToken)
    {
        var result = await queryService.ValidarItensImportacaoAsync(propostaId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{propostaId:int}/email-dados")]
    public async Task<IActionResult> GetEmailDados(
        int propostaId, CancellationToken cancellationToken)
    {
        var result = await queryService.GetEnviarEmailDadosAsync(propostaId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{propostaId:int}/historico-envios")]
    public async Task<IActionResult> GetHistoricoEnvios(
        int propostaId, CancellationToken cancellationToken)
    {
        var result = await queryService.GetHistoricoEnviosAsync(propostaId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{propostaId:int}/edit-dados")]
    public async Task<IActionResult> GetPropostaParaEdit(
        int propostaId, CancellationToken cancellationToken)
    {
        var result = await queryService.GetPropostaParaEditAsync(propostaId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{propostaId:int}/email-template")]
    public async Task<IActionResult> GetEmailTemplate(
        int propostaId, CancellationToken cancellationToken)
    {
        var result = await queryService.GetDadosEmailTemplateAsync(propostaId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    // -- Commands --------------------------------------------------------------

    [HttpPost]
    public async Task<IActionResult> CriarProposta(
        [FromBody] CriarPropostaRequest request,
        CancellationToken cancellationToken)
    {
        var propostaId = await commandService.CriarPropostaAsync(request, cancellationToken);
        return Ok(new { PropostaId = propostaId });
    }

    [HttpPut("{propostaId:int}")]
    public async Task<IActionResult> AtualizarProposta(
        int propostaId,
        [FromBody] AtualizarPropostaRequest request,
        CancellationToken cancellationToken)
    {
        await commandService.AtualizarPropostaAsync(propostaId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{propostaId:int}/itens/adicionar")]
    public async Task<IActionResult> AdicionarItem(
        int propostaId,
        [FromBody] AdicionarItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AdicionarItemAsync(
            propostaId,
            request.CodItemBR, request.DescrItemBR, request.TipoCusto,
            request.PrecoItem, request.VlrCustoAquisicao, request.VlrCustoMedio,
            request.Quantidade, request.VlrPrecoMinimo, request.VlrTabelaPreco,
            cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record AdicionarItemRequest(
        string CodItemBR, string DescrItemBR, string TipoCusto,
        decimal PrecoItem, decimal VlrCustoAquisicao, decimal VlrCustoMedio,
        int Quantidade, decimal VlrPrecoMinimo, decimal VlrTabelaPreco);

    [HttpPost("{propostaId:int}/itens/{propostaItemId:int}/calcular-margem")]
    public async Task<IActionResult> CalcularMargemItem(
        int propostaId, int propostaItemId,
        [FromBody] CalcularMargemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.CalcularMargemItemAsync(
            propostaId, propostaItemId, request.Type, request.ViaTela, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record CalcularMargemRequest(string Type, string ViaTela);

    [HttpPost("{propostaId:int}/itens/{propostaItemId:int}/atualizar")]
    public async Task<IActionResult> AtualizarItem(
        int propostaId, int propostaItemId,
        [FromBody] AtualizarItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AtualizarItemAsync(
            propostaId, propostaItemId, request.PrecoUnitario, request.Quantidade, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record AtualizarItemRequest(decimal PrecoUnitario, decimal Quantidade);

    [HttpPost("{propostaId:int}/itens/{propostaItemId:int}/atualizar-custo")]
    public async Task<IActionResult> AtualizarCustoItem(
        int propostaId, int propostaItemId,
        [FromBody] AtualizarCustoItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AtualizarCustoItemAsync(
            propostaId, propostaItemId, request.TipoCusto, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record AtualizarCustoItemRequest(string TipoCusto);

    [HttpPost("{propostaId:int}/gerar-itens")]
    public async Task<IActionResult> GerarItens(
        int propostaId,
        [FromBody] GerarItensRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.GerarItensAsync(
            propostaId, request.TipoGeracao, request.UsuarioID, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record GerarItensRequest(string TipoGeracao, int UsuarioID);

    public sealed record RemoverItensRequest(List<RemoverItemInfo> Itens, string Motivo, int UsuarioId);
    public sealed record RemoverItemInfo(int PropostaItemId, string CdItem);

    [HttpPost("{propostaId:int}/itens/remover")]
    public async Task<IActionResult> RemoverItens(
        int propostaId,
        [FromBody] RemoverItensRequest request,
        CancellationToken cancellationToken)
    {
        var itens = request.Itens.Select(i => (i.PropostaItemId, i.CdItem)).ToList();
        var result = await commandService.RemoverItensAsync(
            propostaId, itens, request.Motivo, request.UsuarioId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record SalvarCondPagtoRequest(int CondPagtoId);

    [HttpPost("{propostaId:int}/salvar-cond-pagto")]
    public async Task<IActionResult> SalvarCondPagto(
        int propostaId,
        [FromBody] SalvarCondPagtoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.SalvarCondPagtoAsync(
            propostaId, request.CondPagtoId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{propostaId:int}/recalcular-margem-bruta")]
    public async Task<IActionResult> RecalcularMargemBruta(
        int propostaId, CancellationToken cancellationToken)
    {
        var result = await commandService.RecalcularMargemBrutaPropostaAsync(propostaId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{propostaId:int}/finalizar")]
    public async Task<IActionResult> Finalizar(
        int propostaId,
        [FromBody] FinalizarRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.FinalizarAsync(
            propostaId, request.DataValidade, request.UsuarioId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record FinalizarRequest(string DataValidade, int UsuarioId);

    [HttpPost("{propostaId:int}/aprovar")]
    public async Task<IActionResult> Aprovar(
        int propostaId,
        [FromBody] AprovacaoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AprovarAsync(propostaId, request.AprovadorId, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{propostaId:int}/reprovar")]
    public async Task<IActionResult> Reprovar(
        int propostaId,
        [FromBody] ReprovarRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.ReprovarAsync(
            propostaId, request.AprovadorId, request.Justificativa, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record AprovacaoRequest(int AprovadorId);
    public sealed record ReprovarRequest(int AprovadorId, string Justificativa);

    [HttpPost("{propostaId:int}/salvar-frete")]
    public async Task<IActionResult> SalvarFrete(
        int propostaId,
        [FromBody] SalvarFreteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.SalvarFretePropostaAsync(
            propostaId, request.TransportadoraId, request.ValorFrete, request.PrazoTotal, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record SalvarFreteRequest(int TransportadoraId, decimal ValorFrete, int PrazoTotal);

    [HttpPost("{propostaId:int}/autorizar-faturamento")]
    public async Task<IActionResult> AutorizarFaturamento(
        int propostaId,
        [FromBody] AutorizarFaturamentoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandService.AutorizarFaturamentoAsync(
            propostaId, request.IpAprovacao, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    public sealed record AutorizarFaturamentoRequest(string IpAprovacao);

    [HttpPost("enderecos/{clienteEnderecoId:int}/ensure-locais-entrega")]
    public async Task<IActionResult> EnsureLocaisEntrega(
        int clienteEnderecoId, CancellationToken cancellationToken)
    {
        var result = await commandService.EnsureLocaisEntregaAsync(clienteEnderecoId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{propostaId:int}/salvar-log-envio")]
    public async Task<IActionResult> SalvarLogEnvio(
        int propostaId,
        [FromBody] SalvarLogEnvioRequest request,
        CancellationToken cancellationToken)
    {
        request.PropostaId = propostaId;
        await commandService.SalvarLogEnvioAsync(request, cancellationToken);
        return NoContent();
    }
}
