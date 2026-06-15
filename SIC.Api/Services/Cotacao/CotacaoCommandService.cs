using SIC.Api.Contracts.Cotacao;
using SIC.Api.Models.Auth;
using SIC.Domain.Abstractions.Cotacao;
using DomainEntities = SIC.Domain.Entities.Cotacao;

namespace SIC.Api.Services.Cotacao;

/// <summary>
/// Implementação das operações de escrita da Cotação.
/// </summary>
public sealed class CotacaoCommandService(ICotacaoCommandRepository repository) : ICotacaoCommandService
{
    public async Task<OperationResult> AdicionarItemAsync(
        int propostaId,
        string codItemBR,
        string descrItemBR,
        string tipoCusto,
        decimal precoItem,
        decimal vlrCustoAquisicao,
        decimal vlrCustoMedio,
        int quantidade,
        decimal vlrPrecoMinimo,
        decimal vlrTabelaPreco,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.AdicionarItemAsync(
            propostaId,
            codItemBR,
            descrItemBR,
            tipoCusto,
            precoItem,
            vlrCustoAquisicao,
            vlrCustoMedio,
            quantidade,
            vlrPrecoMinimo,
            vlrTabelaPreco,
            cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Item adicionado com sucesso." : (error ?? "Erro ao adicionar item à cotação.")
        };
    }

    public async Task<OperationResult> CalcularMargemItemAsync(
        int propostaId,
        int propostaItemId,
        string type,
        string viaTela,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.CalcularMargemItemAsync(
            propostaId,
            propostaItemId,
            type,
            viaTela,
            cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Margem calculada com sucesso." : (error ?? "Erro ao calcular margem.")
        };
    }

    public async Task<OperationResult> AtualizarItemAsync(
        int propostaId,
        int propostaItemId,
        decimal precoUnitario,
        decimal quantidade,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.AtualizarItemAsync(
            propostaId,
            propostaItemId,
            precoUnitario,
            quantidade,
            cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Item atualizado com sucesso." : (error ?? "Erro ao atualizar item.")
        };
    }
    public async Task<OperationResult> AtualizarCustoItemAsync(
        int propostaId,
        int propostaItemId,
        string tipoCusto,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.AtualizarCustoItemAsync(
            propostaId,
            propostaItemId,
            tipoCusto,
            cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Custo atualizado com sucesso." : (error ?? "Erro ao atualizar custo do item.")
        };
    }
    public async Task<OperationResult> GerarItensAsync(
        int propostaId,
        string tipoGeracao,
        int usuarioId,
        string? cotacaoId = null,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.GerarItensAsync(
            propostaId,
            tipoGeracao,
            usuarioId,
            cotacaoId,
            cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Itens gerados com sucesso." : (error ?? "Erro ao gerar itens.")
        };
    }

    public async Task<OperationResult> RemoverItensAsync(
        int propostaId,
        IReadOnlyList<(int PropostaItemId, string CdItem)> itens,
        string motivo,
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.RemoverItensAsync(
            propostaId, itens, motivo, usuarioId, cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Item(ns) removido(s) com sucesso." : (error ?? "Erro ao remover item(ns).")
        };
    }

    public async Task<OperationResult> SalvarCondPagtoAsync(
        int propostaId,
        int condPagtoId,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.SalvarCondPagtoAsync(
            propostaId, condPagtoId, cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Condição de pagamento salva com sucesso." : (error ?? "Erro ao salvar condição de pagamento.")
        };
    }

    public async Task<OperationResult> RecalcularMargemBrutaPropostaAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.RecalcularMargemBrutaPropostaAsync(
            propostaId, cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Margem bruta recalculada com sucesso." : (error ?? "Erro ao recalcular margem bruta.")
        };
    }

    public async Task<OperationResult> FinalizarAsync(
        int propostaId, string dataValidade, int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var (success, statusId, error) = await repository.FinalizarAsync(
            propostaId, dataValidade, usuarioId, cancellationToken);

        return new OperationResult
        {
            Success = success,
            Message = success ? "Cotação finalizada com sucesso." : (error ?? "Erro ao finalizar cotação."),
            ResetToken = statusId?.ToString()
        };
    }

    public async Task<OperationResult> AprovarAsync(
        int propostaId, int aprovadorId,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.AprovarAsync(propostaId, aprovadorId, cancellationToken);
        return new OperationResult
        {
            Success = success,
            Message = success ? "Cotação aprovada com sucesso." : (error ?? "Erro ao aprovar cotação.")
        };
    }

    public async Task<OperationResult> ReprovarAsync(
        int propostaId, int aprovadorId, string justificativa,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.ReprovarAsync(
            propostaId, aprovadorId, justificativa, cancellationToken);
        return new OperationResult
        {
            Success = success,
            Message = success ? "Cotação reprovada com sucesso." : (error ?? "Erro ao reprovar cotação.")
        };
    }

    public async Task<OperationResult> SalvarFretePropostaAsync(
        int propostaId, int transportadoraId, decimal valorFrete, int prazoTotal,
        CancellationToken cancellationToken = default)
    {
        var (success, error) = await repository.SalvarFretePropostaAsync(
            propostaId, transportadoraId, valorFrete, prazoTotal, cancellationToken);
        return new OperationResult
        {
            Success = success,
            Message = success ? "Frete salvo com sucesso." : (error ?? "Erro ao salvar frete.")
        };
    }

    public async Task<OperationResult> AutorizarFaturamentoAsync(
        int propostaId, string ipAprovacao,
        CancellationToken cancellationToken = default)
    {
        var (success, cotacaoId, error) = await repository.AutorizarFaturamentoAsync(
            propostaId, ipAprovacao, cancellationToken);
        return new OperationResult
        {
            Success = success,
            Message = success ? "Faturamento autorizado com sucesso." : (error ?? "Erro ao autorizar faturamento."),
            ResetToken = cotacaoId?.ToString()
        };
    }

    public Task<int> CriarPropostaAsync(
        CriarPropostaRequest request,
        CancellationToken cancellationToken = default)
    {
        var domainRequest = MapCriarProposta(request);
        return repository.CriarPropostaAsync(domainRequest, cancellationToken);
    }

    public Task AtualizarPropostaAsync(
        int propostaId, AtualizarPropostaRequest request,
        CancellationToken cancellationToken = default)
    {
        var domainRequest = new DomainEntities.CriarPropostaRequest
        {
            Nome = request.Nome,
            TipoID = request.TipoID,
            TipoNome = request.TipoNome,
            EstabelecimentoID = request.EstabelecimentoID,
            ClienteId = request.ClienteId,
            ClienteEnderecoID = request.ClienteEnderecoID,
            ClienteLocalEntregaID = request.ClienteLocalEntregaID,
            ObsLocalEntrega = request.ObsLocalEntrega,
            TabelaPrecoID = request.TabelaPrecoID,
            FlagPrecoConformeTabela = request.FlagPrecoConformeTabela,
            UfOrigem = request.UfOrigem,
            UfDestino = request.UfDestino,
            CodigoIBGE = request.CodigoIBGE,
            MargemPadrao = request.MargemPadrao,
            DataValidade = request.DataValidade,
            CondPagtoId = request.CondPagtoId,
            FormaPagamentoSAP = request.FormaPagamentoSAP,
            TipoOVSAP = request.TipoOVSAP,
            OrdemCompra = request.OrdemCompra,
            NrContrato = request.NrContrato,
            TipoMotivoIDSAP = request.TipoMotivoIDSAP,
            NrChamado = request.NrChamado,
            PedidoOriginalID = request.PedidoOriginalID,
            ContatoNome = request.ContatoNome,
            ContatoEmail = request.ContatoEmail,
            Obs = request.Obs,
            UsuarioId = request.UsuarioId,
            ValorVendaTotal = request.ValorVendaTotal,
            Frete = request.Frete,
            VlrPedidoMinimo = request.VlrPedidoMinimo,
        };
        return repository.AtualizarPropostaAsync(propostaId, domainRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<CotacaoLocalEntregaOptionDto>> EnsureLocaisEntregaAsync(
        int clienteEnderecoId, CancellationToken cancellationToken = default)
    {
        var items = await repository.EnsureLocaisEntregaAsync(clienteEnderecoId, cancellationToken);
        return items.Select(i => new CotacaoLocalEntregaOptionDto
        {
            ClienteLocalEntregaId = i.ClienteLocalEntregaId,
            Text = i.Text,
            Logradouro = i.Logradouro,
            CdUF = i.CdUF,
            Cidade = i.Cidade,
            FlagEnderecoDiferente = i.FlagEnderecoDiferente,
            CdControle = i.CdControle,
            ObsLocalEntrega = i.ObsLocalEntrega,
            TipoOVSAP = i.TipoOVSAP,
            CondPagtoId = i.CondPagtoId,
        }).ToList();
    }

    public Task SalvarLogEnvioAsync(
        SalvarLogEnvioRequest request,
        CancellationToken cancellationToken = default)
    {
        var domainRequest = new DomainEntities.SalvarLogEnvioRequest
        {
            PropostaId = request.PropostaId,
            Nome = request.Nome,
            Email = request.Email,
            Saudacao = request.Saudacao,
            Mensagem = request.Mensagem,
            ComCopia = request.ComCopia,
            Hash = request.Hash,
            UsuarioId = request.UsuarioId,
            PodeDispEstoque = request.PodeDispEstoque,
            PodeAltTransportadora = request.PodeAltTransportadora,
            PodeAltCondPagamento = request.PodeAltCondPagamento,
            PodeNegociar = request.PodeNegociar,
        };
        return repository.SalvarLogEnvioAsync(domainRequest, cancellationToken);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static DomainEntities.CriarPropostaRequest MapCriarProposta(CriarPropostaRequest r) =>
        new()
        {
            Nome = r.Nome,
            TipoID = r.TipoID,
            TipoNome = r.TipoNome,
            EstabelecimentoID = r.EstabelecimentoID,
            ClienteId = r.ClienteId,
            ClienteEnderecoID = r.ClienteEnderecoID,
            ClienteLocalEntregaID = r.ClienteLocalEntregaID,
            ObsLocalEntrega = r.ObsLocalEntrega,
            TabelaPrecoID = r.TabelaPrecoID,
            FlagPrecoConformeTabela = r.FlagPrecoConformeTabela,
            UfOrigem = r.UfOrigem,
            UfDestino = r.UfDestino,
            CodigoIBGE = r.CodigoIBGE,
            MargemPadrao = r.MargemPadrao,
            DataValidade = r.DataValidade,
            CondPagtoId = r.CondPagtoId,
            FormaPagamentoSAP = r.FormaPagamentoSAP,
            TipoOVSAP = r.TipoOVSAP,
            OrdemCompra = r.OrdemCompra,
            NrContrato = r.NrContrato,
            TipoMotivoIDSAP = r.TipoMotivoIDSAP,
            NrChamado = r.NrChamado,
            PedidoOriginalID = r.PedidoOriginalID,
            ContatoNome = r.ContatoNome,
            ContatoEmail = r.ContatoEmail,
            Obs = r.Obs,
            UsuarioId = r.UsuarioId,
            ValorVendaTotal = r.ValorVendaTotal,
            Frete = r.Frete,
            VlrPedidoMinimo = r.VlrPedidoMinimo,
        };
}
