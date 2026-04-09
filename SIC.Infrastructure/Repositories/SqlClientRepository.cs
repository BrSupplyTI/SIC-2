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
              AND ISNULL(E.CdEms,'') <> ''
              AND E.CdCidadeEnderecoID IS NOT NULL

            SELECT @QtLocaisEntrega = COUNT(*)
            FROM BR_ClienteLocalEntrega L WITH (NOLOCK)
            JOIN BR_ClienteEndereco E WITH (NOLOCK) ON E.ClienteEnderecoID = L.ClienteEnderecoID
            WHERE E.ClienteID = @ClienteID
              AND L.FlagAtivo = 1
              AND L.FlagHabilitado = 1              

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
                   ISNULL(C.FlagIntegracaoAutomaticaSAP,0) AS FlagIntegracaoAutomaticaSAP,
                   ISNULL(C.FlagUtilizaLiberacaoAutomatica,0) AS FlagUtilizaLiberacaoAutomatica,
                   ISNULL(C.FlagProgramacaoAutomatica,0) AS FlagProgramacaoAutomatica,
                   ISNULL(C.FlagUtilizaJanelaCorte,0) AS FlagUtilizaJanelaCorte,
                   ISNULL(C.FlagFreteAgrupCNPJ,0) AS FlagFreteAgrupCNPJ,
                   ISNULL(C.FlagValidacaoFiscal,0) AS FlagValidacaoFiscal,
                   ISNULL(CC.FlagValidaImpostosTrocaItem,0) AS FlagValidaImpostosTrocaItem,
                   ISNULL(C.FlagNaoLiberarPedidoSemOC,0) AS FlagNaoLiberarPedidoSemOC,
                   ISNULL(C.FlagNaoEditarPedidoComOC,0) AS FlagNaoEditarPedidoComOC,
                   ISNULL(CC.FlagPoliticaEntrega,0) AS FlagPoliticaEntrega,
                   ISNULL(CC.FlagMultiCD,0) AS FlagMultiCD,
                   ISNULL(CC.FlagMultiCDEnderecos,0) AS FlagMultiCDEnderecos,
                   ISNULL(CC.FlagMultiCDPedidos,0) AS FlagMultiCDPedidos,
                   ISNULL(CC.FlagTrocaItemAutomatica,0) AS FlagTrocaItemAutomatica,
                   ISNULL(DP.FlagNaoValidaTrocaItem,0) AS FlagNaoValidaTrocaItem,
                   ISNULL(CC.FlagNaoValidaNCMTrocaItem,0) AS FlagNaoValidaNCMTrocaItem,
                   ISNULL(CT.FlagAutoConcat,0) AS FlagAutoConcat,
                   ISNULL(CT.FlagOrdemCompra,0) AS FlagOrdemCompra,
                   CASE WHEN ISNULL(CT.FlagPorEndereco,0) = 0                                      
                       THEN CASE WHEN ISNULL(CT.FlagConcatCodigoControle,0) = 1
                               THEN 2 
                               ELSE 0
                               END                       
                       ELSE 1
                   END AS FlagTipoConcat,
                   ISNULL(CT.FlagConcatPedidoRuptura,0) AS FlagConcatPedidoRuptura,
                   ISNULL(CT.FlagAutoIsentaFrete,0) AS FlagAutoIsentaFrete,
                   ISNULL(CT.FlagPrioConcatPerfilSolicitante,0) AS FlagPrioConcatPerfilSolicitante,
                   ISNULL(CT.FlagConcatItemFornecedor,0) AS FlagConcatItemFornecedor,
                   ISNULL(CT.FlagConcatIsolarCategorias,0) AS FlagConcatIsolarCategorias,
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
                   UC.UsuarioID AS UsuarioIDAnaliseCredito,
                   UC.Email AS EmailResponsavelAnaliseCredito,
                   UC.Foto AS FotoResponsavelAnaliseCredito,
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
                   FP.CodFormaPagto AS CodFormaPagamentoSAP,
                   TBP.NmTblPreco,
                   C.TblPrecoID,
                   REPLACE(C.TelefoneCliente, ';', '') AS TelefoneCliente,
                   SC.Nome AS SegmentoCliente,
                   CV.NmCanalVenda AS NmCanalVenda,
                   CP.NmClientePerfil,
                   CPG.NmCondPagto,
                   C.CanalVendaID,
                   C.Cnae,
                   C.CodCnaeSetor,
                   C.DsCnaeSetor,
                   C.CdNatJuridica,
                   C.DsNatJuridica
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
            LEFT JOIN Integracao_Clientes..BR_Itens_DePara_Gestao DP (NOLOCK) ON DP.ClienteID = C.ClienteID
            LEFT JOIN Integracao_Clientes..Concat_Config CT (NOLOCK) ON CT.ClienteID = C.ClienteID
            LEFT JOIN BR_SegmentoCliente SC WITH (NOLOCK) ON SC.SegmentoClienteID = C.SegmentoClienteID
            LEFT JOIN BR_SegmentoEmpresarial SE WITH (NOLOCK) ON SE.SegmentoEmpresarialID = C.SegmentoEmpresarialID
            LEFT JOIN BR_CondPagto CPG WITH (NOLOCK) ON CPG.CondPagtoID = C.CondPagtoID
            LEFT JOIN BR_CanalVenda CV WITH (NOLOCK) ON CV.CanalVendaID = C.CanalVendaID
            LEFT JOIN BR_TblPreco TBP WITH (NOLOCK) ON TBP.TblPrecoID = C.TblPrecoID
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
            FlagIntegracaoAutomaticaSAP = ReadNullableInt32(reader, "FlagIntegracaoAutomaticaSAP") ?? 0,
            FlagUtilizaLiberacaoAutomatica = ReadNullableInt32(reader, "FlagUtilizaLiberacaoAutomatica") ?? 0,
            FlagProgramacaoAutomatica = ReadNullableInt32(reader, "FlagProgramacaoAutomatica") ?? 0,
            FlagUtilizaJanelaCorte = ReadNullableInt32(reader, "FlagUtilizaJanelaCorte") ?? 0,
            FlagFreteAgrupCNPJ = ReadNullableInt32(reader, "FlagFreteAgrupCNPJ") ?? 0,
            FlagValidacaoFiscal = ReadNullableInt32(reader, "FlagValidacaoFiscal") ?? 0,
            FlagValidaImpostosTrocaItem = ReadNullableInt32(reader, "FlagValidaImpostosTrocaItem") ?? 0,
            FlagNaoLiberarPedidoSemOC = ReadNullableInt32(reader, "FlagNaoLiberarPedidoSemOC") ?? 0,
            FlagNaoEditarPedidoComOC = ReadNullableInt32(reader, "FlagNaoEditarPedidoComOC") ?? 0,
            FlagPoliticaEntrega = ReadNullableInt32(reader, "FlagPoliticaEntrega") ?? 0,
            FlagMultiCD = ReadNullableInt32(reader, "FlagMultiCD") ?? 0,
            FlagMultiCDEnderecos = ReadNullableInt32(reader, "FlagMultiCDEnderecos") ?? 0,
            FlagMultiCDPedidos = ReadNullableInt32(reader, "FlagMultiCDPedidos") ?? 0,
            FlagTrocaItemAutomatica = ReadNullableInt32(reader, "FlagTrocaItemAutomatica") ?? 0,
            FlagNaoValidaTrocaItem = ReadNullableInt32(reader, "FlagNaoValidaTrocaItem") ?? 0,
            FlagNaoValidaNCMTrocaItem = ReadNullableInt32(reader, "FlagNaoValidaNCMTrocaItem") ?? 0,
            FlagAutoConcat = ReadNullableInt32(reader, "FlagAutoConcat") ?? 0,
            FlagOrdemCompra = ReadNullableInt32(reader, "FlagOrdemCompra") ?? 0,
            FlagTipoConcat = ReadNullableInt32(reader, "FlagTipoConcat") ?? 0,
            FlagConcatPedidoRuptura = ReadNullableInt32(reader, "FlagConcatPedidoRuptura") ?? 0,
            FlagAutoIsentaFrete = ReadNullableInt32(reader, "FlagAutoIsentaFrete") ?? 0,
            FlagPrioConcatPerfilSolicitante = ReadNullableInt32(reader, "FlagPrioConcatPerfilSolicitante") ?? 0,
            FlagConcatItemFornecedor = ReadNullableInt32(reader, "FlagConcatItemFornecedor") ?? 0,
            FlagConcatIsolarCategorias = ReadNullableInt32(reader, "FlagConcatIsolarCategorias") ?? 0,            
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
            UsuarioIDAnaliseCredito = ReadNullableInt32(reader, "UsuarioIDAnaliseCredito") ?? 0,
            EmailResponsavelAnaliseCredito = ReadString(reader, "EmailResponsavelAnaliseCredito"),
            FotoResponsavelAnaliseCredito = ReadString(reader, "FotoResponsavelAnaliseCredito"),
            StatusCredito = ReadString(reader, "StatusCredito"),
            FlagStatusCredito = ReadNullableInt32(reader, "FlagStatusCredito") ?? 0,
            DiasRestantes = ReadNullableInt32(reader, "DiasRestantes") ?? 0,
            NmCanalDistribuicaoSAP = ReadString(reader, "NmCanalDistribuicaoSAP"),
            TipoDocumentoSAP = ReadString(reader, "TipoDocumentoSAP"),
            DsTipoDocumentoSAP = ReadString(reader, "DsTipoDocumentoSAP"),
            DsFormaPagamentoSAP = ReadString(reader, "DsFormaPagamentoSAP"),
            CodFormaPagamentoSAP = ReadString(reader, "CodFormaPagamentoSAP"),
            NmTblPreco = ReadString(reader, "NmTblPreco"),
            TblPrecoID = ReadNullableInt32(reader, "TblPrecoID") ?? 0,
            TelefoneCliente = ReadString(reader, "TelefoneCliente"),
            SegmentoCliente = ReadString(reader, "SegmentoCliente"),
            NmCanalVenda = ReadString(reader, "NmCanalVenda"),
            NmClientePerfil = ReadString(reader, "NmClientePerfil"),
            NmCondPagto = ReadString(reader, "NmCondPagto"),
            CanalVendaID = ReadNullableInt32(reader, "CanalVendaID") ?? 0,
            Cnae = ReadString(reader, "Cnae"),
            CodCnaeSetor = ReadString(reader, "CodCnaeSetor"),
            DsCnaeSetor = ReadString(reader, "DsCnaeSetor"),
            CdNatJuridica = ReadString(reader, "CdNatJuridica"),
            DsNatJuridica = ReadString(reader, "DsNatJuridica")
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
                   CT.CotacaoID AS Pedido,
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
                Pedido = reader["Pedido"]?.ToString()?.Trim() ?? string.Empty,
                Parcela = reader["Parcela"]?.ToString()?.Trim() ?? string.Empty,
                DtVencimento = ReadNullableDateTime(reader, "DtVencimento"),
                Situacao = ReadString(reader, "Situacao"),
                VlrOriginal = ReadDecimal(reader, "VlrOriginal"),
                VlrSaldo = ReadDecimal(reader, "VlrSaldo")
            });
        }

        return items;
    }

    public async Task<ClientCreditBalance> GetCreditBalanceAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            DECLARE @VlrPedidosNaoFaturados NUMERIC(18,2) = 0
            DECLARE @VlrCreditos NUMERIC(18,2) = 0
            DECLARE @VlrTitulosEmAberto NUMERIC(18,2) = 0

            SELECT @VlrPedidosNaoFaturados = ISNULL(SUM(I.VlrTotalPedido), 0)
            FROM Integracao_Clientes..ImpPedido I WITH (NOLOCK)
            JOIN BR_Cotacao CT WITH (NOLOCK) ON CT.CotacaoID = I.CotacaoID
            WHERE I.ClienteID = @ClienteID
              AND ISNULL(CT.ObsCotacao,'') NOT IN (SELECT ObsCotacao FROM BR_TituloObservacaoDesconsiderar)
              AND I.ImpStatusID NOT IN (70, 80, 200, 300)

            SELECT @VlrCreditos = ISNULL(IIF(T.FlagEspecie = 'AN', SUM(T.VlrSaldo), 0), 0),
                   @VlrTitulosEmAberto = ISNULL(IIF(T.FlagEspecie <> 'AN', SUM(T.VlrSaldo), 0), 0)
            FROM BR_Titulo T WITH (NOLOCK)
            JOIN Br_Cliente C WITH (NOLOCK) ON C.ClienteID = T.ClienteID
            JOIN BR_Cotacao CT WITH (NOLOCK) ON CT.CotacaoID = T.PedidoID
            WHERE CT.ClienteID = @ClienteID
                AND T.VlrSaldo > 0
                AND IIF(ISNULL(CT.ObsCotacao,'') IN (SELECT ObsCotacao FROM BR_TituloObservacaoDesconsiderar), 0, T.VlrSaldo) > 0
            GROUP BY T.FlagEspecie

            SELECT @VlrCreditos AS VlrCreditos,
                   @VlrTitulosEmAberto AS VlrTitulosEmAberto,
                   @VlrPedidosNaoFaturados AS VlrPedidosNaoFaturados
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 30 };
        cmd.Parameters.Add("@ClienteID", SqlDbType.Int).Value = clienteId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new ClientCreditBalance
            {
                VlrCreditos = ReadDecimal(reader, "VlrCreditos"),
                VlrTitulosEmAberto = ReadDecimal(reader, "VlrTitulosEmAberto"),
                VlrPedidosNaoFaturados = ReadDecimal(reader, "VlrPedidosNaoFaturados")
            };
        }

        return new ClientCreditBalance();
    }

    public async Task<IReadOnlyList<ClientAddress>> GetAddressesAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT E.ClienteEnderecoID,
                   CASE WHEN ISNULL(E.CdEms,'') = '' THEN 'Erro'
                        ELSE CASE(ISNULL(E.FlagAtivo,0))
                                WHEN 0 THEN 'Inativo'
                                ELSE 'Ativo'
                             END
                   END AS Situacao,
                   ISNULL(E.CdEms,'') AS CodSAP,
                   E.FlagTipoDocumento AS TipoDocumento,
                   E.CPFCNPJ,
                   E.RazaoSocial,
                   Cid.NmCidade,
                   UF.CdUF,
                   T.NmTblPreco AS TabelaPreco,
                   ISNULL(E.VlrPedidoMinimo,0) AS VlrPedidoMinimo,
                   ISNULL(E.VlrTaxaEntrega,0) AS VlrTaxaEntrega
            FROM BR_ClienteEndereco E WITH (NOLOCK)
            JOIN BR_Cidade Cid WITH (NOLOCK) ON Cid.CidadeID = E.CdCidadeEnderecoID
            JOIN BR_UF UF WITH (NOLOCK) ON UF.UFID = Cid.UFID
            LEFT JOIN BR_TblPreco T WITH (NOLOCK) ON T.TblPrecoID = E.TblPrecoID
            WHERE E.ClienteID = @ClienteID
            ORDER BY E.CPFCNPJ
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 30 };
        cmd.Parameters.Add("@ClienteID", SqlDbType.Int).Value = clienteId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<ClientAddress>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ClientAddress
            {
                ClienteEnderecoID = ReadNullableInt32(reader, "ClienteEnderecoID") ?? 0,
                Situacao = ReadString(reader, "Situacao"),
                CodSAP = ReadString(reader, "CodSAP"),
                TipoDocumento = ReadString(reader, "TipoDocumento"),
                CPFCNPJ = ReadString(reader, "CPFCNPJ"),
                RazaoSocial = ReadString(reader, "RazaoSocial"),
                NmCidade = ReadString(reader, "NmCidade"),
                CdUF = ReadString(reader, "CdUF"),
                TabelaPreco = ReadString(reader, "TabelaPreco"),
                VlrPedidoMinimo = ReadDecimal(reader, "VlrPedidoMinimo"),
                VlrTaxaEntrega = ReadDecimal(reader, "VlrTaxaEntrega")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<ClientDeliveryLocation>> GetDeliveryLocationsAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT L.ClienteLocalEntregaID,
                   L.CdControle,
                   L.NmLocalEntrega,
                   CNV.NmCanalVenda,
                   E.FlagTipoDocumento AS TipoDocumento,
                   E.CPFCNPJ,
                   CASE(ISNULL(L.FlagAtivo,0))
                        WHEN 0 THEN 'Inativo'
                        ELSE CASE(ISNULL(L.FlagHabilitado,0))
                                WHEN 0 THEN 'Desabilitado'
                                ELSE 'Ativo'
                             END
                   END AS Situacao,
                   CASE ISNULL(L.FlagBloqCredito,0)
                        WHEN 0 THEN 'OK'
                        ELSE 'NOK'
                   END AS SituacaoCredito,
                   CASE ISNULL(L.FlagEnderecoDiferente,0)
                        WHEN 1 THEN 'SIM'
                        ELSE 'NÃO'
                   END AS TipoEndereco,
                   Cid.NmCidade,
                   UF.CdUF
            FROM BR_ClienteEndereco E WITH (NOLOCK)
            JOIN BR_ClienteLocalEntrega L WITH (NOLOCK) ON L.ClienteEnderecoID = E.ClienteEnderecoID
            LEFT JOIN BR_Cidade Cid WITH (NOLOCK) ON Cid.CidadeID = IIF(ISNULL(L.FlagEnderecoDiferente,0) = 1, L.CdCidadeID, E.CdCidadeEnderecoID)
            LEFT JOIN BR_UF UF WITH (NOLOCK) ON UF.UFID = Cid.UFID
            LEFT JOIN BR_CanalVenda CNV WITH (NOLOCK) ON CNV.CanalVendaID = L.CanalVendaID
            WHERE E.ClienteID = @ClienteID
            ORDER BY L.CdControle
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 30 };
        cmd.Parameters.Add("@ClienteID", SqlDbType.Int).Value = clienteId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<ClientDeliveryLocation>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ClientDeliveryLocation
            {
                ClienteLocalEntregaID = ReadNullableInt32(reader, "ClienteLocalEntregaID") ?? 0,
                CdControle = ReadString(reader, "CdControle"),
                NmLocalEntrega = ReadString(reader, "NmLocalEntrega"),
                NmCanalVenda = ReadString(reader, "NmCanalVenda"),
                TipoDocumento = ReadString(reader, "TipoDocumento"),
                CPFCNPJ = ReadString(reader, "CPFCNPJ"),
                Situacao = ReadString(reader, "Situacao"),
                SituacaoCredito = ReadString(reader, "SituacaoCredito"),
                TipoEndereco = ReadString(reader, "TipoEndereco"),
                NmCidade = ReadString(reader, "NmCidade"),
                CdUF = ReadString(reader, "CdUF")
            });
        }

        return items;
    }

    public async Task<IReadOnlyList<ClientUser>> GetUsersAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT U.ClienteUsuarioID,
                   U.Apelido AS Login,
                   U.NmUsuario,
                   U.Email,
                   P.NmPerfil,
                   CASE(ISNULL(U.FlagAtivo,0))
                        WHEN 0 THEN 'Inativo'
                        ELSE CASE(ISNULL(U.FlagBloqueado,0))
                                WHEN 1 THEN 'Bloqueado'
                                ELSE 'Ativo'
                             END
                   END AS Situacao,
                   CASE ISNULL(U.FlagCriaPedido,0)
                        WHEN 0 THEN 'Somente Relatórios'
                        WHEN 2 THEN 'Somente Requisições'
                        ELSE 'Pode Lançar Pedidos'
                   END AS Permissao,
                   CASE ISNULL(U.FlagSoItemContrato,0)
                        WHEN 0 THEN 'Catálogo Completo'
                        ELSE 'Somente Contrato'
                   END AS Catalogo,
                   U.DtCadastro,
                   U.DtUltimoLogin
            FROM BR_ClienteUsuario U WITH (NOLOCK)
            JOIN BR_Perfil P WITH (NOLOCK) ON P.PerfilID = U.PerfilID
            WHERE U.ClienteID = @ClienteID
            ORDER BY U.DtCadastro DESC
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection) { CommandTimeout = 30 };
        cmd.Parameters.Add("@ClienteID", SqlDbType.Int).Value = clienteId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var items = new List<ClientUser>();

        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ClientUser
            {
                ClienteUsuarioID = ReadNullableInt32(reader, "ClienteUsuarioID") ?? 0,
                Login = ReadString(reader, "Login"),
                NmUsuario = ReadString(reader, "NmUsuario"),
                Email = ReadString(reader, "Email"),
                NmPerfil = ReadString(reader, "NmPerfil"),
                Situacao = ReadString(reader, "Situacao"),
                Permissao = ReadString(reader, "Permissao"),
                Catalogo = ReadString(reader, "Catalogo"),
                DtCadastro = ReadNullableDateTime(reader, "DtCadastro"),
                DtUltimoLogin = ReadNullableDateTime(reader, "DtUltimoLogin")
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
