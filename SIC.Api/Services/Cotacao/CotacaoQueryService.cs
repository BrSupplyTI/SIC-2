using SIC.Api.Contracts.Cotacao;
using SIC.Domain.Abstractions.Cotacao;

namespace SIC.Api.Services.Cotacao;

/// <summary>
/// Implementação das operações de leitura da Cotação.
/// </summary>
public sealed class CotacaoQueryService(ICotacaoQueryRepository repository) : ICotacaoQueryService
{
    public async Task<IReadOnlyList<CotacaoCatalogoItemDto>> BuscarCatalogoAsync(
        string descricao,
        int clienteId,
        int tblPrecoId,
        int estabelecimentoId,
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        var items = await repository.BuscarCatalogoAsync(
            descricao, clienteId, tblPrecoId, estabelecimentoId, propostaId, cancellationToken);

        return items.Select(i => new CotacaoCatalogoItemDto
        {
            ItemID = i.ItemID,
            CdItem = i.CdItem,
            NmItem = i.NmItem,
            SegmentoID = i.SegmentoID,
            NmSegmento = i.NmSegmento,
            FamiliaID = i.FamiliaID,
            NmFamilia = i.NmFamilia,
            SubFamiliaID = i.SubFamiliaID,
            NmSubFamilia = i.NmSubFamilia,
            EstabelecimentoID = i.EstabelecimentoID,
            Curva = i.Curva,
            QtdDisponivel = i.QtdDisponivel,
            QtEstoqueSIC = i.QtEstoqueSIC,
            Ativo = i.Ativo,
            VlrCustoAquisicao = i.VlrCustoAquisicao,
            VlrCustoMedio = i.VlrCustoMedio,
            VlrTabela = i.VlrTabela,
            VlrPrecoMinimo = i.VlrPrecoMinimo,
            Criticidade = i.Criticidade,
            TabelaPreco = i.TabelaPreco,
        }).ToList();
    }

    public async Task<IReadOnlyList<CotacaoListItemDto>> GetListAsync(
        int? usuarioId, int filtroCotacao, string? cdExtCliente, int? propostaId,
        string? cnpj, int? estabelecimentoId, int? statusId,
        DateTime dataInicial, DateTime dataFinal,
        CancellationToken cancellationToken = default)
    {
        var items = await repository.GetListAsync(
            usuarioId, filtroCotacao, cdExtCliente, propostaId, cnpj,
            estabelecimentoId, statusId, dataInicial, dataFinal, cancellationToken);

        return items.Select(i => new CotacaoListItemDto
        {
            CdExtCliente = i.CdExtCliente,
            PropostaId = i.PropostaId,
            CdProposta = i.CdProposta,
            Nome = i.Nome,
            DtCriacao = i.DtCriacao,
            ClienteId = i.ClienteId,
            ClienteNome = i.ClienteNome,
            ClienteCNPJ = i.ClienteCNPJ,
            MargemPadrao = i.MargemPadrao,
            Frete = i.Frete,
            DataValidade = i.DataValidade,
            DataValidadeSQL = i.DataValidadeSQL,
            StatusID = i.StatusID,
            StatusName = i.StatusName,
            Obs = i.Obs,
            NmMotivo = i.NmMotivo,
            Justificativa = i.Justificativa,
            CotacaoID = i.CotacaoID,
            CotacaoStatusID = i.CotacaoStatusID,
            CotacaoStatus = i.CotacaoStatus,
            TotalVenda = i.TotalVenda,
            TipoCotacao = i.TipoCotacao,
            NmCondPagto = i.NmCondPagto,
            Endereco = i.Endereco,
            QtdItens = i.QtdItens,
            EstabelecimentoID = i.EstabelecimentoID,
            NmEstabelecimento = i.NmEstabelecimento,
            DataAbertura = i.DataAbertura,
            DataAberturaSQL = i.DataAberturaSQL,
            Executivo = i.Executivo,
            AprovadorNmUsuario = i.AprovadorNmUsuario,
        }).ToList();
    }

    public async Task<CotacaoDetalheDto?> GetByPropostaIdAsync(
        int propostaId, CancellationToken cancellationToken = default)
    {
        var d = await repository.GetByPropostaIdAsync(propostaId, cancellationToken);
        if (d is null) return null;

        return new CotacaoDetalheDto
        {
            PropostaID = d.PropostaID,
            CdProposta = d.CdProposta,
            Nome = d.Nome,
            Versao = d.Versao,
            OrdemCompra = d.OrdemCompra,
            StatusID = d.StatusID,
            StatusNome = d.StatusNome,
            TipoCotacao = d.TipoCotacao,
            DataValidade = d.DataValidade,
            EstabelecimentoID = d.EstabelecimentoID,
            EstabelecimentoNome = d.EstabelecimentoNome,
            EstabelecimentoCNPJ = d.EstabelecimentoCNPJ,
            EstabelecimentoRazaoSocial = d.EstabelecimentoRazaoSocial,
            ClienteID = d.ClienteID,
            ClienteCodigo = d.ClienteCodigo,
            ClienteNome = d.ClienteNome,
            ClienteCodNome = d.ClienteCodNome,
            ClienteCNPJ = d.ClienteCNPJ,
            ClienteContribuinte = d.ClienteContribuinte,
            EhContribuinte = d.EhContribuinte,
            ClienteEnderecoID = d.ClienteEnderecoID,
            ClienteEndereco = d.ClienteEndereco,
            ClienteCidadeEstado = d.ClienteCidadeEstado,
            ClienteLocalEntregaID = d.ClienteLocalEntregaID,
            LocalEntregaNome = d.LocalEntregaNome,
            LocalEntregaEndereco = d.LocalEntregaEndereco,
            LocalEntregaCidadeEstado = d.LocalEntregaCidadeEstado,
            LocalEntregaObservacao = d.LocalEntregaObservacao,
            CanalVenda = d.CanalVenda,
            TipoOrdem = d.TipoOrdem,
            TipoOVSAP = d.TipoOVSAP,
            TipoOVEhRevenda = d.TipoOVEhRevenda,
            TipoMotivoIDSAP = d.TipoMotivoIDSAP,
            Motivo = d.Motivo,
            MotivoNome = d.MotivoNome,
            Justificativa = d.Justificativa,
            AprovadorUsuarioID = d.AprovadorUsuarioID,
            AprovadorNome = d.AprovadorNome,
            AprovadorJustificativa = d.AprovadorJustificativa,
            CondPagtoID = d.CondPagtoID,
            CondPagtoNome = d.CondPagtoNome,
            FormaPagamentoSAP = d.FormaPagamentoSAP,
            FormaPagamentoDesc = d.FormaPagamentoDesc,
            FlagDefCondPagTelevendas = d.FlagDefCondPagTelevendas,
            TabelaPrecoID = d.TabelaPrecoID,
            TabelaPrecoNome = d.TabelaPrecoNome,
            FlagPrecoConformeTabela = d.FlagPrecoConformeTabela,
            MargemPadrao = d.MargemPadrao,
            MargemBruta = d.MargemBruta,
            MargemContribuida = d.MargemContribuida,
            MargemBrutaFixa = d.MargemBrutaFixa,
            MargemContribuidaFixa = d.MargemContribuidaFixa,
            Frete = d.Frete,
            ValorVendaTotal = d.ValorVendaTotal,
            VlrContribTotal = d.VlrContribTotal,
            ValorContribuicaoFixo = d.ValorContribuicaoFixo,
            ValorTotalFixo = d.ValorTotalFixo,
            VlrPedidoMinimo = d.VlrPedidoMinimo,
            TotalVenda = d.TotalVenda,
            TotalVendaFrete = d.TotalVendaFrete,
            TotalVendaSemImposto = d.TotalVendaSemImposto,
            TotalVendaFreteSemImposto = d.TotalVendaFreteSemImposto,
            TotalPeso = d.TotalPeso,
            QtdItens = d.QtdItens,
            DiasPrazoEntrega = d.DiasPrazoEntrega,
            DataProgEntrega = d.DataProgEntrega,
            NatOperacao = d.NatOperacao,
            UfOrigem = d.UfOrigem,
            UfDestino = d.UfDestino,
            CodigoIBGE = d.CodigoIBGE,
            ContatoNome = d.ContatoNome,
            ContatoEmail = d.ContatoEmail,
            TransportadoraID = d.TransportadoraID,
            TransportadoraNome = d.TransportadoraNome,
            CotacaoID = d.CotacaoID,
            CotacaoIdOriginal = d.CotacaoIdOriginal,
            CotacaoStatusDesc = d.CotacaoStatusDesc,
            CotacaoEnvioComentarios = d.CotacaoEnvioComentarios,
            FlagRevisarValorProdutos = d.FlagRevisarValorProdutos,
            FlagRevisarValorFrete = d.FlagRevisarValorFrete,
            FlagRevisarPrazoPagamento = d.FlagRevisarPrazoPagamento,
            FlagRevisarPrazoEntrega = d.FlagRevisarPrazoEntrega,
            FlagRevisarAtendimento = d.FlagRevisarAtendimento,
            FlagRevisarPermiteTrocarMarca = d.FlagRevisarPermiteTrocarMarca,
            FlagRevisarPermiteTrocarUnidade = d.FlagRevisarPermiteTrocarUnidade,
            FlagPrecosInformados = d.FlagPrecosInformados,
            CotacaoEnvioIPAprovacao = d.CotacaoEnvioIPAprovacao,
            ConsultorUsuarioID = d.ConsultorUsuarioID,
            ConsultorNome = d.ConsultorNome,
            ConsultorEmail = d.ConsultorEmail,
            CarteiraNome = d.CarteiraNome,
            Observacao = d.Observacao,
            Obs = d.Obs,
            StatusCredito = d.StatusCredito,
            FlagPrecisaAprovacao = d.FlagPrecisaAprovacao,
            PercMargemMinPedido = d.PercMargemMinPedido,
            PercMargemMaxPedido = d.PercMargemMaxPedido,
            AtendenteAprovadorID = d.AtendenteAprovadorID,
            AtendenteAprovadorNome = d.AtendenteAprovadorNome,
            Itens = d.Itens.Select(MapItem).ToList(),
        };
    }

    public async Task<IReadOnlyList<CotacaoDetalheItemDto>> GetItensByPropostaIdAsync(
        int propostaId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetItensByPropostaIdAsync(propostaId, cancellationToken);
        return items.Select(MapItem).ToList();
    }

    public async Task<IReadOnlyList<CotacaoSelectOptionDto>> GetEstabelecimentoOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await repository.GetEstabelecimentoOptionsAsync(cancellationToken);
        return items.Select(i => new CotacaoSelectOptionDto { Id = i.Id, Nome = i.Nome }).ToList();
    }

    public async Task<IReadOnlyList<CotacaoSelectOptionDto>> GetStatusOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await repository.GetStatusOptionsAsync(cancellationToken);
        return items.Select(i => new CotacaoSelectOptionDto { Id = i.Id, Nome = i.Nome }).ToList();
    }

    public async Task<IReadOnlyList<CotacaoSelectOptionDto>> GetCondicoesPagamentoAsync(
        int estabelecimentoId, decimal valorTotal, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetCondicoesPagamentoAsync(estabelecimentoId, valorTotal, cancellationToken);
        return items.Select(i => new CotacaoSelectOptionDto { Id = i.Id, Nome = i.Nome }).ToList();
    }

    public Task<string> GetExecutivoVendasAsync(
        int clienteId, CancellationToken cancellationToken = default)
        => repository.GetExecutivoVendasAsync(clienteId, cancellationToken);

    public async Task<IReadOnlyList<CotacaoFreteOpcaoDto>> CalcularFretePropostaAsync(
        int propostaId, CancellationToken cancellationToken = default)
    {
        var items = await repository.CalcularFretePropostaAsync(propostaId, cancellationToken);
        return items.Select(i => new CotacaoFreteOpcaoDto
        {
            TransportadoraID = i.TransportadoraID,
            Nome = i.Nome,
            TempoLogistico = i.TempoLogistico,
            TempoComercial = i.TempoComercial,
            TaxaExtra = i.TaxaExtra,
            ValorFrete = i.ValorFrete,
            QtItensRestritos = i.QtItensRestritos,
            FlagObrigatoriaCanalVenda = i.FlagObrigatoriaCanalVenda,
            FlagClienteRestrito = i.FlagClienteRestrito,
            FlagClienteFixo = i.FlagClienteFixo,
        }).ToList();
    }

    public async Task<CotacaoItemImpostosDto?> GetImpostosItemAsync(
        int propostaItemId, CancellationToken cancellationToken = default)
    {
        var i = await repository.GetImpostosItemAsync(propostaItemId, cancellationToken);
        if (i is null) return null;
        return new CotacaoItemImpostosDto
        {
            CodItemBR = i.CodItemBR,
            MB = i.MB,
            VlrLiqUnit = i.VlrLiqUnit,
            PercICMS = i.PercICMS,
            VlrICMS = i.VlrICMS,
            PercIPI = i.PercIPI,
            VlrIPI = i.VlrIPI,
            VlrFCP = i.VlrFCP,
            PercPIS = i.PercPIS,
            VlrPIS = i.VlrPIS,
            PercCOFINS = i.PercCOFINS,
            VlrCOFINS = i.VlrCOFINS,
            MVA = i.MVA,
            ST = i.ST,
            VlrFCPST = i.VlrFCPST,
            VlrICMSPartOrigem = i.VlrICMSPartOrigem,
            VlrICMSPartDestino = i.VlrICMSPartDestino,
        };
    }

    public async Task<IReadOnlyList<CotacaoItemValidacaoDto>> ValidarItensImportacaoAsync(
        int propostaId, CancellationToken cancellationToken = default)
    {
        var items = await repository.ValidarItensImportacaoAsync(propostaId, cancellationToken);
        return items.Select(i => new CotacaoItemValidacaoDto
        {
            CdItem = i.CdItem,
            NmItem = i.NmItem,
            VlrUnit = i.VlrUnit,
            VlrPrecoMinimo = i.VlrPrecoMinimo,
            VlrCustoAquisicao = i.VlrCustoAquisicao,
            VlrCustoMedio = i.VlrCustoMedio,
        }).ToList();
    }

    public async Task<CotacaoDadosEmailDto?> GetEnviarEmailDadosAsync(
        int propostaId, CancellationToken cancellationToken = default)
    {
        var d = await repository.GetEnviarEmailDadosAsync(propostaId, cancellationToken);
        if (d is null) return null;
        return new CotacaoDadosEmailDto
        {
            PropostaId = d.PropostaId,
            CotacaoID = d.CotacaoID,
            EstabelecimentoID = d.EstabelecimentoID,
            ClienteId = d.ClienteId,
            CdProposta = d.CdProposta,
            EstabelecimentoNome = d.EstabelecimentoNome,
            ClienteNome = d.ClienteNome,
            ClienteCidadeEstado = d.ClienteCidadeEstado,
            ContatoNome = d.ContatoNome,
            ContatoEmail = d.ContatoEmail,
            ConsultorNome = d.ConsultorNome,
            ConsultorEmail = d.ConsultorEmail,
            ExecutivoNome = d.ExecutivoNome,
            ExecutivoEmail = d.ExecutivoEmail,
            TotalVenda = d.TotalVenda,
            Frete = d.Frete,
        };
    }

    public async Task<IReadOnlyList<CotacaoEnvioHistoricoItemDto>> GetHistoricoEnviosAsync(
        int propostaId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetHistoricoEnviosAsync(propostaId, cancellationToken);
        return items.Select(i => new CotacaoEnvioHistoricoItemDto
        {
            PropostaCotacaoEnvioID = i.PropostaCotacaoEnvioID,
            Nome = i.Nome,
            Email = i.Email,
            DtEnvio = i.DtEnvio,
            NmUsuario = i.NmUsuario,
            DtVisualizacao = i.DtVisualizacao,
            FlagVisualizaEstoque = i.FlagVisualizaEstoque,
            FlagPodeNegociar = i.FlagPodeNegociar,
            FlagPodeTrocarTransportadora = i.FlagPodeTrocarTransportadora,
            FlagPodeTrocarCondPagto = i.FlagPodeTrocarCondPagto,
            FlagAtivo = i.FlagAtivo,
        }).ToList();
    }

    public async Task<IReadOnlyList<CotacaoTipoOptionDto>> GetTiposCotacaoAsync(
        int usuarioId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetTiposCotacaoAsync(usuarioId, cancellationToken);
        return items.Select(i => new CotacaoTipoOptionDto
        {
            CotacaoTipoId = i.CotacaoTipoId,
            DsCotacaoTipo = i.DsCotacaoTipo,
        }).ToList();
    }

    public async Task<IReadOnlyList<CotacaoSelectOptionDto>> GetMotivosBonificacaoAsync(
        int usuarioId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetMotivosBonificacaoAsync(usuarioId, cancellationToken);
        return items.Select(i => new CotacaoSelectOptionDto { Id = i.Id, Nome = i.Nome }).ToList();
    }

    public async Task<IReadOnlyList<CotacaoEstabelecimentoOptionDto>> GetEstabelecimentosAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await repository.GetEstabelecimentosAsync(cancellationToken);
        return items.Select(i => new CotacaoEstabelecimentoOptionDto
        {
            EstabelecimentoId = i.EstabelecimentoId,
            Nome = i.Nome,
            UfId = i.UfId,
        }).ToList();
    }

    public async Task<IReadOnlyList<CotacaoUfOptionDto>> GetUfsAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await repository.GetUfsAsync(cancellationToken);
        return items.Select(i => new CotacaoUfOptionDto { UfId = i.UfId, CdUf = i.CdUf }).ToList();
    }

    public async Task<IReadOnlyList<CotacaoClienteSearchResultDto>> SearchClientesAsync(
        string termo, int estabelecimentoId, CancellationToken cancellationToken = default)
    {
        var items = await repository.SearchClientesAsync(termo, estabelecimentoId, cancellationToken);
        return items.Select(i => new CotacaoClienteSearchResultDto { Id = i.Id, Text = i.Text }).ToList();
    }

    public async Task<IReadOnlyList<CotacaoEnderecoOptionDto>> GetEnderecosByClienteAsync(
        int clienteId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetEnderecosByClienteAsync(clienteId, cancellationToken);
        return items.Select(i => new CotacaoEnderecoOptionDto
        {
            ClienteEnderecoId = i.ClienteEnderecoId,
            Text = i.Text,
        }).ToList();
    }

    public async Task<IReadOnlyList<CotacaoLocalEntregaOptionDto>> GetLocaisEntregaByEnderecoAsync(
        int clienteEnderecoId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetLocaisEntregaByEnderecoAsync(clienteEnderecoId, cancellationToken);
        return items.Select(MapLocalEntrega).ToList();
    }

    public async Task<CotacaoTabelaPrecoOptionDto?> GetTabelaPrecoByClienteAsync(
        int clienteId, CancellationToken cancellationToken = default)
    {
        var t = await repository.GetTabelaPrecoByClienteAsync(clienteId, cancellationToken);
        if (t is null) return null;
        return new CotacaoTabelaPrecoOptionDto { TblPrecoId = t.TblPrecoId, NmTblPreco = t.NmTblPreco };
    }

    public Task<int?> GetFormaPagamentoByClienteAsync(
        int clienteId, CancellationToken cancellationToken = default)
        => repository.GetFormaPagamentoByClienteAsync(clienteId, cancellationToken);

    public Task<string?> GetTipoOVSAPByEnderecoAsync(
        int clienteEnderecoId, CancellationToken cancellationToken = default)
        => repository.GetTipoOVSAPByEnderecoAsync(clienteEnderecoId, cancellationToken);

    public async Task<IReadOnlyList<CotacaoSelectOptionDto>> GetTiposOrdemAsync(
        int cotacaoTipoId, int usuarioId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetTiposOrdemAsync(cotacaoTipoId, usuarioId, cancellationToken);
        return items.Select(i => new CotacaoSelectOptionDto { Id = i.Id, Nome = i.Nome }).ToList();
    }

    public async Task<IReadOnlyList<CotacaoContratoOptionDto>> GetContratosAsync(
        int clienteId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetContratosAsync(clienteId, cancellationToken);
        return items.Select(i => new CotacaoContratoOptionDto { NrContrato = i.NrContrato, Text = i.Text }).ToList();
    }

    public async Task<IReadOnlyList<CotacaoSelectOptionDto>> GetCidadesByUfAsync(
        string cdUf, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetCidadesByUfAsync(cdUf, cancellationToken);
        return items.Select(i => new CotacaoSelectOptionDto { Id = i.Id, Nome = i.Nome }).ToList();
    }

    public async Task<CotacaoEditDadosDto?> GetPropostaParaEditAsync(
        int propostaId, CancellationToken cancellationToken = default)
    {
        var d = await repository.GetPropostaParaEditAsync(propostaId, cancellationToken);
        if (d is null) return null;
        return new CotacaoEditDadosDto
        {
            PropostaId = d.PropostaId,
            Nome = d.Nome,
            TipoCotacao = d.TipoCotacao,
            EstabelecimentoID = d.EstabelecimentoID,
            ClienteId = d.ClienteId,
            ClienteNome = d.ClienteNome,
            ClienteEnderecoID = d.ClienteEnderecoID,
            ClienteLocalEntregaID = d.ClienteLocalEntregaID,
            ObsLocalEntrega = d.ObsLocalEntrega,
            TabelaPrecoID = d.TabelaPrecoID,
            TabelaPrecoNome = d.TabelaPrecoNome,
            FlagPrecoConformeTabela = d.FlagPrecoConformeTabela,
            UfOrigem = d.UfOrigem,
            UfDestino = d.UfDestino,
            CodigoIBGE = d.CodigoIBGE,
            MargemPadrao = d.MargemPadrao,
            DataValidade = d.DataValidade,
            CondPagtoId = d.CondPagtoId,
            FormaPagamentoSAP = d.FormaPagamentoSAP,
            TipoOVSAP = d.TipoOVSAP,
            OrdemCompra = d.OrdemCompra,
            NrContrato = d.NrContrato,
            TipoMotivoIDSAP = d.TipoMotivoIDSAP,
            ContatoNome = d.ContatoNome,
            ContatoEmail = d.ContatoEmail,
            Obs = d.Obs,
            StatusID = d.StatusID,
            StatusNome = d.StatusNome,
        };
    }

    public async Task<CotacaoFreteInicialDto> BuscarFreteInicialAsync(
        int clienteEnderecoId, int clienteId, string? ufDestino,
        CancellationToken cancellationToken = default)
    {
        var (frete, vlrPedidoMinimo) = await repository.BuscarFreteInicialAsync(
            clienteEnderecoId, clienteId, ufDestino, cancellationToken);
        return new CotacaoFreteInicialDto { Frete = frete, VlrPedidoMinimo = vlrPedidoMinimo };
    }

    public async Task<IReadOnlyList<CotacaoSelectOptionDto>> GetFormasPagamentoAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await repository.GetFormasPagamentoAsync(cancellationToken);
        return items.Select(i => new CotacaoSelectOptionDto { Id = i.Id, Nome = i.Nome }).ToList();
    }

    public async Task<CotacaoEmailTemplateDto?> GetDadosEmailTemplateAsync(
        int propostaId, CancellationToken cancellationToken = default)
    {
        var t = await repository.GetDadosEmailTemplateAsync(propostaId, cancellationToken);
        if (t is null) return null;
        return new CotacaoEmailTemplateDto
        {
            CdProposta = t.CdProposta,
            OrdemCompra = t.OrdemCompra,
            Obs = t.Obs,
            ContatoNome = t.ContatoNome,
            ContatoEmail = t.ContatoEmail,
            DataValidade = t.DataValidade,
            CondPagtoNome = t.CondPagtoNome,
            StatusNome = t.StatusNome,
            DiasPrazoEntrega = t.DiasPrazoEntrega,
            TransportadoraNome = t.TransportadoraNome,
            VlrFrete = t.VlrFrete,
            TotalVendaSemFrete = t.TotalVendaSemFrete,
            TotalVendaFinal = t.TotalVendaFinal,
            EstabRazaoSocial = t.EstabRazaoSocial,
            EstabCNPJ = t.EstabCNPJ,
            EstabInscrEstadual = t.EstabInscrEstadual,
            EstabTelefone = t.EstabTelefone,
            EstabEndereco = t.EstabEndereco,
            EstabNumero = t.EstabNumero,
            EstabComplemento = t.EstabComplemento,
            EstabBairro = t.EstabBairro,
            EstabCidade = t.EstabCidade,
            EstabUF = t.EstabUF,
            EstabCEP = t.EstabCEP,
            ConsultorNome = t.ConsultorNome,
            ConsultorEmail = t.ConsultorEmail,
            ConsultorTelefone = t.ConsultorTelefone,
            ClienteRazaoSocial = t.ClienteRazaoSocial,
            ClienteCNPJ = t.ClienteCNPJ,
            ClienteTelefone = t.ClienteTelefone,
            ClienteEndereco = t.ClienteEndereco,
            ClienteNumero = t.ClienteNumero,
            ClienteComplemento = t.ClienteComplemento,
            ClienteBairro = t.ClienteBairro,
            ClienteCidade = t.ClienteCidade,
            ClienteUF = t.ClienteUF,
            ClienteCEP = t.ClienteCEP,
            Itens = t.Itens.Select(i => new CotacaoEmailTemplateItemDto
            {
                CodItemBR = i.CodItemBR,
                DescrItemBR = i.DescrItemBR,
                PrecoItem = i.PrecoItem,
                IPI = i.IPI,
                ST = i.ST,
                Quantidade = i.Quantidade,
                VlrUnitario = i.VlrUnitario,
                NmSegmento = i.NmSegmento,
                NCM = i.NCM,
            }).ToList(),
        };
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static CotacaoDetalheItemDto MapItem(SIC.Domain.Entities.Cotacao.CotacaoDetalheItem i) =>
        new()
        {
            PropostaItemID = i.PropostaItemID,
            PropostaID = i.PropostaID,
            ProdutoID = i.ProdutoID,
            CodigoProduto = i.CodigoProduto,
            DescricaoProduto = i.DescricaoProduto,
            UnidadeMedida = i.UnidadeMedida,
            Quantidade = i.Quantidade,
            EstoqueDisponivel = i.EstoqueDisponivel,
            PrecoMinimo = i.PrecoMinimo,
            PrecoTabelaPreco = i.PrecoTabelaPreco,
            TipoCusto = i.TipoCusto,
            VlrCustoAquisicao = i.VlrCustoAquisicao,
            VlrCustoMedio = i.VlrCustoMedio,
            CustoLiquido = i.CustoLiquido,
            PrecoItem = i.PrecoItem,
            VlrPrecoVenda = i.VlrPrecoVenda,
            Margem = i.Margem,
            MargemPercentual = i.MargemPercentual,
            ICMS = i.ICMS,
            IPI = i.IPI,
            ST = i.ST,
            PIS = i.PIS,
            COFINS = i.COFINS,
            TotalImpostos = i.TotalImpostos,
            TotalSemImposto = i.TotalSemImposto,
            TotalComImposto = i.TotalComImposto,
            ValorLiqUnit = i.ValorLiqUnit,
            ValorICMS = i.ValorICMS,
            PercIPI = i.PercIPI,
            ValorFundoCombPobreza = i.ValorFundoCombPobreza,
            ValorPis = i.ValorPis,
            ValorCOFINS = i.ValorCOFINS,
            ValorFCPST = i.ValorFCPST,
            ValorICMSPartilhaOrigem = i.ValorICMSPartilhaOrigem,
            ValorICMSPartilhaDestino = i.ValorICMSPartilhaDestino,
            MVA = i.MVA,
            NCM = i.NCM,
            NumCA = i.NumCA,
            SegmentoID = i.SegmentoID,
            NmSegmento = i.NmSegmento,
            NmFamilia = i.NmFamilia,
            NmSubFamilia = i.NmSubFamilia,
            CodBarras = i.CodBarras,
            NumeroLinha = i.NumeroLinha,
            Status = i.Status,
            NmStatus = i.NmStatus,
            Invisivel = i.Invisivel,
            FlagCustoAlterado = i.FlagCustoAlterado,
            Curva = i.Curva,
            Criticidade = i.Criticidade,
            PrecoBase = i.PrecoBase,
            NomeTabela = i.NomeTabela,
        };

    private static CotacaoLocalEntregaOptionDto MapLocalEntrega(
        SIC.Domain.Entities.Cotacao.CotacaoLocalEntregaOption i) =>
        new()
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
        };
}
