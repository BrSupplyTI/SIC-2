# Commit 9 — Refatorar SicAuthService para usar IAuthRepository

Data: 2026-03-12

## Objetivo
Transformar `SicAuthService` em camada de orquestração, removendo SQL direto.

## Ações executadas
- `SIC.Api/Services/SicAuthService.cs`
  - removido acesso SQL direto (`SqlConnection`/`SqlCommand` e helpers SQL internos)
  - injeção de `IAuthRepository` no construtor
  - métodos agora delegam operações de dados para `IAuthRepository`
  - mantida lógica de aplicação (validações, mensagens, geração de token de reset, envio de e-mail)
- `SIC.Api/Program.cs`
  - adicionado DI: `AddScoped<IAuthRepository, SqlAuthRepository>();`

## Resultado arquitetural
- Service de aplicação focado em orquestração
- SQL concentrado em `SIC.Infrastructure/Repositories/SqlAuthRepository.cs`

## Validação técnica
- `run_build`: **Compilação bem-sucedida**.

## Validação funcional recomendada (manual)
- Login por senha
- Login SSO
- Sessão única e invalidação de sessão anterior
- Logout
- Reset de senha (solicitação e confirmação)
- Troca de estabelecimento
