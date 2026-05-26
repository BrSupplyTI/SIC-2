using System.Net.Http.Json;
using SIC.Web.Models.Auth;

namespace SIC.Web.Services.PrePedidosPDF;

/// <summary>
/// Operações de escrita (commands) do pré-pedido.
/// Métodos: AtualizarCnpjAsync, AtualizarEnderecoAsync, AtualizarLocalEntregaAsync,
///          AtualizarQuantidadeAsync, ExcluirItemAsync, TrocarItemAsync,
///          AdicionarItemAsync, CancelarAsync, ReprocessarAsync
/// </summary>
public sealed partial class PrePedidoPDFApiClient
{
    public Task<OperationResultVm> AtualizarCnpjAsync(
        int id,
        string cnpj,
        CancellationToken cancellationToken = default)
        => PutAsync($"api/pre-pedidos-pdf/{id}/cnpj", new { Cnpj = cnpj }, cancellationToken);

    public Task<OperationResultVm> AtualizarEnderecoAsync(
        int id,
        int clienteEnderecoId,
        string logradouro,
        CancellationToken cancellationToken = default)
        => PutAsync(
            $"api/pre-pedidos-pdf/{id}/endereco",
            new { ClienteEnderecoID = clienteEnderecoId, Logradouro = logradouro },
            cancellationToken);

    public Task<OperationResultVm> AtualizarLocalEntregaAsync(
        int id,
        int clienteLocalEntregaId,
        string nomeLocalEntrega,
        CancellationToken cancellationToken = default)
        => PutAsync(
            $"api/pre-pedidos-pdf/{id}/local-entrega",
            new { ClienteLocalEntregaID = clienteLocalEntregaId, NmLocalEntrega = nomeLocalEntrega },
            cancellationToken);

    public Task<OperationResultVm> AtualizarQuantidadeAsync(
        int id,
        int itemId,
        int quantidade,
        string descricao,
        CancellationToken cancellationToken = default)
        => PutAsync(
            $"api/pre-pedidos-pdf/{id}/itens/{itemId}/quantidade",
            new { Quantidade = quantidade, Descricao = descricao },
            cancellationToken);

    public Task<OperationResultVm> AtualizarVlrUnitAsync(
        int id,
        int itemId,
        decimal vlrUnit,
        string descricao,
        CancellationToken cancellationToken = default)
        => PutAsync(
            $"api/pre-pedidos-pdf/{id}/itens/{itemId}/vlr-unit",
            new { VlrUnit = vlrUnit, Descricao = descricao },
            cancellationToken);

    public Task<OperationResultVm> AtualizarObsAsync(
        int id,
        string obsNota,
        string obsComprador,
        CancellationToken cancellationToken = default)
        => PutAsync(
            $"api/pre-pedidos-pdf/{id}/obs",
            new { ObsNota = obsNota, ObsComprador = obsComprador },
            cancellationToken);

    public Task<OperationResultVm> ExcluirItemAsync(
        int id,
        int itemId,
        string descricao,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"api/pre-pedidos-pdf/{id}/itens/{itemId}/excluir",
            new { Descricao = descricao },
            cancellationToken);

    public Task<OperationResultVm> TrocarItemAsync(
        int id,
        int itemId,
        string cdItem,
        int novoItemId,
        string nomeItem,
        decimal vlrTabelaPreco,
        string cdItemAntigo,
        string descricaoAntiga,
        string valorAntigo,
        string motivoTrocaItem,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"api/pre-pedidos-pdf/{id}/itens/{itemId}/trocar",
            new
            {
                CdItem = cdItem,
                ItemID = novoItemId,
                NmItem = nomeItem,
                VlrTabelaPreco = vlrTabelaPreco,
                CdItemAntigo = cdItemAntigo,
                DescricaoAntiga = descricaoAntiga,
                ValorAntigo = valorAntigo,
                MotivoTrocaItem = motivoTrocaItem
            },
            cancellationToken);

    public Task<OperationResultVm> CancelarAsync(
        int id,
        CancellationToken cancellationToken = default)
        => PostAsync($"api/pre-pedidos-pdf/{id}/cancelar", new { }, cancellationToken);

    public Task<OperationResultVm> ReprocessarAsync(
        int id,
        CancellationToken cancellationToken = default)
        => PostAsync($"api/pre-pedidos-pdf/{id}/reprocessar", new { }, cancellationToken);

    public Task<OperationResultVm> AceitarPedidoAsync(
        int id,
        CancellationToken cancellationToken = default)
        => PostAsync($"api/pre-pedidos-pdf/{id}/aceitar", new { }, cancellationToken);

    public Task<OperationResultVm> AdicionarItemAsync(
        int id,
        string codItemBR,
        string descrItemBR,
        int quantidade,
        decimal precoTbl,
        string itemDePara,
        int itemId,
        string ordemCompra,
        CancellationToken cancellationToken = default)
        => PostAsync(
            $"api/pre-pedidos-pdf/{id}/itens/adicionar",
            new
            {
                CodItemBR = codItemBR,
                DescrItemBR = descrItemBR,
                Quantidade = quantidade,
                PrecoTbl = precoTbl,
                ItemDePara = itemDePara,
                ItemID = itemId,
                OrdemCompra = ordemCompra
            },
            cancellationToken);

    private async Task<OperationResultVm> PutAsync<TRequest>(
        string url,
        TRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(url, request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<OperationResultVm>(cancellationToken: cancellationToken);

            return result ?? new OperationResultVm
            {
                Success = false,
                Message = "Resposta inválida da API."
            };
        }
        catch
        {
            return new OperationResultVm
            {
                Success = false,
                Message = "Não foi possível conectar na API do SIC."
            };
        }
    }

    private async Task<OperationResultVm> PostAsync<TRequest>(
        string url,
        TRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(url, request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<OperationResultVm>(cancellationToken: cancellationToken);

            return result ?? new OperationResultVm
            {
                Success = false,
                Message = "Resposta inválida da API."
            };
        }
        catch
        {
            return new OperationResultVm
            {
                Success = false,
                Message = "Não foi possível conectar na API do SIC."
            };
        }
    }
}
