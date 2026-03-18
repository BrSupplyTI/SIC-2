# Commit 7 — Extrair repositório de Auth (contrato)

Data: 2026-03-12

## Objetivo
Preparar a remoção de SQL de `SicAuthService` criando o contrato de autenticação no domínio, sem trocar implementação ainda.

## Criado em `SIC.Domain/Abstractions`
- `IAuthRepository.cs`

### Escopo coberto no contrato
- Login (senha, e-mail, identificador)
- Sessão (criar, renovar, desativar, expirar)
- Estabelecimentos (listar autorizados, autorizar troca, alterar)
- Senha/foto (alterar, reset, atualizar foto)
- Auditoria/apoio (último login, log de eventos)

## Entidades de apoio adicionadas em `SIC.Domain/Entities`
- `AuthUser.cs`
- `AuthEstablishment.cs`

## Validação
- `run_build`: **Compilação bem-sucedida**.

## Observação
- Nenhuma implementação foi trocada neste commit.
- `SicAuthService` segue funcionando como está; este commit apenas prepara o próximo passo (`SqlAuthRepository`).
