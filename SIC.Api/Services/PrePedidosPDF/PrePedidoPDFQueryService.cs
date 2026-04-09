using SIC.Api.Contracts.PrePedidosPDF;
using SIC.Domain.Abstractions.PrePedidosPDF;

namespace SIC.Api.Services.PrePedidosPDF;

/// <summary>
/// Implementação das operações de leitura do pré-pedido.
/// </summary>
public sealed class PrePedidoPDFQueryService(
    IPrePedidoPDFQueryRepository repository,
    IPrePedidoPDFIntegrationService integrationService) : IPrePedidoPDFQueryService
{
    public async Task<IReadOnlyList<PrePedidoPDFListItemDto>> GetListAsync(
        int? status,
        string? cdExtCliente,
        DateTime? dataInicial,
        DateTime? dataFinal,
        CancellationToken cancellationToken = default)
    {
        var items = await repository.GetListAsync(status, cdExtCliente, dataInicial, dataFinal, cancellationToken);

        return items.Select(i => new PrePedidoPDFListItemDto
        {
            PDFPrePedidoPDFID = i.PDFPrePedidoPDFID,
            ClienteID = i.ClienteID,
            NmCliente = i.NmCliente,
            OrdemCompra = i.OrdemCompra,
            CNPJ = i.CNPJ,
            CotacaoID = i.CotacaoID,
            StatusPrePedidoPDFID = i.StatusPrePedidoPDFID,
            StatusDescricao = i.StatusDescricao,
            CriadoEm = i.CriadoEm,
        }).ToList();
    }

    public async Task<PrePedidoPDFDetalhesDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var item = await repository.GetByIdAsync(id, cancellationToken);

        if (item is null)
            return null;

        return new PrePedidoPDFDetalhesDto
        {
            PDFPrePedidoPDFID = item.PDFPrePedidoPDFID,
            Arquivo = item.Arquivo,
            ArquivoFormat = item.ArquivoFormat,
            OrdemCompraDataHoraFormat = item.OrdemCompraDataHoraFormat,
            CadastroUsuarioID = item.CadastroUsuarioID,
            CadastroNmUsuario = item.CadastroNmUsuario,
            StatusPrePedidoPDFID = item.StatusPrePedidoPDFID,
            StatusDescricao = item.StatusDescricao,
            CotacaoID = item.CotacaoID,
            OrdemCompra = item.OrdemCompra,
            CNPJ = item.CNPJ,
            ClienteLocalEntregaID = item.ClienteLocalEntregaID,
            ClienteEnderecoID = item.ClienteEnderecoID,
            Cliente = item.Cliente,
            Estabelecimento = item.Estabelecimento,
            EstabelecimentoID = item.EstabelecimentoID,
            Endereco = item.Endereco,
            NmLocalEntrega = item.NmLocalEntrega,
            CondPagto = item.CondPagto,
            CanalVenda = item.CanalVenda,
            TipoOVSAP = item.TipoOVSAP,
            TabelaPreco = item.TabelaPreco,
            CdExtCliente = item.CdExtCliente,
            ClienteID = item.ClienteID,
            TblPrecoID = item.TblPrecoID,
            LogoCliente = item.LogoCliente,
            NmCliente = item.NmCliente,
            VlrMinimoBloqueioPedido = item.VlrMinimoBloqueioPedido,
            ConteudoArquivoJson = await integrationService.GetConteudoArquivoPedidoAsync(item.CdExtCliente, item.OrdemCompra, cancellationToken),
            ObsNota = item.ObsNota,
            ObsComprador = item.ObsComprador,
            ClienteCategoriaPedidoID = item.ClienteCategoriaPedidoID,
            NmCategoriaPedido = item.NmCategoriaPedido,
            Itens = item.Itens.Select(i => new PrePedidoPDFItemDto
            {
                PDFPrePedidoPDFItemID = i.PDFPrePedidoPDFItemID,
                PDFPrePedidoPDFID = i.PDFPrePedidoPDFID,
                PDFSeqItem = i.PDFSeqItem,
                PDFQtde = i.PDFQtde,
                ItemInternoID = i.ItemInternoID,
                ItemCliente = i.ItemCliente,
                Descricao = i.Descricao,
                ItemID = i.ItemID,
                ItemBrSupply = i.ItemBrSupply,
                SegmentoID = i.SegmentoID,
                FamiliaID = i.FamiliaID,
                VlrTblPrecoFormat = i.VlrTblPrecoFormat,
                PDFVlrUnit = i.PDFVlrUnit,
                VlrTotal = i.VlrTotal,
                VlrTotalPedido = i.VlrTotalPedido,
            }).ToList(),
            Logs = item.Logs.Select(l => new PrePedidoPDFLogDto
            {
                Mensagem = l.Mensagem,
                CriadoEmFormatado = l.CriadoEmFormatado,
                Tipo = l.Tipo,
            }).ToList(),
            Enderecos = item.Enderecos.Select(e => new PrePedidoPDFEnderecoDto
            {
                ClienteEnderecoID = e.ClienteEnderecoID,
                Logradouro = e.Logradouro,
            }).ToList(),
            LocaisEntrega = item.LocaisEntrega.Select(l => new PrePedidoPDFLocalEntregaDto
            {
                ClienteLocalEntregaID = l.ClienteLocalEntregaID,
                NmLocalEntrega = l.NmLocalEntrega,
                CdControle = l.CdControle,
            }).ToList(),
            Cnpjs = item.Cnpjs.Select(c => new PrePedidoPDFCnpjDto
            {
                ClienteEnderecoID = c.ClienteEnderecoID,
                CPFCNPJ = c.CPFCNPJ,
            }).ToList(),
            QtdLogsErro = item.QtdLogsErro,
        };
    }

    public async Task<IReadOnlyList<PrePedidoPDFLocalEntregaDto>> GetLocaisEntregaAsync(
        int clienteEnderecoId,
        CancellationToken cancellationToken = default)
    {
        var items = await repository.GetLocaisEntregaAsync(clienteEnderecoId, cancellationToken);

        return items.Select(l => new PrePedidoPDFLocalEntregaDto
        {
            ClienteLocalEntregaID = l.ClienteLocalEntregaID,
            NmLocalEntrega = l.NmLocalEntrega,
            CdControle = l.CdControle,
        }).ToList();
    }

    public async Task<IReadOnlyList<PrePedidoPDFTrocaItemDto>> GetTrocaItensAsync(
        int tblPrecoId,
        int estabelecimentoId,
        int segmentoId,
        int familiaId,
        int itemId,
        CancellationToken cancellationToken = default)
    {
        var items = await repository.GetTrocaItensAsync(
            tblPrecoId,
            estabelecimentoId,
            segmentoId,
            familiaId,
            itemId,
            cancellationToken);

        return items.Select(i => new PrePedidoPDFTrocaItemDto
        {
            CdItem = i.CdItem,
            NmItem = i.NmItem,
            ItemID = i.ItemID,
            VlrTabelaPreco = i.VlrTabelaPreco,
        }).ToList();
    }

    public async Task<IReadOnlyList<PrePedidoPDFCatalogoItemDto>> BuscarCatalogoAsync(
        string descricao,
        int clienteId,
        int tblPrecoId,
        int estabelecimentoId,
        CancellationToken cancellationToken = default)
    {
        var items = await repository.BuscarCatalogoAsync(
            descricao,
            clienteId,
            tblPrecoId,
            estabelecimentoId,
            cancellationToken);

        return items.Select(i => new PrePedidoPDFCatalogoItemDto
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
            Criticidade = i.Criticidade,
            TabelaPreco = i.TabelaPreco,
            ItemDePara = i.ItemDePara,
        }).ToList();
    }
}
