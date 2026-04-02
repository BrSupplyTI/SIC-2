using SIC.Api.Contracts.Produtos;
using SIC.Domain.Abstractions;

namespace SIC.Api.Services;

public sealed class ProductCatalogService(IProductCatalogRepository repository) : IProductCatalogService
{
    public async Task<ProductCatalogResultDto> GetCatalogAsync(ProductCatalogFilterDto filter, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetCatalogAsync(
            filter.PageNumber,
            filter.PageSize,
            filter.ComecaComTexto,
            filter.ContemTexto,
            filter.FlagAtivo,
            filter.FlagMarcaPropria,
            filter.EstabelecimentoID,
            filter.FlagOutlet,
            filter.FlagSobDemanda,
            filter.FlagSustentavel,
            filter.FlagNovidade,
            filter.Curva,
            filter.FlagPadraoBrSupply,
            filter.FlagComEstoque,
            filter.OrderBy,
            cancellationToken);

        var totalRegistros = items.Count > 0 ? items[0].TotalRegistros : 0;

        var dtos = items.Select(i => new ProductCatalogItemDto
        {
            ItemID = i.ItemID,
            CdItem = i.CdItem,
            NmItem = i.NmItem,
            NmSegmento = i.NmSegmento,
            NmFamilia = i.NmFamilia,
            NmSubFamilia = i.NmSubFamilia,
            NmMarca = i.NmMarca,
            FlagTipoMarca = i.FlagTipoMarca,
            NumCA = i.NumCA,
            QtEstoque = i.QtEstoque,
            Curva = i.Curva,
            DtCadastro = i.DtCadastro?.ToString("dd/MM/yyyy")
        }).ToList();

        return new ProductCatalogResultDto
        {
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalRegistros = totalRegistros,
            TotalPaginas = totalRegistros > 0 ? (int)Math.Ceiling((double)totalRegistros / filter.PageSize) : 0,
            Itens = dtos
        };
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

    public async Task<ProductDetailDto?> GetDetailAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetProductDetailAsync(itemId, cancellationToken);
        if (entity is null) return null;

        return new ProductDetailDto
        {
            ItemID = entity.ItemID,
            CdItem = entity.CdItem,
            NmItem = entity.NmItem,
            SegmentoID = entity.SegmentoID,
            NmSegmento = entity.NmSegmento,
            FamiliaID = entity.FamiliaID,
            NmFamilia = entity.NmFamilia,
            SubFamiliaID = entity.SubFamiliaID,
            NmSubFamilia = entity.NmSubFamilia,
            NmMarca = entity.NmMarca,
            DescricaoLonga = entity.DescricaoLonga,
            TituloDsInformacaoTecnica = entity.TituloDsInformacaoTecnica,
            InformacaoTecnica = entity.InformacaoTecnica,
            QtMultiplicador = entity.QtMultiplicador,
            QtMultiplicadorLiberado = entity.QtMultiplicadorLiberado,
            NrPeso = entity.NrPeso,
            Mensagem = entity.Mensagem,
            FlagMarcaPropria = entity.FlagMarcaPropria,
            IconeSegmento = entity.IconeSegmento,
            FlagAtivoSegmento = entity.FlagAtivoSegmento,
            DtMensagem = entity.DtMensagem?.ToString("dd/MM/yyyy"),
            DtCadastro = entity.DtCadastro?.ToString("dd/MM/yyyy"),
            Tags = entity.Tags,
            NumCA = entity.NumCA,
            ValidadeCA = entity.ValidadeCA?.ToString("dd/MM/yyyy"),
            FlagLancamento = entity.FlagLancamento,
            FlagSustentavel = entity.FlagSustentavel,
            CdUnidade = entity.CdUnidade,
            QtdEmbalagem = entity.QtdEmbalagem,
            NmEmbalagem = entity.NmEmbalagem,
            UnidadeMedida = entity.UnidadeMedida,
            QtdeCaixaMaster = entity.QtdeCaixaMaster,
            CodigoBarras = entity.CodigoBarras,
            CodDUN = entity.CodDUN,
            FlagFaltaNoFabricante = entity.FlagFaltaNoFabricante,
            FlagAtivo = entity.FlagAtivo,
            FlagCatalogo = entity.FlagCatalogo,
            CdClassificacaoFiscal = entity.CdClassificacaoFiscal,
            Modelo = entity.Modelo,
            Normas = entity.Normas,
            Referencia = entity.Referencia,
            FSC = entity.FSC,
            ABNT = entity.ABNT,
            Anatel = entity.Anatel,
            Anvisa = entity.Anvisa,
            Inmetro = entity.Inmetro,
            FlagDualSourcing = entity.FlagDualSourcing,
            Origem = entity.Origem,
            FlagOutlet = entity.FlagOutlet,
            Propriedades = entity.Propriedades.Select(p => new ProductPropertyDto
            {
                Tipo = p.Tipo,
                Nome = p.Nome,
                Valor = p.Valor
            }).ToList()
        };
    }

    public async Task<IReadOnlyList<ProductStockEstablishmentDto>> GetStockAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetProductStockAsync(itemId, cancellationToken);

        return items.Select(e => new ProductStockEstablishmentDto
        {
            NmEstabelecimento = e.NmEstabelecimento,
            NmCurto = e.NmCurto,
            EstabelecimentoID = e.EstabelecimentoID,
            CdEstabelecimento = e.CdEstabelecimento,
            QtdContabilSAP = e.QtdContabilSAP,
            QtEstoqueVirtualSP = e.QtEstoqueVirtualSP,
            QtdRemessaSAP = e.QtdRemessaSAP,
            QtdProcessamentoSAP = e.QtdProcessamentoSAP,
            QtdDisponivelSAP = e.QtdDisponivelSAP,
            QtAlocadaSemOVSAP = e.QtAlocadaSemOVSAP,
            QtAlocadaComOVSAP = e.QtAlocadaComOVSAP,
            QtAlocadaSIC = e.QtAlocadaSIC,
            QtNaoDebitaEstoqueSIC = e.QtNaoDebitaEstoqueSIC,
            QtDisponivelSIC = e.QtDisponivelSIC,
            QtdEstoqueSAP = e.QtdEstoqueSAP,
            QtEstoque = e.QtEstoque,
            QtReservadaSIC = e.QtReservadaSIC,
            QtEstoqueWMS = e.QtEstoqueWMS,
            QtProcessamentoWMS = e.QtProcessamentoWMS,
            VlrCustoAquisicao = e.VlrCustoAquisicao,
            VlrCustoMedio = e.VlrCustoMedio,
            FollowComprasNegociacao = e.FollowComprasNegociacao,
            DtFollowComprasNegociacao = e.DtFollowComprasNegociacao?.ToString("dd/MM/yyyy"),
            DsFollowCompras = e.DsFollowCompras,
            DtFollowCompras = e.DtFollowCompras?.ToString("dd/MM/yyyy"),
            Curva = e.Curva,
            Criticidade = e.Criticidade,
            FlagOutlet = e.FlagOutlet,
            FlagSobDemanda = e.FlagSobDemanda,
            FlagOcultoEstoqueZero = e.FlagOcultoEstoqueZero,
            MinLeadTime = e.MinLeadTime,
            MaxLeadTime = e.MaxLeadTime,
            DetalhesCustoAquisicao = e.DetalhesCustoAquisicao,
            NmComprador = e.NmComprador,
            EmailComprador = e.EmailComprador,
            FotoComprador = e.FotoComprador,
            NmGestor = e.NmGestor,
            EmailGestor = e.EmailGestor,
            FotoGestor = e.FotoGestor,
            NmCompradorInternacional = e.NmCompradorInternacional,
            EmailCompradorInternacional = e.EmailCompradorInternacional,
            FotoCompradorInternacional = e.FotoCompradorInternacional
        }).ToList();
    }

    public async Task<IReadOnlyList<ProductStockAllocationDto>> GetStockAllocationsAsync(int itemId, int estabelecimentoId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetProductStockAllocationsAsync(itemId, estabelecimentoId, cancellationToken);

        return items.Select(e => new ProductStockAllocationDto
        {
            Pedido = e.Pedido,
            DtPedido = e.DtPedido.ToString("dd/MM/yyyy"),
            DtProgLiberacao = e.DtProgLiberacao?.ToString("dd/MM/yyyy"),
            NmCliente = e.NmCliente,
            DsStatusCotacao = e.DsStatusCotacao,
            CdEstabelecimento = e.CdEstabelecimento,
            QtSolicitada = e.QtSolicitada,
            QtRupturas = e.QtRupturas,
            NmCanalVenda = e.NmCanalVenda,
            OrdemVendaSAP = e.OrdemVendaSAP
        }).ToList();
    }

    public async Task<IReadOnlyList<ProductPurchaseOrderDto>> GetPurchaseOrdersAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetProductPurchaseOrdersAsync(itemId, cancellationToken);

        return items.Select(e => new ProductPurchaseOrderDto
        {
            Quantidade = e.Quantidade,
            DtPrevisao = e.DtPrevisao?.ToString("dd/MM/yyyy"),
            OrdemCompra = e.OrdemCompra,
            XPed = e.XPed,
            NmEstabelecimento = e.NmEstabelecimento,
            CdEstabelecimento = e.CdEstabelecimento,
            RazaoSocial = e.RazaoSocial
        }).ToList();
    }
}
