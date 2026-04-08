namespace SIC.Api.Contracts.Clientes;

public sealed class ClientSearchFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 30;
    public string? ContemTexto { get; set; }
    public string? ComecaComTexto { get; set; }
    public int FlagAtivo { get; set; } = 1;
    public int EstabelecimentoID { get; set; } = 0;
    public int FlagClienteMae { get; set; } = 0;
    public int CarteiraID { get; set; } = 0;
    public int QtDiasUltimoPedido { get; set; } = 0;
    public string? OrderBy { get; set; } = "Nome (A-Z)";
    public int UsuarioID { get; set; } = 0;
}
