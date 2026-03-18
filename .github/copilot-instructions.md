# Copilot Instructions

## Diretrizes de projeto
- Para simplificar a arquitetura, o usuário prefere aplicar Dependency Inversion de forma pragmática: interfaces de integração (ex.: IEmailService) no SIC.Domain/Abstractions e implementações no SIC.Infrastructure/Integrations.