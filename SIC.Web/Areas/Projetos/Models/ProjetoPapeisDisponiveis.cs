namespace SIC.Web.Areas.Projetos.Models;

/// <summary>
/// Catálogo de papéis pré-definidos que um participante pode assumir em um projeto.
/// Usado como fonte da lista suspensa em modais de adicionar/editar participante.
/// Mantido em memória (estático) para simplicidade; pode evoluir para tabela no banco no futuro.
/// </summary>
public static class ProjetoPapeisDisponiveis
{
    /// <summary>
    /// Lista ordenada de papéis disponíveis. Cada item contém nome, ícone (Font Awesome) e descrição curta.
    /// </summary>
    public static readonly IReadOnlyList<ProjetoPapelItem> Todos =
[
    new("Responsável",     "fa-solid fa-user-tie",     "Coordena o projeto e responde pela entrega"),
    new("Negócio",         "fa-solid fa-bullseye",     "Define requisitos e prioridades"),
    new("Desenvolvedor",   "fa-solid fa-code",         "Desenvolve as funcionalidades"),
    new("Qualidade",       "fa-solid fa-vial-circle-check", "Garante testes e qualidade"),
    new("Infraestrutura",  "fa-solid fa-server",       "Ambiente, deploy e integrações"),
    new("Dados",           "fa-solid fa-database",     "Banco de dados e modelagem"),
    new("Suporte",         "fa-solid fa-headset",      "Atendimento e incidentes"),
    new("Interessado",     "fa-solid fa-handshake",    "Acompanha e influencia decisões"),
    new("Observador",      "fa-solid fa-eye",          "Apenas acompanha")
];
}

/// <summary>
/// Item do catálogo de papéis do projeto.
/// </summary>
/// <param name="Nome">Nome exibido e salvo no campo NmPapel.</param>
/// <param name="Icone">Classe Font Awesome do ícone.</param>
/// <param name="Descricao">Descrição curta exibida no dropdown.</param>
public sealed record ProjetoPapelItem(string Nome, string Icone, string Descricao);
