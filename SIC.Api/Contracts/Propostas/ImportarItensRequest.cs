namespace SIC.Api.Contracts.Propostas;

public sealed class ImportarItensRequest
{
    public int PropostaID { get; set; }
    public List<ImportarItemLine> Itens { get; set; } = [];
}

public sealed class ImportarItemLine
{
    public string CodCliente { get; set; } = string.Empty;
    public string DescricaoBreve { get; set; } = string.Empty;
    public string DescricaoDetalhada { get; set; } = string.Empty;
    public string Familia { get; set; } = string.Empty;
    public string MarcaFornecedor { get; set; } = string.Empty;
    public string UnidadeMedida { get; set; } = string.Empty;
    public int QtdAnual { get; set; }
    public decimal Target { get; set; }
}
