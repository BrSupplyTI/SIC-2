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
            descricao,
            clienteId,
            tblPrecoId,
            estabelecimentoId,
            propostaId,
            cancellationToken);

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
}
