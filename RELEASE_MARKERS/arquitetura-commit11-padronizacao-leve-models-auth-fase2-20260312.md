# Commit 11 — Padronização leve de models Auth (fase 2 opcional)

Data: 2026-03-12

## Objetivo
Reduzir VMs espelhados de Auth no Web sem big-bang.

## Ações executadas
### Refatoração no client Web
- `SIC.Web/Services/SicAuthApiClient.cs`
  - Substituídos modelos de request por payloads anônimos locais nos métodos:
    - `PasswordLoginAsync`
    - `SsoLoginAsync`
    - `ResetPasswordAsync`
    - `GetEstablishmentsAsync`
    - `ChangeEstablishmentAsync`
    - `UpdateUserPhotoAsync`
    - `ChangePasswordAsync`

### Arquivos removidos (VMs de request espelhados, não mais usados)
- `SIC.Web/Models/Auth/LoginRequestVm.cs`
- `SIC.Web/Models/Auth/SsoLoginRequestVm.cs`
- `SIC.Web/Models/Auth/ResetPasswordRequestVm.cs`
- `SIC.Web/Models/Auth/EstablishmentListRequestVm.cs`
- `SIC.Web/Models/Auth/ChangeEstablishmentRequestVm.cs`
- `SIC.Web/Models/Auth/UpdateUserPhotoRequestVm.cs`
- `SIC.Web/Models/Auth/ChangePasswordRequestVm.cs`

## Resultado
- Menos duplicação de modelos de request no Web.
- Mantido comportamento e contratos HTTP da API.

## Validação
- `run_build`: **Compilação bem-sucedida**.

## Regressão funcional recomendada (manual)
- Login senha e SSO
- Esqueci/Reset de senha
- Sessão (validate/logout)
- Troca de estabelecimento
- Atualização de foto e troca de senha
