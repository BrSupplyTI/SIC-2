using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions;
using SIC.Domain.Entities;
using System.Data;

namespace SIC.Infrastructure.Repositories;

public sealed class SqlClientRepository(IConfiguration configuration) : IClientRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    public async Task<IReadOnlyList<ClientSearchItem>> SearchAsync(
        int pageNumber,
        int pageSize,
        string? contemTexto,
        string? comecaComTexto,
        int flagAtivo,
        int estabelecimentoId,
        int flagClienteMae,
        int carteiraId,
        int qtDiasUltimoPedido,
        string? orderBy,
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("SIC_ClientesLista", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@PageNumber", SqlDbType.Int).Value = pageNumber;
        cmd.Parameters.Add("@PageSize", SqlDbType.Int).Value = pageSize;
        cmd.Parameters.Add("@ContemTexto", SqlDbType.VarChar, 100).Value = contemTexto ?? string.Empty;
        cmd.Parameters.Add("@ComecaComTexto", SqlDbType.VarChar, 100).Value = comecaComTexto ?? string.Empty;
        cmd.Parameters.Add("@FlagAtivo", SqlDbType.Int).Value = flagAtivo;
        cmd.Parameters.Add("@EstabelecimentoID", SqlDbType.Int).Value = estabelecimentoId;
        cmd.Parameters.Add("@FlagClienteMae", SqlDbType.Int).Value = flagClienteMae;
        cmd.Parameters.Add("@CarteiraID", SqlDbType.Int).Value = carteiraId;
        cmd.Parameters.Add("@QtDiasUltimoPedido", SqlDbType.Int).Value = qtDiasUltimoPedido;
        cmd.Parameters.Add("@OrderBy", SqlDbType.VarChar, 50).Value = orderBy ?? "Nome (A-Z)";
        cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<ClientSearchItem>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ClientSearchItem
            {
                ClienteID = reader.GetInt32(reader.GetOrdinal("ClienteID")),
                CodigoSAP = ReadString(reader, "CodigoSAP"),
                Nome = ReadString(reader, "Nome"),
                RazaoSocial = ReadString(reader, "RazaoSocial"),
                TipoDocumento = ReadString(reader, "TipoDocumento"),
                CPFCNPJ = ReadString(reader, "CPFCNPJ"),
                Situacao = ReadString(reader, "Situacao"),
                EstabelecimentoID = ReadNullableInt32(reader, "EstabelecimentoID") ?? 0,
                Estabelecimento = ReadString(reader, "Estabelecimento"),
                Carteira = ReadString(reader, "Carteira"),
                QtEnderecos = ReadNullableInt32(reader, "QtEnderecos") ?? 0,
                QtUsuarios = ReadNullableInt32(reader, "QtUsuarios") ?? 0,
                TotalRegistros = reader.GetInt32(reader.GetOrdinal("TotalRegistros"))
            });
        }

        return items;
    }

    public async Task<ClientDetail?> GetDetailAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            DECLARE @QtUsuarios INT = 0
            DECLARE @QtEnderecos INT = 0
            DECLARE @QtLocaisEntrega INT = 0
            DECLARE @CdExtCliente VARCHAR(50)
            DECLARE @CdExtClienteMae VARCHAR(50)

            SELECT @CdExtCliente = C.CdExtCliente
            FROM BR_Cliente C WITH (NOLOCK)
            WHERE C.ClienteID = @ClienteID

            SELECT @QtUsuarios = COUNT(*)
            FROM BR_ClienteUsuario U WITH (NOLOCK)
            WHERE U.FlagAtivo = 1
              AND U.ClienteID = @ClienteID

            SELECT @QtEnderecos = COUNT(*)
            FROM BR_ClienteEndereco E WITH (NOLOCK)
            WHERE E.ClienteID = @ClienteID
              AND E.FlagAtivo = 1

            SELECT @QtLocaisEntrega = COUNT(*)
            FROM BR_ClienteLocalEntrega L WITH (NOLOCK)
            JOIN BR_ClienteEndereco E WITH (NOLOCK) ON E.ClienteEnderecoID = L.ClienteEnderecoID
            WHERE E.ClienteID = @ClienteID
              AND L.FlagAtivo = 1
              AND E.FlagAtivo = 1

            SELECT TOP 1 @CdExtClienteMae = M.CdExtCliente
            FROM BR_Cliente M WITH (NOLOCK)
            JOIN BR_ClienteEndereco ME WITH (NOLOCK) ON ME.ClienteID = M.ClienteID
            WHERE ME.CdEMS = @CdExtCliente
              AND M.ClienteID <> @ClienteID
              AND ME.FlagAtivo = 1

            SELECT C.ClienteID,
                   C.NmCliente AS Nome,
                   C.CdExtCliente AS CodigoSAP,
                   C.RazaoSocialCliente AS RazaoSocial,
                   C.FlagTipoDocumento AS TipoDocumento,
                   C.CNPJCliente AS CPFCNPJ,
                   C.InscrEstCliente AS InscrEstadual,
                   C.LogoCliente,
                   C.CarteiraID,
                   A.NmCarteira,
                   E.NmEstabelecimento,
                   C.EstabelecimentoID,
                   E.CdEstabelecimento,
                   UF.NmUF AS Estado,
                   CASE ISNULL(C.FlagAtivo,0)
                        WHEN 0 THEN 'Inativo'
                        ELSE 'Ativo'
                   END AS Situacao,
                   ISNULL(C.VlrPedidoMinimo,0) AS VlrPedidoMinimo,
                   ISNULL(C.VlrTaxaEntrega,0) AS VlrTaxaEntrega,
                   C.FlagValidacaoFiscal,
                   CC.FlagValidaImpostosTrocaItem,
                   C.FlagProgramacaoAutomatica,
                   C.FlagUtilizaJanelaCorte,
                   C.FlagUtilizaLiberacaoAutomatica,
                   C.FlagLibCatTercAutomatico,
                   CC.FlagNaoValidaNCMTrocaItem,
                   C.FlagIntegracaoAutomaticaSAP,
                   @QtUsuarios AS QtUsuarios,
                   @QtEnderecos AS QtEnderecos,
                   @QtLocaisEntrega AS QtLocaisEntrega,
                   @CdExtClienteMae AS ClienteMae,
                   C.PerfilCreditoID,
                   P.NmPerfilCredito,
                   C.DtAnaliseCredito,
                   C.DtVencAnaliseCredito,
                   C.VlrLimiteCredito,
                   CASE ISNULL(P.FlagControla,0)
                        WHEN 0 THEN 'Crédito Liberado'
                        ELSE 'Sujeito a Avaliação de Crédito'
                   END AS TipoControle,
                   P.DiasAtraso AS DiasAtrasoPermitido,
                   P.MesesDuracaoAnalise,
                   UC.NmUsuario AS ResponsavelAnaliseCredito,
                   CASE C.FlagStatusCredito WHEN 0
                        THEN 'Bloqueado'
                        ELSE CASE WHEN C.DtVencAnaliseCredito < GETDATE()
                                THEN 'Análise Vencida'
                                ELSE CASE P.FlagControla WHEN 0
                                        THEN 'Crédito Liberado (Não Avalia crédito)'
                                        ELSE CASE ISNULL(C.VlrLimiteCredito,0) WHEN 0
                                                THEN 'Somente Pagamento A Vista'
                                                ELSE 'Crédito Normal'
                                             END
                                     END
                             END
                   END AS StatusCredito,
                   C.FlagStatusCredito,
                   DATEDIFF(DAY, GETDATE(), C.DtVencAnaliseCredito) AS DiasRestantes,
                   CP.NmCanalDistribuicaoSAP,
                   C.tipoOVSAP AS TipoDocumentoSAP,
                   TP.Descricao AS DsTipoDocumentoSAP,
                   FP.Descricao AS DsFormaPagamentoSAP,
                   FP.CodFormaPagto AS CodFormaPagamentoSAP                   
            FROM BR_Cliente C WITH (NOLOCK)
            LEFT JOIN BR_ClienteConfig CC WITH (NOLOCK) ON CC.ClienteID = C.ClienteID
            LEFT JOIN BR_Carteira A WITH (NOLOCK) ON A.CarteiraID = C.CarteiraID
            LEFT JOIN BR_Estabelecimento E WITH (NOLOCK) ON E.EstabelecimentoID = C.EstabelecimentoID
            LEFT JOIN BR_UF UF WITH (NOLOCK) ON UF.UfID = C.UfID
            LEFT JOIN BR_PerfilCredito P WITH (NOLOCK) ON P.PerfilCreditoID = C.PerfilCreditoID
            LEFT JOIN BR_Usuario UC WITH (NOLOCK) ON UC.UsuarioID = C.UsuarioAnaliseCredito
            LEFT JOIN BR_ClientePerfil CP WITH (NOLOCK) ON CP.ClientePerfilID = C.ClientePerfilID 
            LEFT JOIN Integracao_Clientes..BR_SAP_TiposDocumentosPedidos TP WITH (NOLOCK) ON TP.Tipo = C.tipoOVSAP
            LEFT JOIN Integracao_Clientes..BR_SAP_FormasPagamento FP WITH (NOLOCK) ON FP.Id = C.FormaPagamentoSAP
            WHERE C.ClienteID = @ClienteID
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.Add("@ClienteID", SqlDbType.Int).Value = clienteId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new ClientDetail
        {
            ClienteID = reader.GetInt32(reader.GetOrdinal("ClienteID")),
            Nome = ReadString(reader, "Nome"),
            CodigoSAP = ReadString(reader, "CodigoSAP"),
            RazaoSocial = ReadString(reader, "RazaoSocial"),
            TipoDocumento = ReadString(reader, "TipoDocumento"),
            CPFCNPJ = ReadString(reader, "CPFCNPJ"),
            InscrEstadual = ReadString(reader, "InscrEstadual"),
            LogoCliente = ReadNullableString(reader, "LogoCliente"),
            CarteiraID = ReadNullableInt32(reader, "CarteiraID") ?? 0,
            NmCarteira = ReadString(reader, "NmCarteira"),
            NmEstabelecimento = ReadString(reader, "NmEstabelecimento"),
            EstabelecimentoID = ReadNullableInt32(reader, "EstabelecimentoID") ?? 0,
            CdEstabelecimento = ReadString(reader, "CdEstabelecimento"),
            Estado = ReadString(reader, "Estado"),
            Situacao = ReadString(reader, "Situacao"),
            VlrPedidoMinimo = ReadDecimal(reader, "VlrPedidoMinimo"),
            VlrTaxaEntrega = ReadDecimal(reader, "VlrTaxaEntrega"),
            FlagValidacaoFiscal = ReadNullableInt32(reader, "FlagValidacaoFiscal") ?? 0,
            FlagValidaImpostosTrocaItem = ReadNullableInt32(reader, "FlagValidaImpostosTrocaItem") ?? 0,
            FlagProgramacaoAutomatica = ReadNullableInt32(reader, "FlagProgramacaoAutomatica") ?? 0,
            FlagUtilizaJanelaCorte = ReadNullableInt32(reader, "FlagUtilizaJanelaCorte") ?? 0,
            FlagUtilizaLiberacaoAutomatica = ReadNullableInt32(reader, "FlagUtilizaLiberacaoAutomatica") ?? 0,
            FlagLibCatTercAutomatico = ReadNullableInt32(reader, "FlagLibCatTercAutomatico") ?? 0,
            FlagNaoValidaNCMTrocaItem = ReadNullableInt32(reader, "FlagNaoValidaNCMTrocaItem") ?? 0,
            QtUsuarios = reader.GetInt32(reader.GetOrdinal("QtUsuarios")),
            QtEnderecos = reader.GetInt32(reader.GetOrdinal("QtEnderecos")),
            QtLocaisEntrega = reader.GetInt32(reader.GetOrdinal("QtLocaisEntrega")),
            ClienteMae = ReadNullableString(reader, "ClienteMae"),
            PerfilCreditoID = ReadNullableInt32(reader, "PerfilCreditoID") ?? 0,
            NmPerfilCredito = ReadString(reader, "NmPerfilCredito"),
            DtAnaliseCredito = ReadNullableDateTime(reader, "DtAnaliseCredito"),
            DtVencAnaliseCredito = ReadNullableDateTime(reader, "DtVencAnaliseCredito"),
            VlrLimiteCredito = ReadDecimal(reader, "VlrLimiteCredito"),
            TipoControle = ReadString(reader, "TipoControle"),
            DiasAtrasoPermitido = ReadNullableInt32(reader, "DiasAtrasoPermitido") ?? 0,
            MesesDuracaoAnalise = ReadNullableInt32(reader, "MesesDuracaoAnalise") ?? 0,
            ResponsavelAnaliseCredito = ReadString(reader, "ResponsavelAnaliseCredito"),
            StatusCredito = ReadString(reader, "StatusCredito"),
            FlagStatusCredito = ReadNullableInt32(reader, "FlagStatusCredito") ?? 0,
            DiasRestantes = ReadNullableInt32(reader, "DiasRestantes") ?? 0,
            FlagIntegracaoAutomaticaSAP = ReadNullableInt32(reader, "FlagIntegracaoAutomaticaSAP") ?? 0,
            NmCanalDistribuicaoSAP = ReadString(reader, "NmCanalDistribuicaoSAP"),
            TipoDocumentoSAP = ReadString(reader, "TipoDocumentoSAP"),
            DsTipoDocumentoSAP = ReadString(reader, "DsTipoDocumentoSAP"),
            DsFormaPagamentoSAP = ReadString(reader, "DsFormaPagamentoSAP"),
            CodFormaPagamentoSAP = ReadString(reader, "CodFormaPagamentoSAP")            
        };
    }

    public async Task<IReadOnlyList<ClientWallet>> GetWalletsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT C.CarteiraID,
                   C.NmCarteira
            FROM BR_Carteira C (NOLOCK)
            ORDER BY C.NmCarteira
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<ClientWallet>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ClientWallet
            {
                CarteiraID = reader.GetInt32(reader.GetOrdinal("CarteiraID")),
                NmCarteira = ReadString(reader, "NmCarteira")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<CatalogEstablishment>> GetEstablishmentsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT 0 AS EstabelecimentoID,
                   'Todos os Estabelecimentos' AS NmEstabelecimento,
                   0 AS OrdemExibicao
            UNION ALL
            SELECT E.EstabelecimentoID,
                   E.NmEstabelecimento,
                   E.OrdemExibicao
            FROM BR_Estabelecimento E WITH (NOLOCK)
            WHERE E.FlagAtivo = 1
              AND ISNULL(E.NmCurto,'') <> ''
              AND SerieNF > 0
            ORDER BY OrdemExibicao
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var items = new List<CatalogEstablishment>();
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new CatalogEstablishment
            {
                EstabelecimentoID = reader.GetInt32(reader.GetOrdinal("EstabelecimentoID")),
                NmEstabelecimento = ReadString(reader, "NmEstabelecimento")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<ClientConsultant>> GetConsultantsAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand("SIC_ConsultoresCarteiraCliente", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        cmd.Parameters.Add("@ClienteID", SqlDbType.Int).Value = clienteId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<ClientConsultant>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ClientConsultant
            {
                UsuarioID = reader.GetInt32(reader.GetOrdinal("UsuarioID")),
                NmUsuario = ReadString(reader, "NmUsuario"),
                Email = ReadString(reader, "Email"),
                Cargo = ReadString(reader, "Cargo")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<ClientTitle>> GetTitulosAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT T.DtEmissao,
                   T.NrNotaFiscal,
                   T.Serie,
                   T.Parcela,
                   T.DtVencimento,
                   CASE WHEN T.FlagEspecie = 'AN'
                        THEN 'Crédito'
                        ELSE CASE WHEN T.DtVencimento < CONVERT(DATE, GETDATE())
                                  THEN 'Vencido'
                                  ELSE 'A Vencer'
                             END
                   END AS Situacao,
                   T.VlrOriginal,
                   IIF(ISNULL(CT.ObsCotacao,'') IN (SELECT ObsCotacao FROM BR_TituloObservacaoDesconsiderar), 0, T.VlrSaldo) AS VlrSaldo
            FROM BR_Titulo T WITH (NOLOCK)
            JOIN Br_Cliente C WITH (NOLOCK) ON C.ClienteID = T.ClienteID
            JOIN BR_Cotacao CT WITH (NOLOCK) ON CT.CotacaoID = T.PedidoID
            WHERE CT.ClienteID = @ClienteID
              AND T.VlrSaldo > 0
              AND IIF(ISNULL(CT.ObsCotacao,'') IN (SELECT ObsCotacao FROM BR_TituloObservacaoDesconsiderar), 0, T.VlrSaldo) > 0
            ORDER BY T.DtVencimento ASC
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 30 };
        cmd.Parameters.Add("@ClienteID", SqlDbType.Int).Value = clienteId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<ClientTitle>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ClientTitle
            {
                DtEmissao = ReadNullableDateTime(reader, "DtEmissao"),
                NrNotaFiscal = reader["NrNotaFiscal"]?.ToString()?.Trim() ?? string.Empty,
                Serie = reader["Serie"]?.ToString()?.Trim() ?? string.Empty,
                Parcela = reader["Parcela"]?.ToString()?.Trim() ?? string.Empty,
                DtVencimento = ReadNullableDateTime(reader, "DtVencimento"),
                Situacao = ReadString(reader, "Situacao"),
                VlrOriginal = ReadDecimal(reader, "VlrOriginal"),
                VlrSaldo = ReadDecimal(reader, "VlrSaldo")
            });
        }

        return items;
    }

    private static string ReadString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal).Trim();
    }

    private static string? ReadNullableString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal).Trim();
    }

    private static int? ReadNullableInt32(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    private static decimal ReadDecimal(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? 0m : reader.GetDecimal(ordinal);
    }

    private static DateTime? ReadNullableDateTime(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }
}
