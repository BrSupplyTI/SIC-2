# SIC

Arquitetura atual da solução (migração PHP -> C# por módulos):

## Projetos

- `SIC.Web`
  - UI em ASP.NET Core (Razor Pages + MVC pontual para `MeusDados`)
  - Consumo de API via `HttpClient` (`SicAuthApiClient`)
  - Sem acesso direto a banco

- `SIC.Api`
  - Controllers REST
  - DTOs de request/response
  - Services de aplicação (orquestração)
  - Sem SQL direto em `SicAuthService` e `UserProfileService`

- `SIC.Domain`
  - Entidades de domínio
  - Abstrações (interfaces) para infraestrutura
    - `IUserProfileRepository`
    - `IAuthRepository`
    - `IEmailService`

- `SIC.Infrastructure`
  - Implementações de infraestrutura
    - `Repositories/SqlUserProfileRepository`
    - `Repositories/SqlAuthRepository`
    - `Integrations/SmtpEmailService`
  - Acesso SQL concentrado nesta camada

## Princípios aplicados

- Dependency Inversion pragmático:
  - Interfaces no `SIC.Domain/Abstractions`
  - Implementações no `SIC.Infrastructure`
- Services na API focados em regra de aplicação/orquestração
- Controllers finos
- Redução incremental de duplicação de modelos no Web

## Checklist funcional de regressão (manual)

### Auth
- [ ] Login por senha
- [ ] Login SSO (Azure)
- [ ] Sessão única (novo login derruba sessão anterior)
- [ ] Logoff
- [ ] Validação de sessão no cookie

### Senha
- [ ] Esqueci senha (solicitação)
- [ ] Reset de senha por token
- [ ] Alterar senha pelo menu do usuário

### Meus Dados
- [ ] Carregar ficha de usuário
- [ ] Atualizar telefone, ramal, matrícula, cargo, setor, área
- [ ] Matrícula aceita apenas número
- [ ] Upload de foto
- [ ] Exclusão de foto

### Estabelecimento
- [ ] Listar estabelecimentos autorizados
- [ ] Troca de estabelecimento e atualização de claims

## Observação

A migração segue em commits pequenos para minimizar risco e manter o sistema sempre compilando e funcional.
