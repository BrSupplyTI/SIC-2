using SIC.Api.Contracts.Propostas;
using SIC.Domain.Abstractions.Propostas;

namespace SIC.Api.Services.Propostas;

public sealed class PropostaQueryService(IPropostaQueryRepository repository) : IPropostaQueryService
{
    public async Task<IReadOnlyList<PropostaListItemDto>> GetListAsync(
        string? filtroCodigo,
        string? filtroNome,
        string? filtroEstabelecimento,
        string? filtroStatus,
        CancellationToken cancellationToken = default)
    {
        var items = await repository.GetListAsync(filtroCodigo, filtroNome, filtroEstabelecimento, filtroStatus, cancellationToken);

        return items.Select(i => new PropostaListItemDto
        {
            PropostaID = i.PropostaID,
            NomeProposta = i.NomeProposta,
            NmEstabelecimento = i.NmEstabelecimento,
            DtCriacao = i.DtCriacao,
            NmStatus = i.NmStatus,
            PercentualConcluido = i.PercentualConcluido,
        }).ToList();
    }

    public async Task<IReadOnlyList<SegmentoItemDto>> GetSegmentosAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await repository.GetSegmentosAsync(cancellationToken);

        return items.Select(i => new SegmentoItemDto
        {
            SegmentoID = i.SegmentoID,
            NmSegmento = i.NmSegmento,
        }).ToList();
    }

    public async Task<SalvarPropostaResponse> SalvarPropostaAsync(
        SalvarPropostaRequest request,
        CancellationToken cancellationToken = default)
    {
        int propostaId;

        if (request.PropostaID.HasValue && request.PropostaID.Value > 0)
        {
            propostaId = request.PropostaID.Value;
            await repository.AtualizarPropostaAsync(
                propostaId,
                request.EstabelecimentoID,
                request.NomeProposta,
                cancellationToken);

            await repository.DeletarPropostaQualidadesAsync(propostaId, cancellationToken);
        }
        else
        {
            propostaId = await repository.SalvarPropostaAsync(
                request.EstabelecimentoID,
                request.NomeProposta,
                cancellationToken);
        }

        foreach (var qs in request.QualSeg)
        {
            await repository.SalvarPropostaQualidadeAsync(
                propostaId,
                qs.SegmentoID,
                qs.Qualidade,
                cancellationToken);
        }

        return new SalvarPropostaResponse { PropostaID = propostaId };
    }

    private static string QualidadeDesc(string valor) => valor switch
    {
        "B" => "Básico",
        "I" => "Intermediário",
        "P" => "Premium",
        _ => valor,
    };

    public async Task<PropostaDetalheDto?> GetByIdAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetByIdAsync(propostaId, cancellationToken);
        if (entity is null) return null;

        return new PropostaDetalheDto
        {
            PropostaID = entity.PropostaID,
            EstabelecimentoID = entity.EstabelecimentoID,
            NomeProposta = entity.NomeProposta,
            StatusID = entity.StatusID,
            QualSeg = entity.QualSeg.Select(qs => new QualSegDetalheDto
            {
                SegmentoID = qs.SegmentoID,
                NmSegmento = qs.NmSegmento,
                Qualidade = qs.Qualidade,
                QualidadeDesc = QualidadeDesc(qs.Qualidade),
            }).ToList(),
        };
    }

    public async Task<PropostaCodificacaoDto?> GetCodificacaoAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetCodificacaoAsync(propostaId, cancellationToken);
        if (entity is null) return null;

        return new PropostaCodificacaoDto
        {
            PropostaID = entity.PropostaID,
            CdProposta = entity.CdProposta,
            EstabelecimentoID = entity.EstabelecimentoID,
            NmEstabelecimento = entity.NmEstabelecimento,
            NomeProposta = entity.NomeProposta,
            StatusID = entity.StatusID,
            NmStatus = entity.NmStatus,
            TotalItens = entity.TotalItens,
            PercentualConcluido = entity.PercentualConcluido,
            QualSeg = entity.QualSeg.Select(qs => new QualSegCodificacaoDto
            {
                Qualidade = qs.Qualidade,
                NmSegmento = qs.NmSegmento,
            }).ToList(),
            Itens = entity.Itens.Select(i => new PropostaCodificacaoItemDto
            {
                PropostaItemID = i.PropostaItemID,
                PropostaID = i.PropostaID,
                DescricaoBreve = i.DescricaoBreve,
                NumeroCA = i.NumeroCA,
                NmMarca = i.NmMarca,
                ItemID = i.ItemID,
                CdItem = i.CdItem,
                NmItem = i.NmItem,
                Qualidade = i.Qualidade,
                VlrCustoAquisicaoFormat = i.VlrCustoAquisicaoFormat,
                FlagForaDeMix = i.FlagForaDeMix,
                FlagSemCorrespondencia = i.FlagSemCorrespondencia,
                FlagAddManual = i.FlagAddManual,
                CodCliente = i.CodCliente,
                DescricaoDetalhada = i.DescricaoDetalhada,
                Familia = i.Familia,
                MarcaFornecedor = i.MarcaFornecedor,
                UnidadeMedida = i.UnidadeMedida,
                TargetFormat = i.TargetFormat,
                QtdAnual = i.QtdAnual,
            }).ToList(),
        };
    }

    public async Task<IReadOnlyList<ItemBuscaResultDto>> BuscarItensBrSupplyAsync(
        int estabelecimentoId,
        string filtro,
        CancellationToken cancellationToken = default)
    {
        var items = await repository.BuscarItensBrSupplyAsync(estabelecimentoId, filtro, cancellationToken);

        return items.Select(i => new ItemBuscaResultDto
        {
            ItemID = i.ItemID,
            Probabilidade = i.Probabilidade,
            CdItem = i.CdItem,
            NmItem = i.NmItem,
            Qualidade = i.Qualidade,
            VlrCustoAquisicaoFormat = i.VlrCustoAquisicaoFormat,
        }).ToList();
    }

    public async Task<bool> AdicionarItemPropostaAsync(
        AdicionarItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var margem = request.MargemPadrao > 0 ? request.MargemPadrao : 30.00m;

        return await repository.AdicionarItemPropostaAsync(
            request.PropostaID,
            request.ItemID,
            request.QtdAnual,
            margem,
            request.ItemFiltro,
            cancellationToken);
    }

    public async Task<bool> ExcluirItemPropostaAsync(
        int propostaId,
        int propostaItemId,
        CancellationToken cancellationToken = default)
    {
        return await repository.ExcluirItemPropostaAsync(propostaId, propostaItemId, cancellationToken);
    }

    public async Task<int> ImportarItensAsync(
        ImportarItensRequest request,
        CancellationToken cancellationToken = default)
    {
        var itens = request.Itens
            .Select(i => (i.CodCliente, i.DescricaoBreve, i.DescricaoDetalhada, i.Familia, i.MarcaFornecedor, i.UnidadeMedida, i.QtdAnual, i.Target))
            .ToList();

        return await repository.ImportarItensAsync(request.PropostaID, itens, cancellationToken);
    }

    public async Task<CodificarItemResultDto> CodificarItemAsync(
        int propostaItemId,
        int estabelecimentoId,
        CancellationToken cancellationToken = default)
    {
        var result = await repository.CodificarItemAsync(propostaItemId, estabelecimentoId, cancellationToken);

        return new CodificarItemResultDto
        {
            PropostaItemID = result.PropostaItemID,
            Codificado = result.Codificado,
            SemCorrespondencia = result.SemCorrespondencia,
            ItemID = result.ItemID,
            CdItem = result.CdItem,
            NmItem = result.NmItem,
            Qualidade = result.Qualidade,
        };
    }

    public async Task<bool> MarcarSegundoPlanoAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        return await repository.MarcarSegundoPlanoAsync(propostaId, cancellationToken);
    }

    public async Task<bool> ExcluirPropostaAsync(
        int propostaId,
        CancellationToken cancellationToken = default)
    {
        return await repository.ExcluirPropostaAsync(propostaId, cancellationToken);
    }

    public async Task<bool> VincularItemManualAsync(
        int propostaItemId,
        int itemId,
        CancellationToken cancellationToken = default)
    {
        return await repository.VincularItemManualAsync(propostaItemId, itemId, cancellationToken);
    }
}
