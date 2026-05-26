using SIC.Api.Contracts.Liberacao;
using SIC.Domain.Abstractions;

namespace SIC.Api.Services;

public sealed class LiberacaoPedidoService(
    ILiberacaoPedidoRepository repository,
    ILiberacaoPedidoDetalheRepository detalheRepository) : ILiberacaoPedidoService
{
    public async Task<IReadOnlyList<LiberacaoPedidoItemDto>> ListarAsync(
        int estabelecimentoId,
        int usuarioId,
        LiberacaoPedidoFilterDto filtro,
        CancellationToken cancellationToken = default)
    {
        var items = await repository.ListarAsync(
            estabelecimentoId,
            usuarioId,
            filtro.Palavra1,
            filtro.Palavra2,
            filtro.Palavra3,
            filtro.FiltroOrdemCompra,
            filtro.FiltroRuptura,
            filtro.FiltroFrete,
            filtro.FiltroMargemNegativa,
            filtro.FiltroValorAbaixo,
            filtro.FiltroValorAcima,
            filtro.FiltroIntegracaoSAP,
            filtro.FiltroContemItem,
            filtro.FiltroAtrasados,
            filtro.FiltroFretePagar,
            cancellationToken);

        return items.Select(e =>
        {
            var cidadeUf = !string.IsNullOrWhiteSpace(e.Cidade) && !string.IsNullOrWhiteSpace(e.UF)
                ? $"{e.Cidade}/{e.UF}"
                : e.Cidade + e.UF;

            var percFrete = e.ValorPedido != 0
                ? Math.Round(e.VlrFrete / e.ValorPedido * 100, 2)
                : 0;

            return new LiberacaoPedidoItemDto
            {
                CotacaoID = e.CotacaoID,
                AgrupadorFrete = e.AgrupadorFrete,
                VlrFreteCalc = e.VlrFreteCalc,
                TransportadoraID = e.TransportadoraID,
                TipoOVSAP = e.TipoOVSAP,
                QtDiasParado = e.QtDiasParado,
                DataCotacao = e.DataCotacao?.ToString("dd/MM/yyyy") ?? string.Empty,
                DataProgEntrega = e.DataProgEntrega?.ToString("dd/MM/yyyy") ?? string.Empty,
                DataProgEmbarque = e.DataProgEmbarque?.ToString("dd/MM/yyyy") ?? string.Empty,
                DataProgLiberacao = e.DataProgLiberacao?.ToString("dd/MM/yyyy") ?? string.Empty,
                DataSLACliente = e.DataSLACliente?.ToString("dd/MM/yyyy") ?? string.Empty,
                StatusSLACliente = e.StatusSLACliente,
                OrdemCompra = e.OrdemCompra,
                NmCliente = e.NmCliente,
                ClienteID = e.ClienteID,
                RazaoSocialCliente = e.RazaoSocialCliente,
                CarteiraID = e.CarteiraID,
                NmCarteira = e.NmCarteira,
                CdControle = e.CdControle,
                NmLocalEntrega = e.NmLocalEntrega,
                Cidade = e.Cidade,
                UF = e.UF,
                CidadeUF = cidadeUf,
                NmCategoria = e.NmCategoria,
                TipoDocumento = e.TipoDocumento,
                NmCanalVenda = e.NmCanalVenda,
                QtItens = e.QtItens,
                QtRuptura = e.QtRuptura,
                ValorPedido = e.ValorPedido,
                LiberarAutomatico = e.LiberarAutomatico,
                FormaPagto = e.FormaPagto,
                MargemBruta = e.MargemBruta,
                PercFrete = percFrete,
                FlagNaoEditarPedidoComOC = e.FlagNaoEditarPedidoComOC,
                FlagNaoLiberarPedidoSemOC = e.FlagNaoLiberarPedidoSemOC,
                OC_Preenchida = e.OC_Preenchida,
                VlrFrete = e.VlrFrete,
                VlrTaxaServico = e.VlrTaxaServico,
                StatusIntegradoSAP = e.StatusIntegradoSAP,
                DescricaoErroSAP = e.DescricaoErroSAP,
                Observacoes = e.Observacoes,
                Solicitante = e.Solicitante,
                CdExtCliente = e.CdExtCliente,
                MsgOrdemCompraObrigatoria = e.MsgOrdemCompraObrigatoria
            };
        }).ToList();
    }

    public async Task<LiberacaoPedidoDetalheDto?> ObterDetalhesAsync(
        int cotacaoId,
        CancellationToken cancellationToken = default)
    {
        var detalhe = await detalheRepository.ObterAsync(cotacaoId, cancellationToken);
        if (detalhe is null) return null;

        var parametros = await detalheRepository.ObterParametrosClienteAsync(cotacaoId, cancellationToken);

        return new LiberacaoPedidoDetalheDto
        {
            CotacaoID = detalhe.CotacaoID,
            EstabelecimentoID = detalhe.EstabelecimentoID,
            DescTipoOVSAP = detalhe.DescTipoOVSAP,
            TipoOVSAP = detalhe.TipoOVSAP,
            DataHoraPedido = detalhe.DataHoraPedido?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty,
            DataPedido = detalhe.DataHoraPedido?.ToString("dd/MM/yyyy") ?? string.Empty,
            Estabelecimento = detalhe.Estabelecimento,

            CodERPCliente = detalhe.CodERPCliente,
            RazaoSocialCliente = detalhe.RazaoSocialCliente,
            TipoDocumentoCliente = detalhe.TipoDocumentoCliente,
            NmCliente = detalhe.NmCliente,
            CPFCNPJCliente = detalhe.CPFCNPJCliente,
            InscrEstCliente = detalhe.InscrEstCliente,
            FlagFreteServico = detalhe.FlagFreteServico,
            UFCliente = detalhe.UFCliente,
            NmUFCliente = detalhe.NmUFCliente,
            TelefoneCliente = detalhe.TelefoneCliente,
            LogoCliente = detalhe.LogoCliente,
            LogoClienteDark = detalhe.LogoClienteDark,
            ClienteID = detalhe.ClienteID,
            ClienteLocalEntregaID = detalhe.ClienteLocalEntregaID,

            CompStatusCotacao = detalhe.CompStatusCotacao,
            OrdemCompra = detalhe.OrdemCompra,
            ObsCotacao = detalhe.ObsCotacao,
            ObsAprovacao = detalhe.ObsAprovacao,
            ObsNota = detalhe.ObsNota,
            CanalVendaID = detalhe.CanalVendaID,
            NmCanalVenda = detalhe.NmCanalVenda,
            NmCarteira = detalhe.NmCarteira,
            StatusCotacao = detalhe.StatusCotacao,
            ClienteUsuarioID = detalhe.ClienteUsuarioID,
            NmUsuario = detalhe.NmUsuario,
            EmailUsuario = detalhe.EmailUsuario,
            NmCondPagto = detalhe.NmCondPagto,
            CondPagtoID = detalhe.CondPagtoID,
            Situacao = detalhe.Situacao,
            StatusID = detalhe.StatusID,
            VlrFrete = detalhe.VlrFrete,
            VlrFreteServico = detalhe.VlrFreteServico,

            ClienteEnderecoID = detalhe.ClienteEnderecoID,
            RazaoSocialEndereco = detalhe.RazaoSocialEndereco,
            TipoDocumentoEndereco = detalhe.TipoDocumentoEndereco,
            CodERPEndereco = detalhe.CodERPEndereco,
            CPFCNPJEndereco = detalhe.CPFCNPJEndereco,
            RuaEndereco = detalhe.RuaEndereco,
            NumeroEndereco = detalhe.NumeroEndereco,
            ComplementoEndereco = detalhe.ComplementoEndereco,
            BairroEndereco = detalhe.BairroEndereco,
            CidadeEndereco = detalhe.CidadeEndereco,
            IBGEEndereco = detalhe.IBGEEndereco,
            UFEndereco = detalhe.UFEndereco,
            CEPEndereco = detalhe.CEPEndereco,
            FoneEndereco = detalhe.FoneEndereco,

            FlagEnderecoDirerente = detalhe.FlagEnderecoDirerente,
            TipoEnderecoEntrega = detalhe.TipoEnderecoEntrega,
            RuaEntrega = detalhe.RuaEntrega,
            NumeroEntrega = detalhe.NumeroEntrega,
            ComplementoEntrega = detalhe.ComplementoEntrega,
            BairroEntrega = detalhe.BairroEntrega,
            CidadeEntrega = detalhe.CidadeEntrega,
            IBGEEntrega = detalhe.IBGEEntrega,
            UFEntrega = detalhe.UFEntrega,
            CEPEntrega = detalhe.CEPEntrega,
            CdControle = detalhe.CdControle,
            NmLocalEntrega = detalhe.NmLocalEntrega,
            ObsLocalEntrega = detalhe.ObsLocalEntrega,
            FlagBloqCredito = detalhe.FlagBloqCredito,
            SituacaoLocal = detalhe.SituacaoLocal,

            CategoriaID = detalhe.CategoriaID,
            NmCategoria = detalhe.NmCategoria,
            LiberaAutomatico = detalhe.LiberaAutomatico,
            FormaPagamento = detalhe.FormaPagamento,

            DataHoraUltimaAprovacao = detalhe.DataHoraUltimaAprovacao?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty,
            DataProgLiberacao = detalhe.DataProgLiberacao?.ToString("dd/MM/yyyy") ?? string.Empty,
            DataProgEmbarque = detalhe.DataProgEmbarque?.ToString("dd/MM/yyyy") ?? string.Empty,
            DataProgEntrega = detalhe.DataProgEntrega?.ToString("dd/MM/yyyy") ?? string.Empty,
            DataSLACliente = detalhe.DataSLACliente?.ToString("dd/MM/yyyy") ?? string.Empty,
            DiasSLA = detalhe.DiasSLA,
            ObsCalcFrete = detalhe.ObsCalcFrete,

            Peso = detalhe.Peso,
            QtItens = detalhe.QtItens,
            QtItensBRSupply = detalhe.QtItensBRSupply,
            QtItensMarketplace = detalhe.QtItensMarketplace,
            QtItensAlocados = detalhe.QtItensAlocados,
            QtItensNaoAlocados = detalhe.QtItensNaoAlocados,
            QtItensBloqueados = detalhe.QtItensBloqueados,
            VlrTotalBRSupply = detalhe.VlrTotalBRSupply,
            VlrTotalMarketplace = detalhe.VlrTotalMarketplace,
            VlrTotalProdutos = detalhe.VlrTotalProdutos,
            VlrTotalItensAlocados = detalhe.VlrTotalItensAlocados,
            VlrTotalItensNaoAlocados = detalhe.VlrTotalItensNaoAlocados,

            StatusSLACliente = detalhe.StatusSLACliente,
            DiasAtrasoSLACliente = detalhe.DiasAtrasoSLACliente,

            NmTransportadora = detalhe.NmTransportadora,
            ApelidoTransportadora = detalhe.ApelidoTransportadora,
            CNPJTransportadora = detalhe.CNPJTransportadora,
            TransportadoraID = detalhe.TransportadoraID,
            PrazoEntregaCalc = detalhe.PrazoEntregaCalc,
            PrazoEntregaTransp = detalhe.PrazoEntregaTransp,
            FreteAgrupado = detalhe.FreteAgrupado,
            TblFreteID = detalhe.TblFreteID,
            CidadeIDDestino = detalhe.CidadeIDDestino,
            VlrFreteCalc = detalhe.VlrFreteCalc,
            PercentualFrete = detalhe.PercentualFrete,

            MargemBruta = detalhe.MargemBruta,
            NrContrato = detalhe.NrContrato,
            LB = detalhe.LB,
            ROL = detalhe.ROL,
            QtFilaSAP = detalhe.QtFilaSAP,

            Taxa = parametros?.Taxa ?? 0m,
            Minimo = parametros?.Minimo ?? 0m,
            Bloqueio = parametros?.Bloqueio ?? 0m,
            FlagNaoEditarPedidoComOC = parametros?.FlagNaoEditarPedidoComOC ?? 0
        };
    }

    public async Task<LiberacaoPedidoAnaliseDto> AnalisarAsync(
        int cotacaoId,
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var linhas = await detalheRepository.AnalisarAsync(cotacaoId, usuarioId, cancellationToken);

        var erros = new List<string>();
        var alertas = new List<string>();
        var temLinhaSemErro = false;

        foreach (var linha in linhas)
        {
            if (linha.FlagErro == 1 && !string.IsNullOrWhiteSpace(linha.MensagemErro))
                erros.AddRange(SplitMensagens(linha.MensagemErro));

            if (linha.FlagAlerta == 1 && !string.IsNullOrWhiteSpace(linha.MensagemAlerta))
                alertas.AddRange(SplitMensagens(linha.MensagemAlerta));

            if (linha.FlagErro == 0)
                temLinhaSemErro = true;
        }

        // Pedido pronto: ao menos uma linha com FlagErro=0 e nenhuma linha com FlagErro=1.
        // Se a SP não retornar nenhuma linha, consideramos não pronto.
        var pedidoPronto = linhas.Count > 0 && erros.Count == 0 && temLinhaSemErro;

        return new LiberacaoPedidoAnaliseDto
        {
            PedidoPronto = pedidoPronto,
            Erros = erros,
            Alertas = alertas,
            Informacoes = []
        };
    }

    private static IEnumerable<string> SplitMensagens(string texto) =>
        texto.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
