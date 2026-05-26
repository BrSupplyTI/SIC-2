namespace SIC.Web.Areas.Projetos.Models;

public sealed class ProjetoListaViewModel
{
    public string? Texto { get; set; }
    public int ProjetoStatusID { get; set; }
    public string OrderBy { get; set; } = "Recentes";
    public bool ExcluirEncerrados { get; set; } = true;

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public IReadOnlyList<ProjetoItemVm> Itens { get; set; } = [];
    public IReadOnlyList<ProjetoStatusItemVm> StatusDisponiveis { get; set; } = [];

    public int UsuarioLogadoID { get; set; }
    public string NmUsuarioLogado { get; set; } = string.Empty;
    }
