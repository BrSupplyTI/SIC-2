using SIC.Api.Models.Auth;
using SIC.Domain.Abstractions.PrePedidosPDF;

namespace SIC.Api.Services.PrePedidosPDF;

/// <summary>
/// Implementação das operações de escrita do pré-pedido.
/// </summary>
public sealed class PrePedidoPDFCommandService(
    IPrePedidoPDFCommandRepository repository,
    IPrePedidoPDFQueryRepository queryRepository,
    IPrePedidoPDFIntegrationService integrationService) : IPrePedidoPDFCommandService
{
    public Task<OperationResult> AtualizarEnderecoAsync(
        int prePedidoId,
        int clienteEnderecoId,
        string logradouro,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            () => repository.AtualizarEnderecoAsync(prePedidoId, clienteEnderecoId, logradouro, cancellationToken),
            "Endereço atualizado com sucesso.",
            "Erro ao atualizar endereço.");

    public Task<OperationResult> AtualizarLocalEntregaAsync(
        int prePedidoId,
        int clienteLocalEntregaId,
        string nomeLocalEntrega,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            () => repository.AtualizarLocalEntregaAsync(prePedidoId, clienteLocalEntregaId, nomeLocalEntrega, cancellationToken),
            "Local de entrega atualizado com sucesso.",
            "Erro ao atualizar local de entrega.");

    public Task<OperationResult> AtualizarCnpjAsync(
        int prePedidoId,
        string cnpj,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            () => repository.AtualizarCnpjAsync(prePedidoId, cnpj, cancellationToken),
            "CNPJ atualizado com sucesso.",
            "Erro ao atualizar CNPJ.");

    public async Task<OperationResult> AtualizarQuantidadeAsync(
        int prePedidoId,
        int prePedidoItemId,
        int quantidade,
        string descricao,
        CancellationToken cancellationToken = default)
    {
        if (quantidade < 1)
        {
            return new OperationResult
            {
                Success = false,
                Message = "A quantidade deve ser maior que zero."
            };
        }

        var prePedido = await queryRepository.GetByIdAsync(prePedidoId, cancellationToken);

        if (prePedido is null)
        {
            return new OperationResult
            {
                Success = false,
                Message = "Pré-pedido não encontrado."
            };
        }

        if (prePedido.StatusPrePedidoPDFID != 1)
        {
            return new OperationResult
            {
                Success = false,
                Message = "A quantidade só pode ser alterada quando o status estiver Aguardando."
            };
        }

        return await ExecuteAsync(
            () => repository.UpdateQuantidadeAsync(prePedidoItemId, prePedidoId, quantidade, descricao, cancellationToken),
            "Quantidade atualizada com sucesso.",
            "Erro ao atualizar quantidade.");
    }

    public async Task<OperationResult> AtualizarVlrUnitAsync(
        int prePedidoId,
        int prePedidoItemId,
        decimal vlrUnit,
        string descricao,
        CancellationToken cancellationToken = default)
    {
        if (vlrUnit < 0)
        {
            return new OperationResult
            {
                Success = false,
                Message = "O valor unitário não pode ser negativo."
            };
        }

        var prePedido = await queryRepository.GetByIdAsync(prePedidoId, cancellationToken);

        if (prePedido is null)
        {
            return new OperationResult
            {
                Success = false,
                Message = "Pré-pedido não encontrado."
            };
        }

        if (prePedido.StatusPrePedidoPDFID != 1)
        {
            return new OperationResult
            {
                Success = false,
                Message = "O valor unitário só pode ser alterado quando o status estiver Aguardando."
            };
        }

        return await ExecuteAsync(
            () => repository.UpdateVlrUnitAsync(prePedidoItemId, prePedidoId, vlrUnit, descricao, cancellationToken),
            "Valor unitário atualizado com sucesso.",
            "Erro ao atualizar valor unitário.");
    }

    public async Task<OperationResult> AtualizarObsAsync(
        int prePedidoId,
        string obsNota,
        string obsComprador,
        CancellationToken cancellationToken = default)
    {
        var prePedido = await queryRepository.GetByIdAsync(prePedidoId, cancellationToken);

        if (prePedido is null)
            return new OperationResult { Success = false, Message = "Pré-pedido não encontrado." };

        if (prePedido.StatusPrePedidoPDFID == 4 || prePedido.StatusPrePedidoPDFID == 5)
            return new OperationResult { Success = false, Message = "As observações não podem ser alteradas quando o pré-pedido está aceito ou recusado." };

        return await ExecuteAsync(
            () => repository.UpdateObsAsync(prePedidoId, obsNota, obsComprador, cancellationToken),
            "Observações atualizadas com sucesso.",
            "Erro ao atualizar observações.");
    }

    public Task<OperationResult> CancelarAsync(
        int prePedidoId,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            () => repository.CancelarAsync(prePedidoId, cancellationToken),
            "Pré-pedido cancelado com sucesso.",
            "Erro ao cancelar pré-pedido.");

    public Task<OperationResult> ExcluirItemAsync(
        int prePedidoId,
        int prePedidoItemId,
        string descricao,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            () => repository.ExcluirItemAsync(prePedidoItemId, prePedidoId, descricao, cancellationToken),
            "Item excluído com sucesso.",
            "Erro ao excluir item.");

    public Task<OperationResult> TrocarItemAsync(
        int prePedidoId,
        int prePedidoItemId,
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
            () => repository.GravarTrocaItemAsync(
                prePedidoItemId,
                prePedidoId,
                cdItem,
                itemId,
                nomeItem,
                vlrTabelaPreco,
                cdItemAntigo,
                descricaoAntiga,
                valorAntigo,
                motivoTrocaItem,
                cancellationToken),
            "Item trocado com sucesso.",
            "Erro ao trocar item.");

    public Task<OperationResult> AdicionarItemAsync(
        int prePedidoId,
        string codItemBR,
        string descrItemBR,
        int quantidade,
        decimal precoTbl,
        string itemDePara,
        int itemId,
        string ordemCompra,
        CancellationToken cancellationToken = default)
        => ExecuteAsync(
            () => repository.AdicionarItemAsync(
                prePedidoId,
                codItemBR,
                descrItemBR,
                quantidade,
                precoTbl,
                itemDePara,
                itemId,
                ordemCompra,
                cancellationToken),
            "Item adicionado com sucesso.",
            "Erro ao adicionar item.");

    public async Task<OperationResult> ReprocessarAsync(
        int prePedidoId,
        CancellationToken cancellationToken = default)
    {
        var prePedido = await queryRepository.GetByIdAsync(prePedidoId, cancellationToken);

        if (prePedido is null)
        {
            return new OperationResult
            {
                Success = false,
                Message = "Pré-pedido não encontrado."
            };
        }

        var jsonPedido = await integrationService.GetConteudoArquivoPedidoAsync(
            prePedido.CdExtCliente,
            prePedido.OrdemCompra,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(jsonPedido))
        {
            return new OperationResult
            {
                Success = false,
                Message = "Conteúdo do arquivo não encontrado."
            };
        }

        var preparado = await repository.SetProcessadorPraZeroAsync(prePedidoId, cancellationToken);

        if (!preparado)
        {
            return new OperationResult
            {
                Success = false,
                Message = "Não foi possível preparar o pré-pedido para reprocessamento."
            };
        }

        var (success, message) = await integrationService.ReprocessarPedidoAsync(jsonPedido, cancellationToken);

        if (success)
        {
            await repository.InserirLogReprocessamentoAsync(
                prePedidoId,
                $"Pré-pedido reprocessado. Ordem de compra: {prePedido.OrdemCompra}",
                cancellationToken);

            if (prePedido.StatusPrePedidoPDFID == 5)
            {
                await repository.AtualizarStatusAguardandoAsync(prePedidoId, cancellationToken);
            }
        }

        return new OperationResult
        {
            Success = success,
            Message = string.IsNullOrWhiteSpace(message)
                ? success
                    ? "Pré-pedido reprocessado com sucesso."
                    : "Erro ao reprocessar pré-pedido."
                : message
        };
    }

    public async Task<OperationResult> AceitarPedidoAsync(
        int prePedidoId,
        CancellationToken cancellationToken = default)
    {
        // ── ValidarParaAceite (réplica fiel do PHP) ──
        var prePedido = await queryRepository.GetByIdAsync(prePedidoId, cancellationToken);

        if (prePedido is null)
            return new OperationResult { Success = false, Message = "Pré-pedido não encontrado." };

        if (string.IsNullOrWhiteSpace(prePedido.CNPJ))
            return new OperationResult { Success = false, Message = "CNPJ nulo." };

        if (prePedido.ClienteEnderecoID == 0)
            return new OperationResult { Success = false, Message = "Endereço nulo." };

        if (prePedido.ClienteLocalEntregaID == 0)
            return new OperationResult { Success = false, Message = "Local de entrega nulo." };

        if (prePedido.CotacaoID > 0)
            return new OperationResult { Success = false, Message = "Cotação já gerada." };

        // ── GerarPedido (BR_sp_InsertCotacao) ──
        var info = await queryRepository.GetInfoGerarPedidoAsync(prePedidoId, cancellationToken);

        if (info is null)
            return new OperationResult { Success = false, Message = "Não foi possível obter informações para gerar o pedido." };

        var cotacaoId = await repository.GerarPedidoAsync(
            info.EstabelecimentoID,
            info.ClienteID,
            info.ClienteEnderecoID,
            info.CNPJ,
            info.ClienteLocalEntregaID,
            info.ClienteUsuarioID,
            info.NaturezaOperacaoID,
            info.CondPagtoID,
            info.OrdemCompra,
            info.ClienteCategoriaPedidoID,
            cancellationToken);

        if (cotacaoId <= 0)
            return new OperationResult { Success = false, Message = "Erro ao gerar cotação." };

        // Atualizar CotacaoID + Status = 4 (Aceito)
        await repository.AtualizarCotacaoStatusAsync(prePedidoId, cotacaoId, cancellationToken);

        // ── GerarItensPedidoBrSupply ──
        var itens = await queryRepository.GetInfoItensGerarPedidoAsync(prePedidoId, cancellationToken);

        foreach (var item in itens)
        {
            await repository.GerarItemPedidoAsync(
                item.CotacaoID,
                item.Tipo,
                item.ItemID,
                item.QtItem,
                item.VlrUnit,
                item.CdItemCliente,
                item.OrdemCliente,
                item.SeqCliente,
                cancellationToken);
        }

        return new OperationResult { Success = true, Message = "Pedido aceito com sucesso." };
    }

    private static async Task<OperationResult> ExecuteAsync(
        Func<Task<bool>> action,
        string successMessage,
        string errorMessage)
    {
        var success = await action();

        return new OperationResult
        {
            Success = success,
            Message = success ? successMessage : errorMessage
        };
    }
}
