namespace SIC.Domain.Entities.PrePedidosPDF;

/// <summary>
/// Entidade de endereço do cliente vinculado ao pré-pedido (GetEnderecos).
/// </summary>
public sealed class PrePedidoPDFEndereco
{
    public int ClienteEnderecoID { get; set; }
    public string Logradouro { get; set; } = string.Empty;
}
