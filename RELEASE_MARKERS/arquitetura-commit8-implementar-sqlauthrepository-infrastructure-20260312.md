# Commit 8 — Implementar SqlAuthRepository na Infrastructure

Data: 2026-03-12

## Objetivo
Concentrar SQL de autenticação na camada Infrastructure, sem trocar a implementação em uso na API ainda.

## Criado
- `SIC.Infrastructure/Repositories/SqlAuthRepository.cs`

## Implementado no repositório
- Operações de usuário para auth:
  - login por senha
  - login por e-mail
  - busca por identificador (login/e-mail)
  - e-mail ativo por usuário
- Operações de sessão:
  - criar sessão (com desativação prévia de ativas)
  - renovar sessão
  - desativar sessão
  - limpeza de sessões expiradas
- Operações de estabelecimentos:
  - listar autorizados
  - validar autorização
  - trocar estabelecimento
- Operações de atualização:
  - foto de usuário
  - alteração de senha
  - reset de senha por SQL configurável
- Operações de suporte/auditoria:
  - atualizar último login
  - inserir logs em `Intranet_Log`
- Helpers internos:
  - mapeamento para `AuthUser`
  - leitura de tipos nulos/flexíveis
  - detecção de coluna em `view_login`

## Observação
- Commit de preparação: `SicAuthService` ainda não foi alterado para usar `IAuthRepository`.

## Validação
- `run_build`: **Compilação bem-sucedida**.

## Smoke funcional
- Login/sessão/troca estabelecimento: validação manual ficará para o próximo commit (quando a API passar a usar esse repositório).
