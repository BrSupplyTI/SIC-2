using SIC.Api.Contracts.Home;

namespace SIC.Api.Services;

public interface IHomeService
{
    Task<IReadOnlyList<ShortcutDto>> GetUserShortcutsAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShortcutDto>> GetAllShortcutsAsync(CancellationToken cancellationToken = default);
    Task AddUserShortcutAsync(int usuarioId, int atalhoId, string estilo, CancellationToken cancellationToken = default);
    Task RemoveUserShortcutAsync(int usuarioId, int atalhoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CurrencyQuoteDto>> GetCurrencyQuotesAsync(CancellationToken cancellationToken = default);
    Task<WeatherInfoDto?> GetWeatherInfoAsync(int estabelecimentoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonitorDto>> GetAllMonitorsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserMonitorResultDto>> GetUserMonitorResultsAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task AddUserMonitorAsync(int usuarioId, int monitorId, string valor, CancellationToken cancellationToken = default);
    Task RemoveUserMonitorAsync(int usuarioId, int usuarioMonitorId, CancellationToken cancellationToken = default);
}
