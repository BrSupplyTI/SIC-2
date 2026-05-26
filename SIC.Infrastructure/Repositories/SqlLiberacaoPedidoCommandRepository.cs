using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIC.Domain.Abstractions;
using System.Data;

namespace SIC.Infrastructure.Repositories;

/// <summary>
/// Comandos de escrita da tela de Liberação de Pedido. Conversão fiel de comercial_liberacao_updates.php,
/// parametrizando os valores (o PHP interpolava diretamente na string).
/// Cada operação grava um bloco de log em BrSupply..BR_CotLog e Integracao_Clientes..BR_BackOfficeLog.
/// </summary>
public sealed class SqlLiberacaoPedidoCommandRepository(IConfiguration configuration) : ILiberacaoPedidoCommandRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("SicDatabase")
        ?? throw new InvalidOperationException("ConnectionStrings:SicDatabase não configurada.");

    // ----------------------------------------------------------------------
    // OBSERVAÇÕES (Nota / Solicitante / Aprovador)
    // ----------------------------------------------------------------------

    public Task AlterarObsNotaAsync(int cotacaoId, int usuarioId, string obsAntiga, string obsNova, string motivo, CancellationToken cancellationToken = default)
        => AlterarObservacaoGenericaAsync(
            cotacaoId, usuarioId,
            colunaUpdate: "ObsNota",
            obsAntiga, obsNova, motivo,
            descricaoAcao: "Alterou a Observação da Nota Fiscal do Pedido",
            cancellationToken);

    public Task AlterarObsSolicitanteAsync(int cotacaoId, int usuarioId, string obsAntiga, string obsNova, string motivo, CancellationToken cancellationToken = default)
        => AlterarObservacaoGenericaAsync(
            cotacaoId, usuarioId,
            colunaUpdate: "ObsCotacao",
            obsAntiga, obsNova, motivo,
            descricaoAcao: "Alterou a Observação do Comprador do Pedido",
            cancellationToken);

    public Task AlterarObsAprovadorAsync(int cotacaoId, int usuarioId, string obsAntiga, string obsNova, string motivo, CancellationToken cancellationToken = default)
        => AlterarObservacaoGenericaAsync(
            cotacaoId, usuarioId,
            colunaUpdate: "ObsAprovacao",
            obsAntiga, obsNova, motivo,
            descricaoAcao: "Alterou a Observação do Aprovador do Pedido",
            cancellationToken);

    private async Task AlterarObservacaoGenericaAsync(
        int cotacaoId, int usuarioId,
        string colunaUpdate, string obsAntiga, string obsNova, string motivo,
        string descricaoAcao, CancellationToken cancellationToken)
    {
        // colunaUpdate é controlada pelo caller (literal). Whitelist para defesa em profundidade.
        if (colunaUpdate is not ("ObsNota" or "ObsCotacao" or "ObsAprovacao"))
            throw new ArgumentException("Coluna inválida.", nameof(colunaUpdate));

        var sql = $@"
            UPDATE BrSupply.dbo.BR_Cotacao
               SET {colunaUpdate} = @ObsNova
             WHERE CotacaoID = @CotacaoID;

            INSERT INTO BrSupply.dbo.BR_CotLog (CotacaoID, UsuarioID, TipoOperacao, Modificacao, DtOperacao)
            VALUES (@CotacaoID, @UsuarioID, 'R',
                    @DescAcao + ' | Valor Antigo: ' + @ObsAntiga + ' | Valor Novo: ' + @ObsNova + ' | Motivo: ' + @Motivo,
                    GETDATE());

            INSERT INTO Integracao_Clientes.dbo.BR_BackOfficeLog (CotacaoID, Motivo, DsAcao, DataHora, UsuarioID)
            VALUES (@CotacaoID,
                    'Motivo: ' + @Motivo + ' | Valor Antigo: ' + @ObsAntiga + ' | Valor Novo: ' + @ObsNova,
                    @DescAcao, GETDATE(), @UsuarioID);";

        await ExecuteAsync(sql, cancellationToken, cmd =>
        {
            cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@ObsAntiga", (object?)obsAntiga ?? string.Empty);
            cmd.Parameters.AddWithValue("@ObsNova", (object?)obsNova ?? string.Empty);
            cmd.Parameters.AddWithValue("@Motivo", (object?)motivo ?? string.Empty);
            cmd.Parameters.AddWithValue("@DescAcao", descricaoAcao);
        });
    }

    // ----------------------------------------------------------------------
    // ORDEM DE COMPRA
    // ----------------------------------------------------------------------

    public async Task AlterarOrdemCompraAsync(int cotacaoId, int usuarioId, string ordemAntiga, string ordemNova, string motivo, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE BrSupply.dbo.BR_Cotacao
               SET OrdemCompra = @OrdemNova
             WHERE CotacaoID = @CotacaoID;

            INSERT INTO BrSupply.dbo.BR_CotLog (CotacaoID, UsuarioID, TipoOperacao, Modificacao, DtOperacao)
            VALUES (@CotacaoID, @UsuarioID, 'R',
                    'Alterou Ordem de Compra do Pedido | Valor Antigo: ' + @OrdemAntiga + ' | Valor Novo: ' + @OrdemNova + ' | Motivo: ' + @Motivo,
                    GETDATE());

            INSERT INTO Integracao_Clientes.dbo.BR_BackOfficeLog (CotacaoID, Motivo, DsAcao, DataHora, UsuarioID)
            VALUES (@CotacaoID,
                    'Motivo: ' + @Motivo + ' | Valor Antigo: ' + @OrdemAntiga + ' | Valor Novo: ' + @OrdemNova,
                    'Alterou Ordem de Compra do Pedido', GETDATE(), @UsuarioID);";

        await ExecuteAsync(sql, cancellationToken, cmd =>
        {
            cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@OrdemAntiga", (object?)ordemAntiga ?? string.Empty);
            cmd.Parameters.AddWithValue("@OrdemNova", (object?)ordemNova ?? string.Empty);
            cmd.Parameters.AddWithValue("@Motivo", (object?)motivo ?? string.Empty);
        });
    }

    // ----------------------------------------------------------------------
    // CANAL DE VENDA
    // ----------------------------------------------------------------------

    public async Task AlterarCanalVendaAsync(int cotacaoId, int usuarioId, string nmCanalAntigo, int canalVendaIdNovo, string motivo, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            DECLARE @NmCanalNovo NVARCHAR(100) = (SELECT NmCanalVenda FROM BrSupply.dbo.BR_CanalVenda (NOLOCK) WHERE CanalVendaID = @CanalVendaIDNovo);

            UPDATE BrSupply.dbo.BR_Cotacao
               SET CanalVendaID = @CanalVendaIDNovo
             WHERE CotacaoID = @CotacaoID;

            INSERT INTO BrSupply.dbo.BR_CotLog (CotacaoID, UsuarioID, TipoOperacao, Modificacao, DtOperacao)
            VALUES (@CotacaoID, @UsuarioID, 'R',
                    'Alterou o Canal de Venda do Pedido | Valor Antigo: ' + @NmCanalAntigo + ' | Valor Novo: ' + ISNULL(@NmCanalNovo,'') + ' | Motivo: ' + @Motivo,
                    GETDATE());

            INSERT INTO Integracao_Clientes.dbo.BR_BackOfficeLog (CotacaoID, Motivo, DsAcao, DataHora, UsuarioID)
            VALUES (@CotacaoID,
                    'Motivo: ' + @Motivo + ' | Valor Antigo: ' + @NmCanalAntigo + ' | Valor Novo: ' + ISNULL(@NmCanalNovo,''),
                    'Alterou o Canal de Venda do Pedido', GETDATE(), @UsuarioID);";

        await ExecuteAsync(sql, cancellationToken, cmd =>
        {
            cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@CanalVendaIDNovo", canalVendaIdNovo);
            cmd.Parameters.AddWithValue("@NmCanalAntigo", (object?)nmCanalAntigo ?? string.Empty);
            cmd.Parameters.AddWithValue("@Motivo", (object?)motivo ?? string.Empty);
        });
    }

    // ----------------------------------------------------------------------
    // CATEGORIA
    // ----------------------------------------------------------------------

    public async Task AlterarCategoriaAsync(int cotacaoId, int usuarioId, string nmCategoriaAntiga, int categoriaIdNova, string motivo, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            DECLARE @NmCategoriaNova NVARCHAR(100) = (SELECT NmCategoria FROM BrSupply.dbo.BR_ClienteCategoriaPedido (NOLOCK) WHERE ClienteCategoriaPedidoID = @CategoriaIDNova);

            UPDATE BrSupply.dbo.BR_Cotacao
               SET ClienteCategoriaPedidoID = @CategoriaIDNova
             WHERE CotacaoID = @CotacaoID;

            INSERT INTO BrSupply.dbo.BR_CotLog (CotacaoID, UsuarioID, TipoOperacao, Modificacao, DtOperacao)
            VALUES (@CotacaoID, @UsuarioID, 'R',
                    'Alterou a Categoria do Pedido. Valor Antigo: ' + @NmCategoriaAntiga + ' | Valor Novo: ' + ISNULL(@NmCategoriaNova,'') + ' | Motivo: ' + @Motivo,
                    GETDATE());

            INSERT INTO Integracao_Clientes.dbo.BR_BackOfficeLog (CotacaoID, Motivo, DsAcao, DataHora, UsuarioID)
            VALUES (@CotacaoID,
                    'Motivo: ' + @Motivo + ' | Valor Antigo: ' + @NmCategoriaAntiga + ' | Valor Novo: ' + ISNULL(@NmCategoriaNova,''),
                    'Alterou a Categoria do Pedido', GETDATE(), @UsuarioID);";

        await ExecuteAsync(sql, cancellationToken, cmd =>
        {
            cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@CategoriaIDNova", categoriaIdNova);
            cmd.Parameters.AddWithValue("@NmCategoriaAntiga", (object?)nmCategoriaAntiga ?? string.Empty);
            cmd.Parameters.AddWithValue("@Motivo", (object?)motivo ?? string.Empty);
        });
    }

    // ----------------------------------------------------------------------
    // CONDIÇÃO DE PAGAMENTO
    // ----------------------------------------------------------------------

    public async Task AlterarCondPagtoAsync(int cotacaoId, int usuarioId, string nmCondPagtoAntiga, int condPagtoIdNova, string motivo, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            DECLARE @NmCondPagtoNova NVARCHAR(100) = (SELECT NmCondPagto FROM BrSupply.dbo.BR_CondPagto (NOLOCK) WHERE CondPagtoID = @CondPagtoIDNova);

            UPDATE BrSupply.dbo.BR_Cotacao
               SET CondPagtoID = @CondPagtoIDNova
             WHERE CotacaoID = @CotacaoID;

            INSERT INTO BrSupply.dbo.BR_CotLog (CotacaoID, UsuarioID, TipoOperacao, Modificacao, DtOperacao)
            VALUES (@CotacaoID, @UsuarioID, 'R',
                    'Alterou a Condição de Pagamento do Pedido | Valor Antigo: ' + @NmCondPagtoAntiga + ' | Valor Novo: ' + ISNULL(@NmCondPagtoNova,'') + ' | Motivo: ' + @Motivo,
                    GETDATE());

            INSERT INTO Integracao_Clientes.dbo.BR_BackOfficeLog (CotacaoID, Motivo, DsAcao, DataHora, UsuarioID)
            VALUES (@CotacaoID,
                    'Motivo: ' + @Motivo + ' | Valor Antigo: ' + @NmCondPagtoAntiga + ' | Valor Novo: ' + ISNULL(@NmCondPagtoNova,''),
                    'Alterou a Condição de Pagamento do Pedido', GETDATE(), @UsuarioID);";

        await ExecuteAsync(sql, cancellationToken, cmd =>
        {
            cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@CondPagtoIDNova", condPagtoIdNova);
            cmd.Parameters.AddWithValue("@NmCondPagtoAntiga", (object?)nmCondPagtoAntiga ?? string.Empty);
            cmd.Parameters.AddWithValue("@Motivo", (object?)motivo ?? string.Empty);
        });
    }

    // ----------------------------------------------------------------------
    // COBRAR FRETE
    // ----------------------------------------------------------------------

    public async Task CobrarFreteAsync(int cotacaoId, int usuarioId, decimal vlrFrete, int flagFreteServico, CancellationToken cancellationToken = default)
    {
        // Replica a regra do PHP: se FlagFreteServico=1, atualiza VlrFreteServico; senão VlrFrete.
        var colunaUpdate = flagFreteServico == 1 ? "VlrFreteServico" : "VlrFrete";
        var mensagemLog = flagFreteServico == 1
            ? "Atualizou manualmente o valor de frete serviço do pedido"
            : "Atualizou manualmente o valor de frete do pedido";

        var sql = $@"
            UPDATE BrSupply.dbo.BR_Cotacao
               SET {colunaUpdate} = @VlrFrete
             WHERE CotacaoID = @CotacaoID;

            INSERT INTO BrSupply.dbo.BR_CotLog (CotacaoID, UsuarioID, Modificacao, TipoOperacao, DtOperacao)
            VALUES (@CotacaoID, @UsuarioID,
                    @MensagemLog + ' | R$ ' + FORMAT(@VlrFrete, 'N2', 'pt-br'),
                    'R', GETDATE());";

        await ExecuteAsync(sql, cancellationToken, cmd =>
        {
            cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@VlrFrete", vlrFrete);
            cmd.Parameters.AddWithValue("@MensagemLog", mensagemLog);
        });
    }

    // ----------------------------------------------------------------------
    // LIBERAR MARKETPLACE
    // ----------------------------------------------------------------------

    public async Task LiberarMarketplaceAsync(int cotacaoId, int usuarioId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            DECLARE @NmUsuario NVARCHAR(100) = (SELECT NmUsuario FROM BrSupply.dbo.BR_Usuario (NOLOCK) WHERE UsuarioID = @UsuarioID);

            UPDATE BrSupply.dbo.BR_Cotacao
               SET StatusCotacao = 5
             WHERE CotacaoID = @CotacaoID
               AND StatusCotacao = 3;

            INSERT INTO BrSupply.dbo.BR_CotAprov (CotacaoID, UsuarioID, DtInclusao, StatusCotacao)
            VALUES (@CotacaoID, @UsuarioID, GETDATE(), 5);

            INSERT INTO BrSupply.dbo.BR_CotLog (CotacaoID, UsuarioID, TipoOperacao, Modificacao, DtOperacao)
            VALUES (@CotacaoID, @UsuarioID, 'A',
                    'Pedido Marketplace liberado pelo atendimento comercial',
                    GETDATE());

            INSERT INTO Integracao_Clientes.dbo.BR_BackOfficeLog (CotacaoID, Motivo, DsAcao, DataHora, UsuarioID)
            VALUES (@CotacaoID,
                    'Pedido Marketplace liberado pelo atendimento comercial',
                    'Liberou Pedido Marketplace', GETDATE(), @UsuarioID);

            EXEC BrSupply.dbo.BR_sp_CadBR_Tracking
                 @Tipo_Operacao  = 'I',
                 @TrackingEventoID = 4,
                 @CotacaoID      = @CotacaoID,
                 @VersaoCotacao  = 1,
                 @NrPedCli       = '',
                 @Usuario        = @NmUsuario,
                 @DtEvento       = '',
                 @Detalhes       = 'Pedido Marketplace liberado pelo atendimento comercial';";

        await ExecuteAsync(sql, cancellationToken, cmd =>
        {
            cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
        });
    }

    // ----------------------------------------------------------------------
    // CANCELAR PEDIDO / MARKETPLACE
    // ----------------------------------------------------------------------

    public Task CancelarPedidoAsync(int cotacaoId, int usuarioId, string motivo, CancellationToken cancellationToken = default)
        => CancelarGenericoAsync(cotacaoId, usuarioId, motivo,
            descricaoAcao: "Pedido cancelado pelo atendimento comercial",
            cancellationToken);

    public Task CancelarMarketplaceAsync(int cotacaoId, int usuarioId, string motivo, CancellationToken cancellationToken = default)
        => CancelarGenericoAsync(cotacaoId, usuarioId, motivo,
            descricaoAcao: "Pedido Marketplace cancelado pelo atendimento comercial",
            cancellationToken);

    private async Task CancelarGenericoAsync(int cotacaoId, int usuarioId, string motivo, string descricaoAcao, CancellationToken cancellationToken)
    {
        const string sql = @"
            DECLARE @NmUsuario NVARCHAR(100) = (SELECT NmUsuario FROM BrSupply.dbo.BR_Usuario (NOLOCK) WHERE UsuarioID = @UsuarioID);

            UPDATE BrSupply.dbo.BR_Cotacao
               SET StatusCotacao = 19
             WHERE CotacaoID = @CotacaoID
               AND StatusCotacao = 3;

            INSERT INTO BrSupply.dbo.BR_CotAprov (CotacaoID, UsuarioID, DtInclusao, StatusCotacao)
            VALUES (@CotacaoID, @UsuarioID, GETDATE(), 19);

            INSERT INTO BrSupply.dbo.BR_CotLog (CotacaoID, UsuarioID, TipoOperacao, Modificacao, DtOperacao)
            VALUES (@CotacaoID, @UsuarioID, 'A',
                    @DescAcao + ' | Motivo: ' + @Motivo,
                    GETDATE());

            INSERT INTO Integracao_Clientes.dbo.BR_BackOfficeLog (CotacaoID, Motivo, DsAcao, DataHora, UsuarioID)
            VALUES (@CotacaoID,
                    'Motivo do cancelamento: ' + @Motivo,
                    @DescAcao, GETDATE(), @UsuarioID);

            EXEC BrSupply.dbo.BR_sp_CadBR_Tracking
                 @Tipo_Operacao  = 'I',
                 @TrackingEventoID = 5,
                 @CotacaoID      = @CotacaoID,
                 @VersaoCotacao  = 1,
                 @NrPedCli       = '',
                 @Usuario        = @NmUsuario,
                 @DtEvento       = '',
                 @Detalhes       = 'Pedido cancelado pelo atendimento comercial';";

        await ExecuteAsync(sql, cancellationToken, cmd =>
        {
            cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@Motivo", (object?)motivo ?? string.Empty);
            cmd.Parameters.AddWithValue("@DescAcao", descricaoAcao);
        });
    }

    // ----------------------------------------------------------------------
    // DESBLOQUEAR ALOCAÇÕES
    // ----------------------------------------------------------------------

    public async Task DesbloquearAlocacoesAsync(int cotacaoId, int usuarioId, string motivo, CancellationToken cancellationToken = default)
    {
        // NOTA: o PHP original tinha um erro de sintaxe (vírgula sobrando após FlagAlocaPedido=0).
        // A intenção correta é: itens com ItemID>0, FlagAlocaPedido=0 e FlagAtendimentoManager=0 → setar FlagAtendimentoManager=1.
        const string sql = @"
            UPDATE BrSupply.dbo.BR_CotacaoItem
               SET FlagAlocaPedido = 0,
                   FlagAtendimentoManager = 1
             WHERE CotacaoID = @CotacaoID
               AND ISNULL(ItemID, 0) > 0
               AND ISNULL(FlagAlocaPedido, 0) = 0
               AND ISNULL(FlagAtendimentoManager, 0) = 0;

            INSERT INTO BrSupply.dbo.BR_CotLog (CotacaoID, UsuarioID, TipoOperacao, Modificacao, DtOperacao)
            VALUES (@CotacaoID, @UsuarioID, 'R',
                    'Desbloqueada a alocação de estoques do pedido',
                    GETDATE());

            INSERT INTO Integracao_Clientes.dbo.BR_BackOfficeLog (CotacaoID, Motivo, DsAcao, DataHora, UsuarioID)
            VALUES (@CotacaoID,
                    'Motivo do desbloqueio: ' + @Motivo,
                    'Desbloqueada a alocação de estoques do pedido',
                    GETDATE(), @UsuarioID);";

        await ExecuteAsync(sql, cancellationToken, cmd =>
        {
            cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@Motivo", (object?)motivo ?? string.Empty);
        });
    }

    // ----------------------------------------------------------------------
    // GERAR PEDIDO COM RUPTURAS
    // ----------------------------------------------------------------------

    public async Task<int?> GerarPedidoRupturasAsync(int cotacaoId, int clienteId, int clienteUsuarioId, int usuarioId, string motivo, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Executa a SP de geração
        await using (var cmd = new SqlCommand("BrSupply.dbo.BR_sp_GerarPedidoComRupturas", connection) { CommandType = CommandType.StoredProcedure, CommandTimeout = 300 })
        {
            cmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
            cmd.Parameters.AddWithValue("@ClienteID", clienteId);
            cmd.Parameters.AddWithValue("@ClienteUsuarioID", clienteUsuarioId);
            cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
            cmd.Parameters.AddWithValue("@Motivo", (object?)motivo ?? string.Empty);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        // Aguarda estabilização (5s como no PHP)
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

        // Lê o log para extrair PedidoOrigem:X|PedidoDestino:Y
        const string selectLog = @"
            SELECT TOP 1 ISNULL(L.Modificacao, '') AS Modificacao
              FROM BrSupply.dbo.BR_CotLog L (NOLOCK)
             WHERE L.CotacaoID = @CotacaoID
               AND L.Modificacao LIKE '%PedidoOrigem:%'
               AND L.Modificacao LIKE '%PedidoDestino:%'
             ORDER BY L.DtOperacao DESC;";

        await using var selectCmd = new SqlCommand(selectLog, connection);
        selectCmd.Parameters.AddWithValue("@CotacaoID", cotacaoId);
        var result = await selectCmd.ExecuteScalarAsync(cancellationToken);
        if (result is null || result is DBNull) return null;

        var modificacao = result.ToString() ?? string.Empty;
        var partes = modificacao.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var parte in partes)
        {
            if (parte.StartsWith("PedidoDestino:", StringComparison.OrdinalIgnoreCase))
            {
                var valor = parte["PedidoDestino:".Length..].Trim();
                if (int.TryParse(valor, out var novoId) && novoId > 0)
                    return novoId;
            }
        }
        return null;
    }

    // ----------------------------------------------------------------------
    // INFRA
    // ----------------------------------------------------------------------

    private async Task ExecuteAsync(string sql, CancellationToken cancellationToken, Action<SqlCommand> applyParams)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await using var cmd = new SqlCommand(sql, connection, transaction) { CommandTimeout = 120 };
            applyParams(cmd);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
