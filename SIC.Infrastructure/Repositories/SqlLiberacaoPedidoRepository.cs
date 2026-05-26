using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions;
using SIC.Domain.Entities;
using System.Data;

namespace SIC.Infrastructure.Repositories;

public sealed class SqlLiberacaoPedidoRepository(IConfiguration configuration) : ILiberacaoPedidoRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    public async Task<IReadOnlyList<LiberacaoPedidoItem>> ListarAsync(
        int estabelecimentoId,
        int usuarioId,
        string? filtroPalavra1 = null,
        string? filtroPalavra2 = null,
        string? filtroPalavra3 = null,
        int filtroOrdemCompra = 0,
        int filtroRuptura = 0,
        int filtroFrete = 0,
        int filtroMargemNegativa = 0,
        decimal filtroValorAbaixo = 0,
        decimal filtroValorAcima = 0,
        string? filtroIntegracaoSAP = null,
        string? filtroContemItem = null,
        int filtroAtrasados = 0,
        int filtroFretePagar = 0,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("SIC_Lista_Liberacao_Comercial", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 600
        };

        cmd.Parameters.AddWithValue("@EstabelecimentoID", estabelecimentoId);
        cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
        cmd.Parameters.AddWithValue("@FiltroPalavra1", filtroPalavra1 ?? string.Empty);
        cmd.Parameters.AddWithValue("@FiltroPalavra2", filtroPalavra2 ?? string.Empty);
        cmd.Parameters.AddWithValue("@FiltroPalavra3", filtroPalavra3 ?? string.Empty);
        cmd.Parameters.AddWithValue("@FiltroOrdemCompra", filtroOrdemCompra);
        cmd.Parameters.AddWithValue("@FiltroRuptura", filtroRuptura);
        cmd.Parameters.AddWithValue("@FiltroFrete", filtroFrete);
        cmd.Parameters.AddWithValue("@FiltroMargemNegativa", filtroMargemNegativa);
        cmd.Parameters.AddWithValue("@FiltroValorAbaixo", filtroValorAbaixo);
        cmd.Parameters.AddWithValue("@FiltroValorAcima", filtroValorAcima);
        cmd.Parameters.AddWithValue("@FiltroIntegracaoSAP", filtroIntegracaoSAP ?? string.Empty);
        cmd.Parameters.AddWithValue("@FiltroContemItem", filtroContemItem ?? string.Empty);
        cmd.Parameters.AddWithValue("@FiltroAtrasados", filtroAtrasados);
        cmd.Parameters.AddWithValue("@FiltroFretePagar", filtroFretePagar);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<LiberacaoPedidoItem>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(MapRow(reader));
        }

        return items;
    }

    private static LiberacaoPedidoItem MapRow(SqlDataReader r)
    {
        return new LiberacaoPedidoItem
        {
            CotacaoID = r.IsDBNull("CotacaoID") ? 0 : r.GetInt32(r.GetOrdinal("CotacaoID")),
            AgrupadorFrete = r.IsDBNull("AgrupadorFrete") ? 0 : r.GetInt32(r.GetOrdinal("AgrupadorFrete")),
            VlrFreteCalc = r.IsDBNull("VlrFreteCalc") ? 0 : r.GetDecimal(r.GetOrdinal("VlrFreteCalc")),
            TransportadoraID = r.IsDBNull("TransportadoraID") ? 0 : r.GetInt32(r.GetOrdinal("TransportadoraID")),
            TipoOVSAP = r.IsDBNull("TipoOVSAP") ? string.Empty : r.GetString(r.GetOrdinal("TipoOVSAP")).Trim(),
            QtDiasParado = r.IsDBNull("QtDiasParado") ? 0 : r.GetInt32(r.GetOrdinal("QtDiasParado")),
            DataCotacao = r.IsDBNull("DataCotacao") ? null : r.GetDateTime(r.GetOrdinal("DataCotacao")),
            DataProgEntrega = r.IsDBNull("DataProgEntrega") ? null : r.GetDateTime(r.GetOrdinal("DataProgEntrega")),
            DataProgEmbarque = r.IsDBNull("DataProgEmbarque") ? null : r.GetDateTime(r.GetOrdinal("DataProgEmbarque")),
            DataProgLiberacao = r.IsDBNull("DataProgLiberacao") ? null : r.GetDateTime(r.GetOrdinal("DataProgLiberacao")),
            DataSLACliente = r.IsDBNull("DataSLACliente") ? null : r.GetDateTime(r.GetOrdinal("DataSLACliente")),
            StatusSLACliente = r.IsDBNull("StatusSLACliente") ? string.Empty : r.GetString(r.GetOrdinal("StatusSLACliente")).Trim(),
            OrdemCompra = r.IsDBNull("OrdemCompra") ? string.Empty : r.GetString(r.GetOrdinal("OrdemCompra")).Trim(),
            NmCliente = r.IsDBNull("NmCliente") ? string.Empty : r.GetString(r.GetOrdinal("NmCliente")).Trim(),
            ClienteID = r.IsDBNull("ClienteID") ? 0 : r.GetInt32(r.GetOrdinal("ClienteID")),
            RazaoSocialCliente = r.IsDBNull("RazaoSocialCliente") ? string.Empty : r.GetString(r.GetOrdinal("RazaoSocialCliente")).Trim(),
            CarteiraID = r.IsDBNull("CarteiraID") ? 0 : r.GetInt32(r.GetOrdinal("CarteiraID")),
            NmCarteira = r.IsDBNull("NmCarteira") ? string.Empty : r.GetString(r.GetOrdinal("NmCarteira")).Trim(),
            CdControle = r.IsDBNull("CdControle") ? string.Empty : r.GetString(r.GetOrdinal("CdControle")).Trim(),
            NmLocalEntrega = r.IsDBNull("NmLocalEntrega") ? string.Empty : r.GetString(r.GetOrdinal("NmLocalEntrega")).Trim(),
            Cidade = r.IsDBNull("Cidade") ? string.Empty : r.GetString(r.GetOrdinal("Cidade")).Trim(),
            UF = r.IsDBNull("UF") ? string.Empty : r.GetString(r.GetOrdinal("UF")).Trim(),
            NmCategoria = r.IsDBNull("NmCategoria") ? string.Empty : r.GetString(r.GetOrdinal("NmCategoria")).Trim(),
            TipoDocumento = r.IsDBNull("TipoDocumento") ? string.Empty : r.GetString(r.GetOrdinal("TipoDocumento")).Trim(),
            NmCanalVenda = r.IsDBNull("NmCanalVenda") ? string.Empty : r.GetString(r.GetOrdinal("NmCanalVenda")).Trim(),
            QtItens = r.IsDBNull("QtItens") ? 0 : r.GetInt32(r.GetOrdinal("QtItens")),
            QtRuptura = r.IsDBNull("QtRuptura") ? 0 : r.GetInt32(r.GetOrdinal("QtRuptura")),
            ValorPedido = r.IsDBNull("ValorPedido") ? 0 : r.GetDecimal(r.GetOrdinal("ValorPedido")),
            LiberarAutomatico = r.IsDBNull("LiberarAutomatico") ? string.Empty : r.GetString(r.GetOrdinal("LiberarAutomatico")).Trim(),
            FormaPagto = r.IsDBNull("FormaPagto") ? string.Empty : r.GetString(r.GetOrdinal("FormaPagto")).Trim(),
            MargemBruta = r.IsDBNull("MargemBruta") ? 0 : r.GetDecimal(r.GetOrdinal("MargemBruta")),
            FlagNaoEditarPedidoComOC = r.IsDBNull("FlagNaoEditarPedidoComOC") ? 0 : r.GetInt32(r.GetOrdinal("FlagNaoEditarPedidoComOC")),
            FlagNaoLiberarPedidoSemOC = r.IsDBNull("FlagNaoLiberarPedidoSemOC") ? 0 : r.GetInt32(r.GetOrdinal("FlagNaoLiberarPedidoSemOC")),
            OC_Preenchida = r.IsDBNull("OC_Preenchida") ? string.Empty : r.GetString(r.GetOrdinal("OC_Preenchida")).Trim(),
            VlrFrete = r.IsDBNull("VlrFrete") ? 0 : r.GetDecimal(r.GetOrdinal("VlrFrete")),
            VlrTaxaServico = r.IsDBNull("VlrTaxaServico") ? 0 : r.GetDecimal(r.GetOrdinal("VlrTaxaServico")),
            StatusIntegradoSAP = r.IsDBNull("StatusIntegradoSAP") ? string.Empty : r.GetString(r.GetOrdinal("StatusIntegradoSAP")).Trim(),
            DescricaoErroSAP = r.IsDBNull("DescricaoErroSAP") ? string.Empty : r.GetString(r.GetOrdinal("DescricaoErroSAP")).Trim(),
            Observacoes = r.IsDBNull("Observacoes") ? string.Empty : r.GetString(r.GetOrdinal("Observacoes")).Trim(),
            Solicitante = r.IsDBNull("Solicitante") ? string.Empty : r.GetString(r.GetOrdinal("Solicitante")).Trim(),
            CdExtCliente = r.IsDBNull("CdExtCliente") ? string.Empty : r.GetString(r.GetOrdinal("CdExtCliente")).Trim(),
            MsgOrdemCompraObrigatoria = r.IsDBNull("MsgOrdemCompraObrigatoria") ? string.Empty : r.GetString(r.GetOrdinal("MsgOrdemCompraObrigatoria")).Trim()
        };
    }
}
