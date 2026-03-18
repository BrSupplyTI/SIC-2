# Commit 10 — Reduzir duplicação Profile no Web (fase 1)

Data: 2026-03-12

## Objetivo
Limpeza segura e pequena no módulo `Meus Dados`, reduzindo ViewModels duplicados.

## Ações executadas
- `SIC.Web/Services/SicAuthApiClient.cs`
  - `GetMyProfileAsync` agora desserializa para `MyDataPageVm`
  - `UpdateMyProfileAsync` agora recebe `MyDataPageVm` e projeta payload mínimo para `api/profile`
- `SIC.Web/Controllers/AccountController.cs`
  - GET: usa diretamente `MyDataPageVm` retornado pela API e injeta `Areas`
  - POST: usa `MyDataPageVm` diretamente (define `UsuarioId` por claim)
- Arquivos removidos (não utilizados após refatoração):
  - `SIC.Web/Models/Profile/UserProfileVm.cs`
  - `SIC.Web/Models/Profile/UpdateUserProfileVm.cs`

## Resultado
- `MyDataPageVm` passa a ser o modelo principal da tela `MeusDados`.
- Menos duplicação de modelos no Web sem alteração de arquitetura geral.

## Validação
- `run_build`: **Compilação bem-sucedida**.

## Smoke funcional recomendado (manual)
- `Meus Dados` GET/POST
- atualização/exclusão de foto
- matrícula numérica no formulário
