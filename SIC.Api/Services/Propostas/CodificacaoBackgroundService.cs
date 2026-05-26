using SIC.Domain.Abstractions.Propostas;

namespace SIC.Api.Services.Propostas;

public sealed class CodificacaoBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<CodificacaoBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CodificacaoBackgroundService iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarPropostasPendentesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Erro no CodificacaoBackgroundService.");
            }

            await Task.Delay(Intervalo, stoppingToken);
        }

        logger.LogInformation("CodificacaoBackgroundService finalizado.");
    }

    private async Task ProcessarPropostasPendentesAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPropostaQueryRepository>();

        var propostas = await repository.GetPropostasPendentesSegundoPlanoAsync(stoppingToken);

        if (propostas.Count == 0)
            return;

        logger.LogInformation("Encontrada(s) {Count} proposta(s) pendente(s) de codificação.", propostas.Count);

        foreach (var (propostaId, estabelecimentoId) in propostas)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                await CodificarPropostaAsync(repository, propostaId, estabelecimentoId, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Erro ao codificar proposta {PropostaID}.", propostaId);
            }
        }
    }

    private async Task CodificarPropostaAsync(
        IPropostaQueryRepository repository,
        int propostaId,
        int estabelecimentoId,
        CancellationToken stoppingToken)
    {
        // Marcar como "Em Processamento" (StatusID = 10)
        await repository.AtualizarStatusPropostaAsync(propostaId, 10, stoppingToken);

        logger.LogInformation("Iniciando codificação da proposta {PropostaID}.", propostaId);

        var itens = await repository.GetItensNaoCodificadosAsync(propostaId, stoppingToken);

        var codificados = 0;
        var semCorrespondencia = 0;

        foreach (var propostaItemId in itens)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                var result = await repository.CodificarItemAsync(propostaItemId, estabelecimentoId, stoppingToken);

                if (result.Codificado)
                    codificados++;
                else
                    semCorrespondencia++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Erro ao codificar item {PropostaItemID} da proposta {PropostaID}.", propostaItemId, propostaId);
                semCorrespondencia++;
            }
        }

        // Verificar se ainda restam itens não codificados
        var pendentes = await repository.GetItensNaoCodificadosAsync(propostaId, stoppingToken);

        if (pendentes.Count == 0)
        {
            // Todos codificados → Marcar como "Codificação Realizada" (StatusID = 11)
            await repository.AtualizarStatusPropostaAsync(propostaId, 11, stoppingToken);
        }
        else
        {
            // Ainda há pendentes → voltar para status inicial (StatusID = 9)
            await repository.AtualizarStatusPropostaAsync(propostaId, 9, stoppingToken);
        }

        logger.LogInformation(
            "Proposta {PropostaID} processada: {Codificados} codificado(s), {SemCorrespondencia} sem correspondência, {Pendentes} pendente(s).",
            propostaId, codificados, semCorrespondencia, pendentes.Count);
    }
}
