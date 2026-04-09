using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions.PrePedidosPDF;
using System.Data;

namespace SIC.Infrastructure.Repositories.PrePedidosPDF;

/// <summary>
/// Implementação SQL das operações de escrita do pré-pedido.
/// </summary>
public sealed class SqlPrePedidoPDFCommandRepository(IConfiguration configuration) : IPrePedidoPDFCommandRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    public Task<bool> AtualizarEnderecoAsync(
        int prePedidoId,
        int clienteEnderecoId,
        string logradouro,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            UPDATE Integracao_Clientes.dbo.PDF_PrePedido
               SET ClienteEnderecoID = @ClienteEnderecoID
             WHERE PDFPrePedidoID = @PDFPrePedidoID;

            INSERT INTO Integracao_Clientes.dbo.PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo)
            VALUES (@Mensagem, GETDATE(), @PDFPrePedidoID, 'Atualização');
            """,
            parameters =>
            {
                parameters.AddWithValue("@ClienteEnderecoID", clienteEnderecoId);
                parameters.AddWithValue("@PDFPrePedidoID", prePedidoId);
                parameters.AddWithValue("@Mensagem", $"Endereço atualizado para: {clienteEnderecoId} - {logradouro}");
            },
            cancellationToken);

    public Task<bool> AtualizarLocalEntregaAsync(
        int prePedidoId,
        int clienteLocalEntregaId,
        string nomeLocalEntrega,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            UPDATE Integracao_Clientes.dbo.PDF_PrePedido
               SET ClienteLocalEntregaID = @ClienteLocalEntregaID
             WHERE PDFPrePedidoID = @PDFPrePedidoID;

            INSERT INTO Integracao_Clientes.dbo.PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo)
            VALUES (@Mensagem, GETDATE(), @PDFPrePedidoID, 'Atualização');
            """,
            parameters =>
            {
                parameters.AddWithValue("@ClienteLocalEntregaID", clienteLocalEntregaId);
                parameters.AddWithValue("@PDFPrePedidoID", prePedidoId);
                parameters.AddWithValue("@Mensagem", $"Local de entrega atualizado para: {clienteLocalEntregaId} - {nomeLocalEntrega}");
            },
            cancellationToken);

    public Task<bool> AtualizarCnpjAsync(
        int prePedidoId,
        string cnpj,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            UPDATE Integracao_Clientes.dbo.PDF_PrePedido
               SET CNPJ = @CNPJ
             WHERE PDFPrePedidoID = @PDFPrePedidoID;

            INSERT INTO Integracao_Clientes.dbo.PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo)
            VALUES (@Mensagem, GETDATE(), @PDFPrePedidoID, 'Atualização');
            """,
            parameters =>
            {
                parameters.AddWithValue("@CNPJ", cnpj);
                parameters.AddWithValue("@PDFPrePedidoID", prePedidoId);
                parameters.AddWithValue("@Mensagem", $"CNPJ atualizado para: {cnpj}");
            },
            cancellationToken);

    public Task<bool> UpdateQuantidadeAsync(
        int prePedidoItemId,
        int prePedidoId,
        int quantidade,
        string descricao,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            UPDATE Integracao_Clientes.dbo.PDF_PrePedidoItem
               SET Quantidade = @Quantidade
             WHERE PDFPrePedidoItemID = @PDFPrePedidoItemID;

            INSERT INTO Integracao_Clientes.dbo.PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo)
            VALUES (@Mensagem, GETDATE(), @PDFPrePedidoID, 'Atualização');
            """,
            parameters =>
            {
                parameters.AddWithValue("@Quantidade", quantidade);
                parameters.AddWithValue("@PDFPrePedidoItemID", prePedidoItemId);
                parameters.AddWithValue("@PDFPrePedidoID", prePedidoId);
                parameters.AddWithValue("@Mensagem", $"Quantidade atualizada para o item: {descricao} - Nova Quantidade: {quantidade}");
            },
            cancellationToken);

    public Task<bool> CancelarAsync(
        int prePedidoId,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            UPDATE Integracao_Clientes.dbo.PDF_PrePedido
               SET StatusPrePedidoID = 5
             WHERE PDFPrePedidoID = @PDFPrePedidoID;

            INSERT INTO Integracao_Clientes.dbo.PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo)
            VALUES ('Pre-pedido cancelado!', GETDATE(), @PDFPrePedidoID, 'Aviso');
            """,
            parameters =>
            {
                parameters.AddWithValue("@PDFPrePedidoID", prePedidoId);
            },
            cancellationToken);

    public Task<bool> ExcluirItemAsync(
        int prePedidoItemId,
        int prePedidoId,
        string descricao,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            DELETE FROM Integracao_Clientes.dbo.PDF_PrePedidoItem
             WHERE PDFPrePedidoItemID = @PDFPrePedidoItemID
               AND PDFPrePedidoID = @PDFPrePedidoID;

            INSERT INTO Integracao_Clientes.dbo.PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo)
            VALUES (@Mensagem, GETDATE(), @PDFPrePedidoID, 'Exclusão');
            """,
            parameters =>
            {
                parameters.AddWithValue("@PDFPrePedidoItemID", prePedidoItemId);
                parameters.AddWithValue("@PDFPrePedidoID", prePedidoId);
                parameters.AddWithValue("@Mensagem", $"Item excluído: {descricao}");
            },
            cancellationToken);

    public Task<bool> GravarTrocaItemAsync(
        int prePedidoItemId,
        int prePedidoId,
        string cdItem,
        int itemId,
        string nomeItem,
        decimal vlrTabelaPreco,
        string cdItemAntigo,
        string descricaoAntiga,
        string valorAntigo,
        string motivoTrocaItem,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            UPDATE Integracao_Clientes.dbo.PDF_PrePedidoItem
               SET CdItem = @CdItem,
                   ItemID = @ItemID,
                   Descricao = @NmItem,
                   TblPrecoValorUnitario = @VlrTabelaPreco
             WHERE PDFPrePedidoItemID = @PDFPrePedidoItemID;

            INSERT INTO Integracao_Clientes.dbo.PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo)
            VALUES (@Mensagem, GETDATE(), @PDFPrePedidoID, 'Troca');
            """,
            parameters =>
            {
                parameters.AddWithValue("@CdItem", cdItem);
                parameters.AddWithValue("@ItemID", itemId);
                parameters.AddWithValue("@NmItem", nomeItem);
                parameters.AddWithValue("@VlrTabelaPreco", vlrTabelaPreco);
                parameters.AddWithValue("@PDFPrePedidoItemID", prePedidoItemId);
                parameters.AddWithValue("@PDFPrePedidoID", prePedidoId);
                parameters.AddWithValue("@Mensagem", $"Item Substituído - DE: {cdItemAntigo} - {descricaoAntiga} - {valorAntigo} | PARA: {cdItem} - {nomeItem} - {vlrTabelaPreco} | Motivo: {motivoTrocaItem}");
            },
            cancellationToken);

    public Task<bool> AdicionarItemAsync(
        int prePedidoId,
        string cdItem,
        string descricao,
        int quantidade,
        decimal vlrTabelaPreco,
        string cdItemCliente,
        int itemId,
        string ordemCompra,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            INSERT INTO Integracao_Clientes.dbo.PDF_PrePedidoItem
                (PDFPrePedidoID, CdItem, Descricao, Quantidade, TblPrecoValorUnitario, ValorUnitario, CdItemCliente, ItemID, Sequencia)
            VALUES
                (@PDFPrePedidoID, @CdItem, @Descricao, @Quantidade, @VlrTabelaPreco, @VlrTabelaPreco, @CdItemCliente, @ItemID,
                 (SELECT ISNULL(MAX(Sequencia), 0) + 1 FROM Integracao_Clientes.dbo.PDF_PrePedidoItem WHERE PDFPrePedidoID = @PDFPrePedidoID));

            INSERT INTO Integracao_Clientes.dbo.PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo)
            VALUES (@Mensagem, GETDATE(), @PDFPrePedidoID, 'Inclusão');
            """,
            parameters =>
            {
                parameters.AddWithValue("@PDFPrePedidoID", prePedidoId);
                parameters.AddWithValue("@CdItem", cdItem);
                parameters.AddWithValue("@Descricao", descricao);
                parameters.AddWithValue("@Quantidade", quantidade);
                parameters.AddWithValue("@VlrTabelaPreco", vlrTabelaPreco);
                parameters.AddWithValue("@CdItemCliente", cdItemCliente);
                parameters.AddWithValue("@ItemID", itemId);
                parameters.AddWithValue("@Mensagem", $"Item adicionado: {cdItem} - {descricao} - Qtde: {quantidade} - OC: {ordemCompra}");
            },
            cancellationToken);

    public Task<bool> SetProcessadorPraZeroAsync(
        int prePedidoId,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            DECLARE @ArquivoPrePedidoId INT;

            SELECT @ArquivoPrePedidoId = ArquivoPrePedidoId
              FROM Integracao_Clientes.dbo.PDF_PrePedido
             WHERE PDFPrePedidoID = @PDFPrePedidoID;

            UPDATE Integracao_Clientes.dbo.PDF_ArquivoPrePedido
               SET Processado = 0
             WHERE PDFArquivoPrePedidoID = @ArquivoPrePedidoId;
            """,
            parameters =>
            {
                parameters.AddWithValue("@PDFPrePedidoID", prePedidoId);
            },
            cancellationToken);

    public Task<bool> InserirLogReprocessamentoAsync(
        int prePedidoId,
        string mensagem,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            INSERT INTO Integracao_Clientes.dbo.PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo)
            VALUES (@Mensagem, GETDATE(), @PDFPrePedidoID, 'Reprocessamento');
            """,
            parameters =>
            {
                parameters.AddWithValue("@PDFPrePedidoID", prePedidoId);
                parameters.AddWithValue("@Mensagem", mensagem);
            },
            cancellationToken);

    public Task<bool> AtualizarStatusAguardandoAsync(
        int prePedidoId,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            UPDATE Integracao_Clientes.dbo.PDF_PrePedido
               SET StatusPrePedidoID = 1
             WHERE PDFPrePedidoID = @PDFPrePedidoID;

            INSERT INTO Integracao_Clientes.dbo.PDF_PrePedidoLog (Mensagem, CriadoEm, PDFPrePedidoID, Tipo)
            VALUES ('Status atualizado para Aguardando após reprocessamento.', GETDATE(), @PDFPrePedidoID, 'Reprocessamento');
            """,
            parameters =>
            {
                parameters.AddWithValue("@PDFPrePedidoID", prePedidoId);
            },
            cancellationToken);

    public async Task<int> GerarPedidoAsync(
        int estabelecimentoId,
        int clienteId,
        int clienteEnderecoId,
        string cnpj,
        int clienteLocalEntregaId,
        int clienteUsuarioId,
        int naturezaOperacaoId,
        int condPagtoId,
        string ordemCompra,
        int? clienteCategoriaPedidoId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SET NOCOUNT ON
            EXEC BrSupply.dbo.BR_sp_InsertCotacao
                @EstabelecimentoID,
                @ClienteID,
                @ClienteEnderecoID,
                @CNPJ,
                @ClienteLocalEntregaID,
                @ClienteUsuarioID,
                @NaturezaOperacaoID,
                @CondPagtoID,
                @OrdemCompra,
                @ClienteCategoriaPedidoID
            """;

        await using var cmd = new SqlCommand(sql, connection) { CommandType = CommandType.Text };
        cmd.Parameters.AddWithValue("@EstabelecimentoID", estabelecimentoId);
        cmd.Parameters.AddWithValue("@ClienteID", clienteId);
        cmd.Parameters.AddWithValue("@ClienteEnderecoID", clienteEnderecoId);
        cmd.Parameters.AddWithValue("@CNPJ", cnpj);
        cmd.Parameters.AddWithValue("@ClienteLocalEntregaID", clienteLocalEntregaId);
        cmd.Parameters.AddWithValue("@ClienteUsuarioID", clienteUsuarioId);
        cmd.Parameters.AddWithValue("@NaturezaOperacaoID", naturezaOperacaoId);
        cmd.Parameters.AddWithValue("@CondPagtoID", condPagtoId);
        cmd.Parameters.AddWithValue("@OrdemCompra", ordemCompra);
        cmd.Parameters.AddWithValue("@ClienteCategoriaPedidoID", clienteCategoriaPedidoId.HasValue ? clienteCategoriaPedidoId.Value : DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            var ordinal = reader.GetOrdinal("ID");
            return reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal));
        }

        return 0;
    }

    public Task<bool> AtualizarCotacaoStatusAsync(
        int prePedidoId,
        int cotacaoId,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            """
            UPDATE Integracao_Clientes.dbo.PDF_PrePedido
               SET CotacaoID = @CotacaoID,
                   StatusPrePedidoID = 4
             WHERE PDFPrePedidoID = @PDFPrePedidoID;
            """,
            parameters =>
            {
                parameters.AddWithValue("@CotacaoID", cotacaoId);
                parameters.AddWithValue("@PDFPrePedidoID", prePedidoId);
            },
            cancellationToken);

    public async Task<bool> GerarItemPedidoAsync(
        int cotacaoId,
        int tipo,
        int itemId,
        int qtItem,
        decimal vlrUnit,
        string cdItemCliente,
        string ordemCliente,
        int seqCliente,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SET NOCOUNT ON
            EXEC BrSupply.dbo.BR_sp_InsertCotacaoItem
                @CotacaoID,
                @Tipo,
                @ItemID,
                @QtItem,
                @VlrUnit,
                @CdItemCliente,
                @OrdemCliente,
                @SeqCliente
            """;

        await using var cmd = new SqlCommand(sql, connection) { CommandType = CommandType.Text };
        cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
        cmd.Parameters.AddWithValue("@Tipo", tipo);
        cmd.Parameters.AddWithValue("@ItemID", itemId);
        cmd.Parameters.AddWithValue("@QtItem", qtItem);
        cmd.Parameters.AddWithValue("@VlrUnit", vlrUnit);
        cmd.Parameters.AddWithValue("@CdItemCliente", cdItemCliente);
        cmd.Parameters.AddWithValue("@OrdemCliente", ordemCliente);
        cmd.Parameters.AddWithValue("@SeqCliente", seqCliente);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
        return true;
    }

    private async Task<bool> ExecuteAsync(
        string sql,
        Action<SqlParameterCollection> addParameters,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection)
        {
            CommandType = CommandType.Text
        };

        addParameters(command.Parameters);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }
}
