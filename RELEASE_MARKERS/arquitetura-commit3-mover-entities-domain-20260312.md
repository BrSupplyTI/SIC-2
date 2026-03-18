# Commit 3 — Mover Entities para Domain

Data: 2026-03-12

## Objetivo
Separar entidades de domínio no projeto `SIC.Domain` com risco baixo.

## Movimentações executadas
### Criado em `SIC.Domain/Entities`
- `UserProfile.cs`
- `UserPermission.cs`
- `AreaOption.cs`

### Removido de `SIC.Api/Domain/Entities`
- `UserProfile.cs`
- `UserPermission.cs`
- `AreaOption.cs`

## Ajustes realizados
- Atualizados namespaces/usings:
  - `SIC.Api/Repositories/IUserProfileRepository.cs`
  - `SIC.Api/Repositories/SqlUserProfileRepository.cs`
- Novo namespace de entidades: `SIC.Domain.Entities`

## Validação
- `run_build`: **Compilação bem-sucedida**.
- `dotnet build SIC.slnx -c Debug`: **Build bem-sucedido**.

## Smoke funcional
- Endpoints de perfil: validação funcional manual pendente em ambiente de execução da API/Web.
