# Commit 4 — Mover contrato de Profile Repository para Domain

Data: 2026-03-12

## Objetivo
Estabilizar contrato de repositório de perfil no domínio.

## Movimentações executadas
### Movido
- `SIC.Api/Repositories/IUserProfileRepository.cs`
  -> `SIC.Domain/Abstractions/IUserProfileRepository.cs`

## Ajustes realizados
- Atualizado `using` em:
  - `SIC.Api/Services/UserProfileService.cs` (`SIC.Domain.Abstractions`)
  - `SIC.Api/Repositories/SqlUserProfileRepository.cs` (`SIC.Domain.Abstractions`)
  - `SIC.Api/Program.cs` (`SIC.Domain.Abstractions` para DI)

## Validação
- `run_build`: **Compilação bem-sucedida**.
- `dotnet build SIC.slnx -c Debug -v minimal`: **Build bem-sucedido**.

## Smoke funcional
- GET/PUT `api/profile` e tela `MeusDados`: validação manual recomendada em execução da aplicação.
