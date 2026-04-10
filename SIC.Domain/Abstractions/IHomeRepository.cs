namespace SIC.Domain.Abstractions;

using SIC.Domain.Entities;

public interface IHomeRepository
{
    Task<IReadOnlyList<Shortcut>> GetUserShortcutsAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Shortcut>> GetAllShortcutsAsync(CancellationToken cancellationToken = default);
    Task AddUserShortcutAsync(int usuarioId, int atalhoId, CancellationToken cancellationToken = default);
    Task RemoveUserShortcutAsync(int usuarioId, int atalhoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CurrencyQuote>> GetCurrencyQuotesAsync(CancellationToken cancellationToken = default);
    Task<WeatherInfo?> GetWeatherInfoAsync(int estabelecimentoId, CancellationToken cancellationToken = default);
}
