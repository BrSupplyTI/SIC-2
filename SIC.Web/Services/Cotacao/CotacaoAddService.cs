using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace SIC.Web.Services.Cotacao;

/// <summary>
/// Service da tela de criação de cotação.
/// Usa ADO.NET direto, seguindo o padrão de CotacaoQueryService.
/// </summary>
public sealed class CotacaoAddService(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    /// <summary>
    /// Retorna os tipos de cotação permitidos para o usuário informado.
    /// </summary>
    public async Task<List<SelectListItem>> GetTiposAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT t.ID AS CotacaoTipoID, t.NmTipo AS DsCotacaoTipo
            FROM BrWeb..Cotacao_Tipo t WITH (NOLOCK)
            INNER JOIN BrWeb..Intranet_PermissoesUsuario pu WITH (NOLOCK)
                ON pu.PermissaoID = t.PermissaoID
            WHERE t.FlagAtivo = 1
              AND pu.UsuarioID = @UsuarioID
            ORDER BY t.NmTipo
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);

        var items = new List<SelectListItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new SelectListItem
            {
                Value = reader.GetInt32(reader.GetOrdinal("CotacaoTipoID")).ToString(),
                Text = ReadString(reader, "DsCotacaoTipo")
            });
        }

        return items;
    }

    /// <summary>
    /// Retorna os motivos de pedido de bonificação disponíveis para o usuário.
    /// </summary>
    public async Task<List<SelectListItem>> GetMotivosBonificacaoAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT M.Id, M.Descricao
            FROM Integracao_Clientes.dbo.BR_SAP_RelacaoUsuariosMotivos RM WITH (NOLOCK)
            JOIN Integracao_Clientes.dbo.BR_SAP_MotivosPedidos M WITH (NOLOCK)
                ON M.Id = RM.TipoID
            WHERE RM.UsuarioID = @UsuarioID
            ORDER BY M.Descricao
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);

        var items = new List<SelectListItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new SelectListItem
            {
                Value = reader.GetInt32(reader.GetOrdinal("Id")).ToString(),
                Text = ReadString(reader, "Descricao")
            });
        }

        return items;
    }

    /// <summary>
    /// Retorna as condições de pagamento.
    /// </summary>
    public async Task<List<SelectListItem>> GetCondicoesPagamentoAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CondPagtoID, NmCondPagto
            FROM BrSupply..BR_CondPagto WITH (NOLOCK)
            WHERE FlagAtivo = 1
            ORDER BY NmCondPagto
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);

        var items = new List<SelectListItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new SelectListItem
            {
                Value = reader.GetInt32(reader.GetOrdinal("CondPagtoID")).ToString(),
                Text = ReadString(reader, "NmCondPagto")
            });
        }

        return items;
    }

    /// <summary>
    /// Retorna as formas de pagamento.
    /// </summary>
    public async Task<List<SelectListItem>> GetFormasPagamentoAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id,CodFormaPagto,Descricao
            FROM Integracao_Clientes..BR_SAP_FormasPagamento WITH (NOLOCK)
            ORDER BY Descricao
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);

        var items = new List<SelectListItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new SelectListItem
            {
                Value = reader.GetInt32(reader.GetOrdinal("Id")).ToString(),
                Text = $"{ReadString(reader, "CodFormaPagto")} - {ReadString(reader, "Descricao")}"
            });
        }

        return items;
    }

    /// <summary>
    /// Retorna os estabelecimentos ativos com UFID.
    /// </summary>
    public async Task<List<EstabelecimentoOption>> GetEstabelecimentosAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT E.EstabelecimentoID, E.NmEstabelecimento, UFID
            FROM BrSupply..BR_Estabelecimento E WITH (NOLOCK)
            WHERE E.FlagAtivo = 1
              AND E.EstabelecimentoID NOT IN (7,11,12,13,14)
            ORDER BY E.OrdemExibicao
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);

        var items = new List<EstabelecimentoOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new EstabelecimentoOption
            {
                EstabelecimentoId = reader.GetInt32(reader.GetOrdinal("EstabelecimentoID")),
                Nome = ReadString(reader, "NmEstabelecimento"),
                UfId = reader.IsDBNull(reader.GetOrdinal("UFID")) ? 0 : reader.GetInt32(reader.GetOrdinal("UFID"))
            });
        }

        return items;
    }

    /// <summary>
    /// Retorna todas as UFs (UFID → CdUF).
    /// </summary>
    public async Task<List<UfOption>> GetUfsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT UFID, CdUF FROM BrSupply.dbo.BR_UF
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);

        var items = new List<UfOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new UfOption
            {
                UfId = reader.GetInt32(reader.GetOrdinal("UFID")),
                CdUf = ReadString(reader, "CdUF")
            });
        }

        return items;
    }

    /// <summary>
    /// Pesquisa clientes via stored procedure (Select2 AJAX).
    /// </summary>
    public async Task<List<ClienteSearchResult>> SearchClientesAsync(string termo, int estabelecimentoId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SET NOCOUNT ON EXEC BrSupply.dbo.SIC_PesquisaCliente @Termo, @EstabelecimentoID
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Termo", termo);
        cmd.Parameters.AddWithValue("@EstabelecimentoID", estabelecimentoId);

        var items = new List<ClienteSearchResult>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ClienteSearchResult
            {
                Id = reader.GetInt32(reader.GetOrdinal("ClienteID")),
                Text = ReadString(reader, "Cliente")
            });
        }

        return items;
    }

    /// <summary>
    /// Retorna os endereços de um cliente para popular o select de Endereço.
    /// Formato: Bairro | Logradouro | Nº Numero (CdEMS)
    /// </summary>
    public async Task<List<EnderecoOption>> GetEnderecosByClienteAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                ClienteEndereco.ClienteEnderecoID,
                ClienteEndereco.Bairro,
                ClienteEndereco.Logradouro,
                ClienteEndereco.Numero,
                ClienteEndereco.CdEMS,
                BrWeb.dbo.FormataCPFCNPJ(ClienteEndereco.CPFCNPJ) AS CNPJ
            FROM BrSupply.dbo.BR_ClienteEndereco ClienteEndereco (NOLOCK)
            WHERE ClienteEndereco.ClienteID = @ClienteID
            ORDER BY ClienteEndereco.Logradouro
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ClienteID", clienteId);

        var items = new List<EnderecoOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var bairro = ReadString(reader, "Bairro");
            var logradouro = ReadString(reader, "Logradouro");
            var numero = ReadString(reader, "Numero");
            var cdEms = ReadString(reader, "CdEMS");

            items.Add(new EnderecoOption
            {
                ClienteEnderecoId = reader.GetInt32(reader.GetOrdinal("ClienteEnderecoID")),
                Text = $"{ReadString(reader, "CNPJ")} - {bairro} | {logradouro} | Nº {numero} ({cdEms})"
            });
        }

        return items;
    }

    /// <summary>
    /// Retorna os locais de entrega de um endereço para popular o select de Local de Entrega.
    /// Caso não exista nenhum, insere automaticamente o padrão 'Recebimento / Almox' e busca novamente.
    /// </summary>
    public async Task<List<LocalEntregaOption>> GetLocaisEntregaByEnderecoAsync(int clienteEnderecoId, CancellationToken cancellationToken = default)
    {
        const string sqlSelect = """
            SELECT
                CLE.ClienteLocalEntregaID,
                CLE.FlagEnderecoDiferente,
                CLE.NmLocalEntrega,
                CLE.DsLogradouro,
                CLE.CdControle,
                UFLoc.CdUF,
                CidadeLoc.NmCidade AS Cidade,
                CE.Logradouro,
                UFEnd.CdUF AS CdUFEndereco,
                CE.Cidade AS CidadeEndereco,
                CLE.ObsLocalEntrega,
                CE.Bairro,
                CE.CondPagtoID,
                CE.Numero,
                CE.tipoOVSAP
            FROM BrSupply.dbo.BR_ClienteLocalEntrega CLE WITH (NOLOCK)
            LEFT JOIN BrSupply.dbo.BR_Cidade CidadeLoc WITH (NOLOCK)
                ON CidadeLoc.CidadeID = CLE.CdCidadeID
            LEFT JOIN BrSupply.dbo.BR_UF UFLoc WITH (NOLOCK)
                ON UFLoc.UFID = CLE.CdUFID
            LEFT JOIN BrSupply.dbo.BR_ClienteEndereco CE WITH (NOLOCK)
                ON CE.ClienteEnderecoID = CLE.ClienteEnderecoID
            LEFT JOIN BrSupply.dbo.BR_UF UFEnd WITH (NOLOCK)
                ON UFEnd.UFID = CE.UFID
            WHERE CLE.ClienteEnderecoID = @ClienteEnderecoID
              AND CLE.FlagAtivo = 1
            ORDER BY CLE.NmLocalEntrega, CE.Logradouro
            """;

        const string sqlInsert = """
            INSERT INTO BrSupply.dbo.BR_ClienteLocalEntrega (
                CdControle, NmLocalEntrega, CanalVendaID, ClienteEnderecoID,
                FlagAtivo, FlagHabilitado, FlagAceitaRegra, FlagEnderecoDiferente,
                VlrVerbaMensal, VlrUtilizado, VlrPendente
            )
            VALUES ('1', 'Recebimento / Almox', 3, @ClienteEnderecoID, 1, 1, 1, 0, 0, 0, 0)
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var items = await ReadLocaisEntregaAsync(connection, sqlSelect, clienteEnderecoId, cancellationToken);

        if (items.Count == 0)
        {
            await using var insertCmd = new SqlCommand(sqlInsert, connection);
            insertCmd.Parameters.AddWithValue("@ClienteEnderecoID", clienteEnderecoId);
            await insertCmd.ExecuteNonQueryAsync(cancellationToken);

            items = await ReadLocaisEntregaAsync(connection, sqlSelect, clienteEnderecoId, cancellationToken);
        }

        return items;
    }

    private static async Task<List<LocalEntregaOption>> ReadLocaisEntregaAsync(
        SqlConnection connection, string sql, int clienteEnderecoId, CancellationToken cancellationToken)
    {
        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ClienteEnderecoID", clienteEnderecoId);

        var items = new List<LocalEntregaOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var nmLocal   = ReadString(reader, "NmLocalEntrega");
            var logradouro = reader.IsDBNull(reader.GetOrdinal("Logradouro")) ? ReadString(reader, "DsLogradouro") : ReadString(reader, "Logradouro");
            var cdUF      = reader.IsDBNull(reader.GetOrdinal("CdUF"))
                ? (reader.IsDBNull(reader.GetOrdinal("CdUFEndereco")) ? null : ReadString(reader, "CdUFEndereco"))
                : ReadString(reader, "CdUF");
            var cidade    = reader.IsDBNull(reader.GetOrdinal("Cidade"))
                ? (reader.IsDBNull(reader.GetOrdinal("CidadeEndereco")) ? null : ReadString(reader, "CidadeEndereco"))
                : ReadString(reader, "Cidade");
            var cdControle = ReadString(reader, "CdControle");

            items.Add(new LocalEntregaOption
            {
                ClienteLocalEntregaId = reader.GetInt32(reader.GetOrdinal("ClienteLocalEntregaID")),
                Text                  = $"{nmLocal} - {logradouro} ({cdControle})",
                Logradouro            = logradouro,
                CdUF                  = cdUF,
                Cidade                = cidade,
                FlagEnderecoDiferente = reader.GetInt32(reader.GetOrdinal("FlagEnderecoDiferente")),
                CdControle            = cdControle,
                ObsLocalEntrega       = reader.IsDBNull(reader.GetOrdinal("ObsLocalEntrega")) ? null : ReadString(reader, "ObsLocalEntrega"),
                TipoOVSAP             = reader.IsDBNull(reader.GetOrdinal("tipoOVSAP")) ? null : ReadString(reader, "tipoOVSAP"),
                CondPagtoId           = reader.IsDBNull(reader.GetOrdinal("CondPagtoID")) ? null : reader.GetInt32(reader.GetOrdinal("CondPagtoID"))
            });
        }

        return items;
    }

    /// <summary>
    /// Retorna a tabela de preço vinculada ao cliente informado.
    /// </summary>
    public async Task<TabelaPrecoOption?> GetTabelaPrecoByClienteAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TblPrecoID, NmTblPreco
            FROM BrSupply..BR_TblPreco
            WHERE FlagAtivo = 1
              AND ClienteID = @ClienteID
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ClienteID", clienteId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return new TabelaPrecoOption
            {
                TblPrecoId = reader.GetInt32(reader.GetOrdinal("TblPrecoID")),
                NmTblPreco = ReadString(reader, "NmTblPreco")
            };
        }

        return null;
    }

    public async Task<int?> GetFormaPagamentoByClienteAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Cliente.FormaPagamentoSAP
            FROM BrSupply.dbo.BR_Cliente AS Cliente WITH (NOLOCK)
            WHERE Cliente.ClienteID = @ClienteID
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ClienteID", clienteId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is DBNull or null ? null : Convert.ToInt32(result);
    }

    public async Task<string?> GetTipoOVSAPByEnderecoAsync(int clienteEnderecoId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP 1 ClienteEndereco.tipoOVSAP
            FROM BrSupply.dbo.BR_ClienteLocalEntrega AS ClienteLocalEntrega WITH (NOLOCK)
            LEFT JOIN BrSupply.dbo.BR_ClienteEndereco AS ClienteEndereco WITH (NOLOCK)
                ON ClienteEndereco.ClienteEnderecoID = ClienteLocalEntrega.ClienteEnderecoID
            WHERE ClienteLocalEntrega.ClienteEnderecoID = @ClienteEnderecoID
              AND ClienteLocalEntrega.FlagAtivo = 1
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ClienteEnderecoID", clienteEnderecoId);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is DBNull or null ? null : result.ToString();
    }

    /// <summary>
    /// Retorna os tipos de ordem de venda permitidos para o tipo de cotação e usuário informados.
    /// </summary>
    public async Task<List<SelectListItem>> GetTiposOrdemAsync(int cotacaoTipoId, int usuarioId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT T.ID, T.Tipo AS CodTipoOV, T.Tipo + ' - ' + T.Descricao AS TipoOV
            FROM BrWeb..CotacaoTipo_OrdemVenda O (NOLOCK)
                JOIN Integracao_Clientes..BR_SAP_TiposDocumentosPedidos T (NOLOCK) ON T.Id = O.TipoDocumentoPedidoID
                JOIN BrWeb..Cotacao_Tipo C (NOLOCK) ON C.ID = O.CotacaoTipoID
                LEFT JOIN Integracao_Clientes.dbo.BR_SAP_RelacaoUsuariosTiposDocumentos RTD (NOLOCK) ON RTD.TipoId = T.ID
            WHERE O.CotacaoTipoID = @CotacaoTipoID
                AND RTD.UsuarioID = @UsuarioID
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@CotacaoTipoID", cotacaoTipoId);
        cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);

        var items = new List<SelectListItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new SelectListItem
            {
                Value = ReadString(reader, "CodTipoOV"),
                Text = ReadString(reader, "TipoOV")
            });
        }

        return items;
    }

    /// <summary>
    /// Retorna os contratos vigentes de um cliente para popular o select de Nº do Contrato.
    /// </summary>
    public async Task<List<ContratoOption>> GetContratosAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT ClienteID, NrContrato, NrContrato + ' - ' + NmContrato AS Contrato
            FROM BrSupply.dbo.BR_ClienteGestaoContrato
            WHERE ClienteID = @ClienteID
              AND Vigencia >= CAST(GETDATE() AS DATE)
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ClienteID", clienteId);

        var items = new List<ContratoOption>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ContratoOption
            {
                NrContrato = ReadString(reader, "NrContrato"),
                Text = ReadString(reader, "Contrato")
            });
        }

        return items;
    }

    /// <summary>
    /// Retorna as cidades de uma UF para popular o select de Cidade Destino.
    /// </summary>
    public async Task<List<SelectListItem>> GetCidadesByUfAsync(string cdUf, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Cidade.CodigoIBGE, Cidade.NmCidade
            FROM BrSupply..BR_Cidade AS Cidade (NOLOCK)
            INNER JOIN BrSupply..BR_UF AS Estado (NOLOCK)
                ON Cidade.UFID = Estado.UFID
                AND Estado.CdUF = @CdUF
            WHERE Cidade.CidadeID > 0
            ORDER BY Cidade.NmCidade
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@CdUF", cdUf);

        var items = new List<SelectListItem>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new SelectListItem
            {
                Value = reader.GetInt32(reader.GetOrdinal("CodigoIBGE")).ToString(),
                Text = ReadString(reader, "NmCidade")
            });
        }

        return items;
    }

    /// <summary>
    /// Busca os dados de uma proposta existente para pré-popular o formulário de edição.
    /// </summary>
    public async Task<CotacaoEditDados?> GetPropostaParaEditAsync(int propostaId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                P.PropostaId, P.Nome, P.TipoID, P.TipoCotacao, P.EstabelecimentoID,
                P.ClienteId, P.ClienteEnderecoID, P.ClienteLocalEntregaID,
                P.ObsLocalEntrega, P.TabelaPrecoID, P.FlagPrecoConformeTabela,
                P.UfOrigem, P.UfDestino, P.CodigoIBGE, P.MargemPadrao,
                P.DataValidade, P.CondPagto, P.FormaPagamentoSAP, P.tipoOVSAP,
                P.OrdemCompra, P.NrContrato, P.TipoMotivoIDSAP,
                P.ContatoNome, P.ContatoEmail, P.Obs, P.StatusID,
                C.CdExtCliente + ' - ' + C.NmCliente AS ClienteNome,
                T.NmTblPreco,
                S.NmStatus AS StatusNome
            FROM BrWeb.dbo.Proposta P WITH (NOLOCK)
            LEFT JOIN BrSupply.dbo.BR_Cliente C WITH (NOLOCK) ON C.ClienteID = P.ClienteId
            LEFT JOIN BrSupply.dbo.BR_TblPreco T WITH (NOLOCK) ON T.TblPrecoID = P.TabelaPrecoID
            LEFT JOIN BrWeb.dbo.Proposta_Status S WITH (NOLOCK) ON S.StatusID = P.StatusID
            WHERE P.PropostaId = @PropostaId
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@PropostaId", propostaId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new CotacaoEditDados
        {
            PropostaId              = Convert.ToInt32(reader["PropostaId"]),
            Nome                    = ReadString(reader, "Nome"),
            TipoCotacao             = ReadString(reader, "TipoCotacao"),
            EstabelecimentoID       = Convert.ToInt32(reader["EstabelecimentoID"]),
            ClienteId               = Convert.ToInt32(reader["ClienteId"]),
            ClienteNome             = ReadString(reader, "ClienteNome"),
            ClienteEnderecoID       = reader.IsDBNull(reader.GetOrdinal("ClienteEnderecoID")) ? null : Convert.ToInt32(reader["ClienteEnderecoID"]),
            ClienteLocalEntregaID   = reader.IsDBNull(reader.GetOrdinal("ClienteLocalEntregaID")) ? null : Convert.ToInt32(reader["ClienteLocalEntregaID"]),
            ObsLocalEntrega         = reader.IsDBNull(reader.GetOrdinal("ObsLocalEntrega")) ? null : ReadString(reader, "ObsLocalEntrega"),
            TabelaPrecoID           = reader.IsDBNull(reader.GetOrdinal("TabelaPrecoID")) ? null : Convert.ToInt32(reader["TabelaPrecoID"]),
            TabelaPrecoNome         = ReadString(reader, "NmTblPreco"),
            FlagPrecoConformeTabela = !reader.IsDBNull(reader.GetOrdinal("FlagPrecoConformeTabela")) && Convert.ToInt32(reader["FlagPrecoConformeTabela"]) == 1,
            UfOrigem                = ReadString(reader, "UfOrigem"),
            UfDestino               = ReadString(reader, "UfDestino"),
            CodigoIBGE              = reader.IsDBNull(reader.GetOrdinal("CodigoIBGE")) ? null : (int.TryParse(reader["CodigoIBGE"]?.ToString(), out var ibge) ? ibge : null),
            MargemPadrao            = reader.IsDBNull(reader.GetOrdinal("MargemPadrao")) ? null : Convert.ToDecimal(reader["MargemPadrao"]),
            DataValidade            = reader.IsDBNull(reader.GetOrdinal("DataValidade")) ? null : reader.GetDateTime(reader.GetOrdinal("DataValidade")),
            CondPagtoId             = reader.IsDBNull(reader.GetOrdinal("CondPagto")) ? null : Convert.ToInt32(reader["CondPagto"]),
            FormaPagamentoSAP       = reader.IsDBNull(reader.GetOrdinal("FormaPagamentoSAP")) ? null : Convert.ToInt32(reader["FormaPagamentoSAP"]),
            TipoOVSAP               = reader.IsDBNull(reader.GetOrdinal("tipoOVSAP")) ? null : ReadString(reader, "tipoOVSAP"),
            OrdemCompra             = reader.IsDBNull(reader.GetOrdinal("OrdemCompra")) ? null : ReadString(reader, "OrdemCompra"),
            NrContrato              = reader.IsDBNull(reader.GetOrdinal("NrContrato")) ? null : ReadString(reader, "NrContrato"),
            TipoMotivoIDSAP         = reader.IsDBNull(reader.GetOrdinal("TipoMotivoIDSAP")) ? null : Convert.ToInt32(reader["TipoMotivoIDSAP"]),
            ContatoNome             = reader.IsDBNull(reader.GetOrdinal("ContatoNome")) ? null : ReadString(reader, "ContatoNome"),
            ContatoEmail            = reader.IsDBNull(reader.GetOrdinal("ContatoEmail")) ? null : ReadString(reader, "ContatoEmail"),
            Obs                     = reader.IsDBNull(reader.GetOrdinal("Obs")) ? null : ReadString(reader, "Obs"),
            StatusID                = reader.IsDBNull(reader.GetOrdinal("StatusID")) ? 0 : Convert.ToInt32(reader["StatusID"]),
            StatusNome              = ReadString(reader, "StatusNome"),
        };
    }

    /// <summary>
    /// Atualiza os dados de cabeçalho de uma proposta existente.
    /// </summary>
    public async Task AtualizarPropostaAsync(int propostaId, CriarPropostaRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE BrWeb.dbo.Proposta SET
                Nome                    = @Nome,
                TipoCotacao             = @TipoCotacao,
                EstabelecimentoID       = @EstabelecimentoID,
                ClienteId               = @ClienteId,
                ClienteEnderecoID       = @ClienteEnderecoID,
                ClienteLocalEntregaID   = @ClienteLocalEntregaID,
                ObsLocalEntrega         = @ObsLocalEntrega,
                TabelaPrecoID           = @TabelaPrecoID,
                FlagPrecoConformeTabela = @FlagPrecoConformeTabela,
                UfOrigem                = @UfOrigem,
                UfDestino               = @UfDestino,
                CodigoIBGE              = @CodigoIBGE,
                MargemPadrao            = @MargemPadrao,
                DataValidade            = @DataValidade,
                CondPagto               = @CondPagto,
                FormaPagamentoSAP       = @FormaPagamentoSAP,
                tipoOVSAP               = @TipoOVSAP,
                OrdemCompra             = @OrdemCompra,
                NrContrato              = @NrContrato,
                TipoMotivoIDSAP         = @TipoMotivoIDSAP,
                ContatoNome             = @ContatoNome,
                ContatoEmail            = @ContatoEmail,
                Obs                     = @Obs
            WHERE PropostaId = @PropostaId
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@PropostaId",              propostaId);
        cmd.Parameters.AddWithValue("@Nome",                    (object?)request.Nome ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TipoCotacao",             (object?)request.TipoNome ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@EstabelecimentoID",       request.EstabelecimentoID);
        cmd.Parameters.AddWithValue("@ClienteId",               request.ClienteId);
        cmd.Parameters.AddWithValue("@ClienteEnderecoID",       (object?)request.ClienteEnderecoID ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ClienteLocalEntregaID",   (object?)request.ClienteLocalEntregaID ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ObsLocalEntrega",         (object?)request.ObsLocalEntrega ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TabelaPrecoID",           (object?)request.TabelaPrecoID ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FlagPrecoConformeTabela", request.FlagPrecoConformeTabela ? 1 : 0);
        cmd.Parameters.AddWithValue("@UfOrigem",                (object?)request.UfOrigem ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@UfDestino",               (object?)request.UfDestino ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CodigoIBGE",              (object?)request.CodigoIBGE ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@MargemPadrao",            (object?)request.MargemPadrao ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DataValidade",            (object?)request.DataValidade ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CondPagto",               (object?)request.CondPagtoId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FormaPagamentoSAP",       (object?)request.FormaPagamentoSAP ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TipoOVSAP",               (object?)request.TipoOVSAP ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@OrdemCompra",             (object?)request.OrdemCompra ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@NrContrato",              (object?)request.NrContrato ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TipoMotivoIDSAP",         (object?)request.TipoMotivoIDSAP ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ContatoNome",             (object?)request.ContatoNome ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ContatoEmail",            (object?)request.ContatoEmail ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Obs",                     (object?)request.Obs ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Insere uma nova Proposta na tabela BrWeb.dbo.Proposta e retorna o PropostaId gerado.
    /// </summary>
    public async Task<int> CriarPropostaAsync(CriarPropostaRequest request, CancellationToken cancellationToken = default)
    {
        const string insertSql = """
            INSERT INTO BrWeb.dbo.Proposta
            (
                Nome, TipoCotacao, TipoID, EstabelecimentoID, ClienteId, ClienteEnderecoID, ClienteLocalEntregaID,
                ObsLocalEntrega, TabelaPrecoID, FlagPrecoConformeTabela,
                UfOrigem, UfDestino, CodigoIBGE, MargemPadrao,
                DataValidade, CondPagto, FormaPagamentoSAP, tipoOVSAP, OrdemCompra, NrContrato,
                TipoMotivoIDSAP,
                ContatoNome, ContatoEmail, Obs,
                UsuarioId, DtCriacao, Versao, StatusID, NatOperacao,
                ValorVendaTotal, Frete, VlrPedidoMinimo
            )
            OUTPUT INSERTED.PropostaId
            VALUES
            (
                @Nome, @TipoCotacao, 2, @EstabelecimentoID, @ClienteId, @ClienteEnderecoID, @ClienteLocalEntregaID,
                @ObsLocalEntrega, @TabelaPrecoID, @FlagPrecoConformeTabela,
                @UfOrigem, @UfDestino, @CodigoIBGE, @MargemPadrao,
                @DataValidade, @CondPagto, @FormaPagamentoSAP, @TipoOVSAP, @OrdemCompra, @NrContrato,
                @TipoMotivoIDSAP,
                @ContatoNome, @ContatoEmail, @Obs,
                @UsuarioId, GETDATE(), 1, 1, 1,
                @ValorVendaTotal, @Frete, @VlrPedidoMinimo
            )
            """;

        const string updateCdProposta = """
            UPDATE BrWeb.dbo.Proposta
            SET CdProposta = @CdProposta
            WHERE PropostaId = @PropostaId
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var insertCmd = new SqlCommand(insertSql, connection);
        insertCmd.Parameters.AddWithValue("@Nome", (object?)request.Nome ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@TipoCotacao", (object?)request.TipoNome ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@EstabelecimentoID", request.EstabelecimentoID);
        insertCmd.Parameters.AddWithValue("@ClienteId", request.ClienteId);
        insertCmd.Parameters.AddWithValue("@ClienteEnderecoID", (object?)request.ClienteEnderecoID ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@ClienteLocalEntregaID", (object?)request.ClienteLocalEntregaID ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@ObsLocalEntrega", (object?)request.ObsLocalEntrega ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@TabelaPrecoID", (object?)request.TabelaPrecoID ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@FlagPrecoConformeTabela", request.FlagPrecoConformeTabela ? 1 : 0);
        insertCmd.Parameters.AddWithValue("@UfOrigem", (object?)request.UfOrigem ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@UfDestino", (object?)request.UfDestino ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@CodigoIBGE", (object?)request.CodigoIBGE ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@MargemPadrao", (object?)request.MargemPadrao ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@DataValidade", (object?)request.DataValidade ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@CondPagto", (object?)request.CondPagtoId ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@FormaPagamentoSAP", (object?)request.FormaPagamentoSAP ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@TipoOVSAP", (object?)request.TipoOVSAP ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@OrdemCompra", (object?)request.OrdemCompra ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@NrContrato", (object?)request.NrContrato ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@TipoMotivoIDSAP", (object?)request.TipoMotivoIDSAP ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@ContatoNome", (object?)request.ContatoNome ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@ContatoEmail", (object?)request.ContatoEmail ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@Obs", (object?)request.Obs ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@UsuarioId", request.UsuarioId);
        insertCmd.Parameters.AddWithValue("@ValorVendaTotal", request.ValorVendaTotal);
        insertCmd.Parameters.AddWithValue("@Frete", request.Frete);
        insertCmd.Parameters.AddWithValue("@VlrPedidoMinimo", request.VlrPedidoMinimo);

        var propostaId = (int)(await insertCmd.ExecuteScalarAsync(cancellationToken))!;

        // Prefixo: "PR" se TipoID = 1, "CT" se TipoID = 2
        var prefixo = request.TipoID == 2 ? "CT" : "PR";
        var sufixo = request.EstabelecimentoID switch
        {
            1 => "MTZ",
            2 => "FSP",
            3 => "TSL",
            4 => "TPA",
            5 => "BPN",
            6 => "FBR",
            7 => "SPA",
            8 => "KPX",
            9 => "STP",
            _ => string.Empty
        };
        var cdProposta = $"{prefixo}{propostaId:D6}{sufixo}";

        await using var updateCmd = new SqlCommand(updateCdProposta, connection);
        updateCmd.Parameters.AddWithValue("@CdProposta", cdProposta);
        updateCmd.Parameters.AddWithValue("@PropostaId", propostaId);
        await updateCmd.ExecuteNonQueryAsync(cancellationToken);

        return propostaId;
    }

    /// <summary>
    /// Busca frete e pedido mínimo inicial em cascata:
    /// 1) ClienteEndereco → 2) Cliente → 3) Política canal+UF → 4) Política geral UF
    /// Para no primeiro VlrTaxaEntrega > 0.
    /// </summary>
    public async Task<(decimal Frete, decimal VlrPedidoMinimo)> BuscarFreteInicialAsync(
        int clienteEnderecoId, int clienteId, string? ufDestino, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // 1) Endereço do cliente
        const string sqlEndereco = """
            SELECT CONVERT(NUMERIC(10,2), ISNULL(VlrTaxaEntrega, 0)) AS VlrTaxaEntrega,
                   ISNULL(VlrPedidoMinimo, 0) AS VlrPedidoMinimo
            FROM BrSupply.dbo.BR_ClienteEndereco
            WHERE ClienteEnderecoID = @ClienteEnderecoID
            """;
        await using (var cmd = new SqlCommand(sqlEndereco, connection))
        {
            cmd.Parameters.AddWithValue("@ClienteEnderecoID", clienteEnderecoId);
            await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await r.ReadAsync(cancellationToken))
            {
                var frete = r.GetDecimal(r.GetOrdinal("VlrTaxaEntrega"));
                var min   = r.GetDecimal(r.GetOrdinal("VlrPedidoMinimo"));
                if (frete > 0) return (frete, min);
            }
        }

        // 2) Cliente
        const string sqlCliente = """
            SELECT ISNULL(VlrTaxaEntrega, 0) AS VlrTaxaEntrega,
                   ISNULL(VlrPedidoMinimo, 0) AS VlrPedidoMinimo
            FROM BrSupply.dbo.BR_Cliente
            WHERE ClienteID = @ClienteID
            """;
        await using (var cmd = new SqlCommand(sqlCliente, connection))
        {
            cmd.Parameters.AddWithValue("@ClienteID", clienteId);
            await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
            if (await r.ReadAsync(cancellationToken))
            {
                var frete = r.GetDecimal(r.GetOrdinal("VlrTaxaEntrega"));
                var min   = r.GetDecimal(r.GetOrdinal("VlrPedidoMinimo"));
                if (frete > 0) return (frete, min);
            }
        }

        if (!string.IsNullOrWhiteSpace(ufDestino))
        {
            // 3) Política canal de venda + UF
            const string sqlCanal = """
                SELECT PF.VlrTaxaEntrega, ISNULL(PF.VlrPedidoMinimo, 0) AS VlrPedidoMinimo
                FROM BrSupply.dbo.BR_PoliticaFrete PF
                INNER JOIN BrSupply.dbo.BR_UF UF ON UF.UFID = PF.UFID
                WHERE PF.CanalVendaID = (
                    SELECT CanalVendaID FROM BrSupply.dbo.BR_Cliente WHERE ClienteID = @ClienteID
                )
                AND UF.CdUF = @UfDestino
                """;
            await using (var cmd = new SqlCommand(sqlCanal, connection))
            {
                cmd.Parameters.AddWithValue("@ClienteID", clienteId);
                cmd.Parameters.AddWithValue("@UfDestino", ufDestino);
                await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await r.ReadAsync(cancellationToken))
                {
                    var frete = r.IsDBNull(r.GetOrdinal("VlrTaxaEntrega")) ? 0m : r.GetDecimal(r.GetOrdinal("VlrTaxaEntrega"));
                    var min   = r.GetDecimal(r.GetOrdinal("VlrPedidoMinimo"));
                    if (frete > 0) return (frete, min);
                }
            }

            // 4) Política geral por UF
            const string sqlUf = """
                SELECT PF.VlrTaxaEntrega, ISNULL(PF.VlrPedidoMinimo, 0) AS VlrPedidoMinimo
                FROM BrSupply.dbo.BR_PoliticaFrete PF
                INNER JOIN BrSupply.dbo.BR_UF UF ON UF.UFID = PF.UFID
                WHERE UF.CdUF = @UfDestino
                  AND (PF.CanalVendaID IS NULL OR PF.CanalVendaID = 0)
                """;
            await using (var cmd = new SqlCommand(sqlUf, connection))
            {
                cmd.Parameters.AddWithValue("@UfDestino", ufDestino);
                await using var r = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await r.ReadAsync(cancellationToken))
                {
                    var frete = r.IsDBNull(r.GetOrdinal("VlrTaxaEntrega")) ? 0m : r.GetDecimal(r.GetOrdinal("VlrTaxaEntrega"));
                    var min   = r.GetDecimal(r.GetOrdinal("VlrPedidoMinimo"));
                    if (frete > 0) return (frete, min);
                }
            }
        }

        return (0m, 0m);
    }

    private static string ReadString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }
}

public sealed class CriarPropostaRequest
{
    public string? Nome { get; set; }
    public int TipoID { get; set; }
    public string? TipoNome { get; set; }
    public int EstabelecimentoID { get; set; }
    public int ClienteId { get; set; }
    public int? ClienteEnderecoID { get; set; }
    public int? ClienteLocalEntregaID { get; set; }
    public string? ObsLocalEntrega { get; set; }
    public int? TabelaPrecoID { get; set; }
    public bool FlagPrecoConformeTabela { get; set; }
    public string? UfOrigem { get; set; }
    public string? UfDestino { get; set; }
    public int? CodigoIBGE { get; set; }
    public decimal? MargemPadrao { get; set; }
    public DateTime? DataValidade { get; set; }
    public int? CondPagtoId { get; set; }
    public int? FormaPagamentoSAP { get; set; }
    public string? TipoOVSAP { get; set; }
    public string? OrdemCompra { get; set; }
    public string? NrContrato { get; set; }
    public int? TipoMotivoIDSAP { get; set; }
    public string? NrChamado { get; set; }
    public int? PedidoOriginalID { get; set; }
    public string? ContatoNome { get; set; }
    public string? ContatoEmail { get; set; }
    public string? Obs { get; set; }
    public int UsuarioId { get; set; }
    public decimal ValorVendaTotal { get; set; }
    public decimal Frete { get; set; }
    public decimal VlrPedidoMinimo { get; set; }
}

public sealed class ClienteSearchResult
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}

public sealed class EstabelecimentoOption
{
    public int EstabelecimentoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int UfId { get; set; }
}

public sealed class UfOption
{
    public int UfId { get; set; }
    public string CdUf { get; set; } = string.Empty;
}

public sealed class EnderecoOption
{
    public int ClienteEnderecoId { get; set; }
    public string Text { get; set; } = string.Empty;
}

public sealed class LocalEntregaOption
{
    public int ClienteLocalEntregaId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Logradouro { get; set; }
    public string? CdUF { get; set; }
    public string? Cidade { get; set; }
    public int FlagEnderecoDiferente { get; set; }
    public string? CdControle { get; set; }
    public string? ObsLocalEntrega { get; set; }
    public string? TipoOVSAP { get; set; }
    public int? CondPagtoId { get; set; }
}

public sealed class TabelaPrecoOption
{
    public int TblPrecoId { get; set; }
    public string NmTblPreco { get; set; } = string.Empty;
}

public sealed class ContratoOption
{
    public string NrContrato { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public sealed class CotacaoEditDados
{
    public int PropostaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string TipoCotacao { get; set; } = string.Empty;
    public int EstabelecimentoID { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int? ClienteEnderecoID { get; set; }
    public int? ClienteLocalEntregaID { get; set; }
    public string? ObsLocalEntrega { get; set; }
    public int? TabelaPrecoID { get; set; }
    public string TabelaPrecoNome { get; set; } = string.Empty;
    public bool FlagPrecoConformeTabela { get; set; }
    public string UfOrigem { get; set; } = string.Empty;
    public string UfDestino { get; set; } = string.Empty;
    public int? CodigoIBGE { get; set; }
    public decimal? MargemPadrao { get; set; }
    public DateTime? DataValidade { get; set; }
    public int? CondPagtoId { get; set; }
    public int? FormaPagamentoSAP { get; set; }
    public string? TipoOVSAP { get; set; }
    public string? OrdemCompra { get; set; }
    public string? NrContrato { get; set; }
    public int? TipoMotivoIDSAP { get; set; }
    public string? ContatoNome { get; set; }
    public string? ContatoEmail { get; set; }
    public string? Obs { get; set; }
    public int StatusID { get; set; }
    public string StatusNome { get; set; } = string.Empty;
}
