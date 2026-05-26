using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions;
using SIC.Domain.Entities.Liberacao;
using System.Data;

namespace SIC.Infrastructure.Repositories;

public sealed class SqlLiberacaoPedidoDetalheRepository(IConfiguration configuration) : ILiberacaoPedidoDetalheRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    public async Task<LiberacaoPedidoDetalhe?> ObterAsync(int cotacaoId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("SIC_DetalhesLiberacaoPedido", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 600
        };
        cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return MapDetalhe(reader);
    }

    public async Task<LiberacaoPedidoParametrosCliente?> ObterParametrosClienteAsync(int cotacaoId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("SIC_Parametros_ClienteEndereco", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 60
        };
        cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new LiberacaoPedidoParametrosCliente
        {
            Taxa = GetDecimal(reader, "Taxa"),
            Minimo = GetDecimal(reader, "Minimo"),
            Bloqueio = GetDecimal(reader, "Bloqueio"),
            FlagNaoEditarPedidoComOC = GetInt(reader, "FlagNaoEditarPedidoComOC")
        };
    }

    public async Task<IReadOnlyList<LiberacaoPedidoAnalise>> AnalisarAsync(int cotacaoId, int usuarioId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("SIC_AnaliseLiberacaoPedido", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 120
        };
        cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
        cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);

        var items = new List<LiberacaoPedidoAnalise>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new LiberacaoPedidoAnalise
            {
                FlagErro = GetInt(reader, "FlagErro"),
                FlagAlerta = GetInt(reader, "FlagAlerta"),
                MensagemErro = GetString(reader, "MensagemErro"),
                MensagemAlerta = GetString(reader, "MensagemAlerta")
            });
        }
        return items;
    }

    private static LiberacaoPedidoDetalhe MapDetalhe(SqlDataReader r) => new()
    {
        CotacaoID = GetInt(r, "CotacaoID"),
        EstabelecimentoID = GetInt(r, "EstabelecimentoID"),
        DescTipoOVSAP = GetString(r, "DescTipoOVSAP"),
        TipoOVSAP = GetString(r, "TipoOVSAP"),
        DataHoraPedido = GetDateTime(r, "DataHoraPedido"),
        Estabelecimento = GetString(r, "Estabelecimento"),

        CodERPCliente = GetString(r, "CodERPCliente"),
        RazaoSocialCliente = GetString(r, "RazaoSocialCliente"),
        TipoDocumentoCliente = GetString(r, "TipoDocumentoCliente"),
        NmCliente = GetString(r, "NmCliente"),
        CPFCNPJCliente = GetString(r, "CPFCNPJCliente"),
        InscrEstCliente = GetString(r, "InscrEstCliente"),
        FlagFreteServico = GetInt(r, "FlagFreteServico"),
        UFCliente = GetString(r, "UFCliente"),
        NmUFCliente = GetString(r, "NmUFCliente"),
        TelefoneCliente = GetString(r, "TelefoneCliente"),
        LogoCliente = GetString(r, "LogoCliente"),
        LogoClienteDark = GetString(r, "LogoClienteDark"),
        ClienteID = GetInt(r, "ClienteID"),
        ClienteLocalEntregaID = GetInt(r, "ClienteLocalEntregaID"),

        CompStatusCotacao = GetString(r, "CompStatusCotacao"),
        OrdemCompra = GetString(r, "OrdemCompra"),
        ObsCotacao = GetString(r, "ObsCotacao"),
        ObsAprovacao = GetString(r, "ObsAprovacao"),
        ObsNota = GetString(r, "ObsNota"),
        CanalVendaID = GetInt(r, "CanalVendaID"),
        NmCanalVenda = GetString(r, "NmCanalVenda"),
        NmCarteira = GetString(r, "NmCarteira"),
        StatusCotacao = GetInt(r, "StatusCotacao"),
        ClienteUsuarioID = GetInt(r, "ClienteUsuarioID"),
        NmUsuario = GetString(r, "NmUsuario"),
        EmailUsuario = GetString(r, "EmailUsuario"),
        NmCondPagto = GetString(r, "NmCondPagto"),
        CondPagtoID = GetInt(r, "CondPagtoID"),
        Situacao = GetString(r, "Situacao"),
        StatusID = GetInt(r, "StatusID"),
        VlrFrete = GetDecimal(r, "VlrFrete"),
        VlrFreteServico = GetDecimal(r, "VlrFreteServico"),

        ClienteEnderecoID = GetInt(r, "ClienteEnderecoID"),
        RazaoSocialEndereco = GetString(r, "RazaoSocialEndereco"),
        TipoDocumentoEndereco = GetString(r, "TipoDocumentoEndereco"),
        CodERPEndereco = GetString(r, "CodERPEndereco"),
        CPFCNPJEndereco = GetString(r, "CPFCNPJEndereco"),
        RuaEndereco = GetString(r, "RuaEndereco"),
        NumeroEndereco = GetString(r, "NumeroEndereco"),
        ComplementoEndereco = GetString(r, "ComplementoEndereco"),
        BairroEndereco = GetString(r, "BairroEndereco"),
        CidadeEndereco = GetString(r, "CidadeEndereco"),
        IBGEEndereco = GetString(r, "IBGEEndereco"),
        UFEndereco = GetString(r, "UFEndereco"),
        CEPEndereco = GetString(r, "CEPEndereco"),
        FoneEndereco = GetString(r, "FoneEndereco"),

        FlagEnderecoDirerente = GetInt(r, "FlagEnderecoDirerente"),
        TipoEnderecoEntrega = GetString(r, "TipoEnderecoEntrega"),
        RuaEntrega = GetString(r, "RuaEntrega"),
        NumeroEntrega = GetString(r, "NumeroEntrega"),
        ComplementoEntrega = GetString(r, "ComplementoEntrega"),
        BairroEntrega = GetString(r, "BairroEntrega"),
        CidadeEntrega = GetString(r, "CidadeEntrega"),
        IBGEEntrega = GetString(r, "IBGEEntrega"),
        UFEntrega = GetString(r, "UFEntrega"),
        CEPEntrega = GetString(r, "CEPEntrega"),
        CdControle = GetString(r, "CdControle"),
        NmLocalEntrega = GetString(r, "NmLocalEntrega"),
        ObsLocalEntrega = GetString(r, "ObsLocalEntrega"),
        FlagBloqCredito = GetInt(r, "FlagBloqCredito"),
        SituacaoLocal = GetInt(r, "SituacaoLocal"),

        CategoriaID = GetInt(r, "CategoriaID"),
        NmCategoria = GetString(r, "NmCategtoria"), // SP retorna com typo "NmCategtoria"
        LiberaAutomatico = GetString(r, "LiberaAutomatico"),
        FormaPagamento = GetString(r, "FormaPagamento"),

        DataHoraUltimaAprovacao = GetDateTime(r, "DataHoraUltimaAprovacao"),
        DataProgLiberacao = GetDateTime(r, "DataProgLiberacao"),
        DataProgEmbarque = GetDateTime(r, "DataProgEmbarque"),
        DataProgEntrega = GetDateTime(r, "DataProgEntrega"),
        DataSLACliente = GetDateTime(r, "DataSLACliente"),
        DiasSLA = GetInt(r, "DiasSLA"),
        ObsCalcFrete = GetString(r, "ObsCalcFrete"),

        Peso = GetDecimal(r, "Peso"),
        QtItens = GetInt(r, "QtItens"),
        QtItensBRSupply = GetInt(r, "QtItensBRSupply"),
        QtItensMarketplace = GetInt(r, "QtItensMarketplace"),
        QtItensAlocados = GetInt(r, "QtItensAlocados"),
        QtItensNaoAlocados = GetInt(r, "QtItensNaoAlocados"),
        QtItensBloqueados = GetInt(r, "QtItensBloqueados"),
        VlrTotalBRSupply = GetDecimal(r, "VlrTotalBRSupply"),
        VlrTotalMarketplace = GetDecimal(r, "VlrTotalMarketplace"),
        VlrTotalProdutos = GetDecimal(r, "VlrTotalProdutos"),
        VlrTotalItensAlocados = GetDecimal(r, "VlrTotalItensAlocados"),
        VlrTotalItensNaoAlocados = GetDecimal(r, "VlrTotalItensNaoAlocados"),

        StatusSLACliente = GetString(r, "StatusSLACliente"),
        DiasAtrasoSLACliente = GetInt(r, "DiasAtrasoSLACliente"),

        NmTransportadora = GetString(r, "NmTransportadora"),
        ApelidoTransportadora = GetString(r, "ApelidoTransportadora"),
        CNPJTransportadora = GetString(r, "CNPJTransportadora"),
        TransportadoraID = GetInt(r, "TransportadoraID"),
        PrazoEntregaCalc = GetInt(r, "PrazoEntregaCalc"),
        PrazoEntregaTransp = GetInt(r, "PrazoEntregaTransp"),
        FreteAgrupado = GetString(r, "FreteAgrupado"),
        TblFreteID = GetInt(r, "TblFreteID"),
        CidadeIDDestino = GetInt(r, "CidadeIDDestino"),
        VlrFreteCalc = GetDecimal(r, "VlrFreteCalc"),
        PercentualFrete = GetDecimal(r, "PercentualFrete"),

        MargemBruta = GetDecimal(r, "MargemBruta"),
        NrContrato = GetString(r, "NrContrato"),
        LB = GetString(r, "LB"),
        ROL = GetString(r, "ROL"),
        QtFilaSAP = GetInt(r, "QtFilaSAP")
    };

    private static string GetString(SqlDataReader r, string col)
    {
        var idx = r.GetOrdinal(col);
        return r.IsDBNull(idx) ? string.Empty : r.GetValue(idx)?.ToString()?.Trim() ?? string.Empty;
    }

    private static int GetInt(SqlDataReader r, string col)
    {
        var idx = r.GetOrdinal(col);
        if (r.IsDBNull(idx)) return 0;
        return r.GetFieldType(idx) switch
        {
            var t when t == typeof(int) => r.GetInt32(idx),
            var t when t == typeof(short) => r.GetInt16(idx),
            var t when t == typeof(long) => (int)r.GetInt64(idx),
            var t when t == typeof(byte) => r.GetByte(idx),
            _ => Convert.ToInt32(r.GetValue(idx))
        };
    }

    private static decimal GetDecimal(SqlDataReader r, string col)
    {
        var idx = r.GetOrdinal(col);
        if (r.IsDBNull(idx)) return 0m;
        return r.GetFieldType(idx) switch
        {
            var t when t == typeof(decimal) => r.GetDecimal(idx),
            var t when t == typeof(double) => (decimal)r.GetDouble(idx),
            var t when t == typeof(float) => (decimal)r.GetFloat(idx),
            _ => Convert.ToDecimal(r.GetValue(idx))
        };
    }

    private static DateTime? GetDateTime(SqlDataReader r, string col)
    {
        var idx = r.GetOrdinal(col);
        return r.IsDBNull(idx) ? null : r.GetDateTime(idx);
    }
}
