using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SIC.Domain.Abstractions.PrePedidosPDF;

namespace SIC.Infrastructure.Integrations.PrePedidosPDF;

/// <summary>
/// Implementação das integrações externas do pré-pedido.
/// </summary>
public sealed class PrePedidoPDFIntegrationService(HttpClient httpClient) : IPrePedidoPDFIntegrationService
{
    public async Task<string> GetConteudoArquivoPedidoAsync(
        string cdExtCliente,
        string ordemCompra,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cdExtCliente) || string.IsNullOrWhiteSpace(ordemCompra))
            return string.Empty;

        var url = $"https://punchout.brsupply.com.br/storage/processadorERP/{Uri.EscapeDataString(cdExtCliente)}/{Uri.EscapeDataString(ordemCompra)}.json";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return string.Empty;

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        try
        {
            using var json = JsonDocument.Parse(content);

            return JsonSerializer.Serialize(json.RootElement, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch
        {
            return content;
        }
    }

    public async Task<(bool Success, string Message)> ReprocessarPedidoAsync(
        string jsonPedido,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jsonPedido))
            return (false, "Conteúdo do arquivo não encontrado para reprocessamento.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brsupply.com.br/v1/IntegradorERP/PedidoNew")
        {
            Content = new StringContent(jsonPedido, Encoding.UTF8)
        };

        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            return (false, $"Falha ao reprocessar o pré-pedido. HTTP {(int)response.StatusCode}.");

        if (string.IsNullOrWhiteSpace(responseContent))
            return (true, "Pré-pedido enviado para reprocessamento com sucesso.");

        try
        {
            using var json = JsonDocument.Parse(responseContent);

            if (json.RootElement.TryGetProperty("success", out var successProperty)
                && successProperty.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                var success = successProperty.GetBoolean();
                var message = json.RootElement.TryGetProperty("message", out var messageProperty)
                    ? messageProperty.GetString() ?? string.Empty
                    : string.Empty;

                return (success, string.IsNullOrWhiteSpace(message)
                    ? success
                        ? "Pré-pedido enviado para reprocessamento com sucesso."
                        : "Falha ao reprocessar o pré-pedido."
                    : message);
            }
        }
        catch
        {
        }

        return (true, "Pré-pedido enviado para reprocessamento com sucesso.");
    }
}