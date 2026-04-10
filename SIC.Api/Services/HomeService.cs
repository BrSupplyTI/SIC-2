using SIC.Api.Contracts.Home;
using SIC.Domain.Abstractions;
using SIC.Domain.Entities;

namespace SIC.Api.Services;

public sealed class HomeService(IHomeRepository repository) : IHomeService
{
    public async Task<IReadOnlyList<ShortcutDto>> GetUserShortcutsAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetUserShortcutsAsync(usuarioId, cancellationToken);

        return items.Select(i => new ShortcutDto
        {
            AtalhoID = i.AtalhoID,
            Nome = i.Nome,
            Url = i.Url,
            FlagExterna = i.FlagExterna,
            Icone = i.Icone
        }).ToList();
    }

    public async Task<IReadOnlyList<ShortcutDto>> GetAllShortcutsAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.GetAllShortcutsAsync(cancellationToken);

        return items.Select(i => new ShortcutDto
        {
            AtalhoID = i.AtalhoID,
            Nome = i.Nome,
            Url = i.Url,
            FlagExterna = i.FlagExterna,
            Icone = i.Icone
        }).ToList();
    }

    public async Task AddUserShortcutAsync(int usuarioId, int atalhoId, CancellationToken cancellationToken = default)
        => await repository.AddUserShortcutAsync(usuarioId, atalhoId, cancellationToken);

    public async Task RemoveUserShortcutAsync(int usuarioId, int atalhoId, CancellationToken cancellationToken = default)
        => await repository.RemoveUserShortcutAsync(usuarioId, atalhoId, cancellationToken);

    public async Task<IReadOnlyList<CurrencyQuoteDto>> GetCurrencyQuotesAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository.GetCurrencyQuotesAsync(cancellationToken);

        return items.Select(i => new CurrencyQuoteDto
        {
            Moeda = i.Moeda,
            Nome = i.Nome,
            Valor = i.Valor,
            Variacao = i.Variacao,
            DataAtualizacao = i.DataAtualizacao
        }).ToList();
    }

    public async Task<WeatherInfoDto?> GetWeatherInfoAsync(int estabelecimentoId, CancellationToken cancellationToken = default)
    {
        var item = await repository.GetWeatherInfoAsync(estabelecimentoId, cancellationToken);
        if (item is null) return null;

        return new WeatherInfoDto
        {
            EstabelecimentoID = item.EstabelecimentoID,
            Cidade = item.Cidade,
            UF = item.UF,
            Temperatura = item.Temperatura,
            Sensacao = item.Sensacao,
            Umidade = item.Umidade,
            VelocidadeVento = item.VelocidadeVento,
            Descricao = item.Descricao,
            DtUltimaAtualizacao = item.DtUltimaAtualizacao
        };
    }
}
