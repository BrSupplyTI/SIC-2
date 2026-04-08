using SIC.Api.Contracts.Clientes;
using SIC.Api.Contracts.Produtos;
using SIC.Domain.Abstractions;

namespace SIC.Api.Services;

public sealed class ClientService(IClientRepository repository) : IClientService
{
    public async Task<ClientSearchResultDto> SearchAsync(ClientSearchFilterDto filter, CancellationToken cancellationToken = default)
    {
        var items = await repository.SearchAsync(
            filter.PageNumber,
            filter.PageSize,
            filter.ContemTexto,
            filter.ComecaComTexto,
            filter.FlagAtivo,
            filter.EstabelecimentoID,
            filter.FlagClienteMae,
            filter.CarteiraID,
            filter.QtDiasUltimoPedido,
            filter.OrderBy,
            filter.UsuarioID,
            cancellationToken);

        var totalRegistros = items.Count > 0 ? items[0].TotalRegistros : 0;

        var dtos = items.Select(i => new ClientSearchItemDto
        {
            ClienteID = i.ClienteID,
            CodigoSAP = i.CodigoSAP,
            Nome = i.Nome,
            RazaoSocial = i.RazaoSocial,
            TipoDocumento = i.TipoDocumento,
            CPFCNPJ = i.CPFCNPJ,
            Situacao = i.Situacao,
            EstabelecimentoID = i.EstabelecimentoID,
            Estabelecimento = i.Estabelecimento,
            Carteira = i.Carteira,
            QtEnderecos = i.QtEnderecos,
            QtLocaisEntrega = i.QtLocaisEntrega,
            QtUsuarios = i.QtUsuarios
        }).ToList();

        return new ClientSearchResultDto
        {
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalRegistros = totalRegistros,
            TotalPaginas = totalRegistros > 0 ? (int)Math.Ceiling((double)totalRegistros / filter.PageSize) : 0,
            Itens = dtos
        };
    }

    public async Task<ClientDetailDto?> GetDetailAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetDetailAsync(clienteId, cancellationToken);
        if (entity is null) return null;

        return new ClientDetailDto
        {
            ClienteID = entity.ClienteID,
            Nome = entity.Nome,
            CodigoSAP = entity.CodigoSAP,
            RazaoSocial = entity.RazaoSocial,
            TipoDocumento = entity.TipoDocumento,
            CPFCNPJ = entity.CPFCNPJ,
            InscrEstadual = entity.InscrEstadual,
            LogoCliente = entity.LogoCliente,
            CarteiraID = entity.CarteiraID,
            NmCarteira = entity.NmCarteira,
            NmEstabelecimento = entity.NmEstabelecimento,
            EstabelecimentoID = entity.EstabelecimentoID,
            CdEstabelecimento = entity.CdEstabelecimento,
            Estado = entity.Estado,
            Situacao = entity.Situacao,
            VlrPedidoMinimo = entity.VlrPedidoMinimo,
            VlrTaxaEntrega = entity.VlrTaxaEntrega,
            FlagValidacaoFiscal = entity.FlagValidacaoFiscal,
            FlagValidaImpostosTrocaItem = entity.FlagValidaImpostosTrocaItem,
            FlagProgramacaoAutomatica = entity.FlagProgramacaoAutomatica,
            FlagUtilizaJanelaCorte = entity.FlagUtilizaJanelaCorte,
            FlagUtilizaLiberacaoAutomatica = entity.FlagUtilizaLiberacaoAutomatica,
            FlagLibCatTercAutomatico = entity.FlagLibCatTercAutomatico,
            FlagNaoValidaNCMTrocaItem = entity.FlagNaoValidaNCMTrocaItem,
            QtUsuarios = entity.QtUsuarios,
            QtEnderecos = entity.QtEnderecos,
            QtLocaisEntrega = entity.QtLocaisEntrega,
            ClienteMae = entity.ClienteMae,
            PerfilCreditoID = entity.PerfilCreditoID,
            NmPerfilCredito = entity.NmPerfilCredito,
            DtAnaliseCredito = entity.DtAnaliseCredito?.ToString("dd/MM/yyyy"),
            DtVencAnaliseCredito = entity.DtVencAnaliseCredito?.ToString("dd/MM/yyyy"),
            VlrLimiteCredito = entity.VlrLimiteCredito,
            TipoControle = entity.TipoControle,
            DiasAtrasoPermitido = entity.DiasAtrasoPermitido,
            MesesDuracaoAnalise = entity.MesesDuracaoAnalise,
            ResponsavelAnaliseCredito = entity.ResponsavelAnaliseCredito,
            StatusCredito = entity.StatusCredito,
            FlagStatusCredito = entity.FlagStatusCredito,
            DiasRestantes = entity.DiasRestantes,
            FlagIntegracaoAutomaticaSAP = entity.FlagIntegracaoAutomaticaSAP,
            NmCanalDistribuicaoSAP = entity.NmCanalDistribuicaoSAP,
            TipoDocumentoSAP = entity.TipoDocumentoSAP,
            DsTipoDocumentoSAP = entity.DsTipoDocumentoSAP,
            DsFormaPagamentoSAP = entity.DsFormaPagamentoSAP,
            CodFormaPagamentoSAP = entity.CodFormaPagamentoSAP
        };
    }

    public async Task<IReadOnlyList<ClientWalletDto>> GetWalletsAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.GetWalletsAsync(cancellationToken);
        return items.Select(w => new ClientWalletDto
        {
            CarteiraID = w.CarteiraID,
            NmCarteira = w.NmCarteira
        }).ToList();
    }

    public async Task<IReadOnlyList<CatalogEstablishmentDto>> GetEstablishmentsAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.GetEstablishmentsAsync(cancellationToken);
        return items.Select(e => new CatalogEstablishmentDto
        {
            EstabelecimentoID = e.EstabelecimentoID,
            NmEstabelecimento = e.NmEstabelecimento
        }).ToList();
    }

    public async Task<IReadOnlyList<ClientConsultantDto>> GetConsultantsAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetConsultantsAsync(clienteId, cancellationToken);
        return items.Select(c => new ClientConsultantDto
        {
            UsuarioID = c.UsuarioID,
            NmUsuario = c.NmUsuario,
            Email = c.Email,
            Cargo = c.Cargo
        }).ToList();
    }

    public async Task<IReadOnlyList<ClientTitleDto>> GetTitulosAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetTitulosAsync(clienteId, cancellationToken);
        return items.Select(t => new ClientTitleDto
        {
            DtEmissao = t.DtEmissao?.ToString("dd/MM/yyyy"),
            NrNotaFiscal = t.NrNotaFiscal,
            Serie = t.Serie,
            Parcela = t.Parcela,
            DtVencimento = t.DtVencimento?.ToString("dd/MM/yyyy"),
            Situacao = t.Situacao,
            VlrOriginal = t.VlrOriginal,
            VlrSaldo = t.VlrSaldo
        }).ToList();
    }
}
