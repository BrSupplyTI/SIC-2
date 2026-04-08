namespace SIC.Domain.Entities;

public sealed class ClientSearchItem
{
    public int ClienteID { get; set; }
    public string CodigoSAP { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string CPFCNPJ { get; set; } = string.Empty;
    public string Situacao { get; set; } = string.Empty;
    public int EstabelecimentoID { get; set; }
    public string Estabelecimento { get; set; } = string.Empty;
    public string Carteira { get; set; } = string.Empty;
    public int QtEnderecos { get; set; }
    public int QtLocaisEntrega { get; set; }
    public int QtUsuarios { get; set; }
    public int TotalRegistros { get; set; }
}
