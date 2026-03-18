# Commit 5 — Mover implementação SQL de Profile para Infrastructure

Data: 2026-03-12

## Objetivo
Remover implementação SQL de Profile da API, mantendo apenas contrato/serviço na camada de aplicação.

## Movimentações executadas
### Movido
- `SIC.Api/Repositories/SqlUserProfileRepository.cs`
  -> `SIC.Infrastructure/Repositories/SqlUserProfileRepository.cs`

### Ajustes de namespace/usings
- Novo namespace da implementação: `SIC.Infrastructure.Repositories`
- `Program.cs` da API ajustado para usar `using SIC.Infrastructure.Repositories;`

### Dependências ajustadas em Infrastructure
- `Microsoft.Data.SqlClient` (PackageReference)
- `Microsoft.Extensions.Configuration.Abstractions` (PackageReference)
- `using Microsoft.Extensions.Configuration;` no repositório

## DI (API)
- Mantido registro:
  - `builder.Services.AddScoped<IUserProfileRepository, SqlUserProfileRepository>();`
- Agora apontando para implementação em `SIC.Infrastructure`.

## Validação
- `run_build`: **Compilação bem-sucedida**.
- `dotnet build SIC.slnx -c Debug -v minimal`: **Build bem-sucedido**.

## Smoke funcional
- Fluxo completo `Meus Dados` (carregar perfil, atualizar cadastro, atualizar/excluir foto): validação manual recomendada em runtime.
