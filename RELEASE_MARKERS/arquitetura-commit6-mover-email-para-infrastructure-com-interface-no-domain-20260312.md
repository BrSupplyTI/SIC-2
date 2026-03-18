# Commit 6 — Mover integração de e-mail para Infrastructure (interface no Domain)

Data: 2026-03-12

## Objetivo
Aplicar DIP de forma simples e pragmática:
- Interface no Domain
- Implementação no Infrastructure

## Movimentações executadas
### Interface movida para Domain
- Criado: `SIC.Domain/Abstractions/IEmailService.cs`
- Removido: `SIC.Api/Services/IEmailService.cs`

### Implementação movida para Infrastructure
- Criado: `SIC.Infrastructure/Integrations/SmtpEmailService.cs`
- Removido: `SIC.Api/Services/SmtpEmailService.cs`

## Ajustes de referência e DI
- `SIC.Api/Services/SicAuthService.cs`
  - usa `using SIC.Domain.Abstractions;`
- `SIC.Api/Program.cs`
  - usa `using SIC.Infrastructure.Integrations;`
  - mantém registro: `AddScoped<IEmailService, SmtpEmailService>()`

## Ajustes no projeto Infrastructure
- `SIC.Infrastructure.csproj`
  - adicionado `Microsoft.Extensions.Configuration.Binder` (GetValue<T>)

## Validação
- `run_build`: **Compilação bem-sucedida**.

## Smoke funcional
- Fluxo forgot/reset password: validação manual recomendada com aplicação em execução.
