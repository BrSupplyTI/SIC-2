# Copilot Instructions

## Diretrizes de projeto
- Para simplificar a arquitetura, o usuário prefere aplicar Dependency Inversion de forma pragmática: interfaces de integração (ex.: IEmailService) no SIC.Domain/Abstractions e implementações no SIC.Infrastructure/Integrations.
- O usuário prefere evitar scripts em outras linguagens (PowerShell, Bash, etc.) e quer que tudo seja feito em C# sempre que possível. Apenas usar scripts quando absolutamente necessário.
- O usuário quer migrações fiéis de telas antigas em PHP para C#, sem reinventar lógica ou alterar regras de negócio; modernizar apenas o layout no padrão atual do SIC e priorizar C# sobre scripts. Analisar a pasta antiga primeiro e replicar exatamente.
- Na migração da tela de Pré-Pedido, manter a lógica e o fluxo da tela antiga em PHP sem alterar regras de negócio, priorizando C# e usando JavaScript apenas para interação de tela quando necessário. Os botões de ação da grid devem usar ícones no mesmo padrão visual da tela de lista.
- Na tela de Pré-Pedido, os campos CNPJ, Endereço e Local de Entrega devem se comportar como um select pesquisável no próprio controle, semelhante ao chosen do PHP, e não como input separado com lista. Os campos devem permitir digitação/filtro no estilo chosen, e a quantidade deve atualizar automaticamente sem botão Salvar.