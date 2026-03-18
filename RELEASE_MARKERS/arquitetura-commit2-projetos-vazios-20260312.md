# Commit 2 — Criar projetos de arquitetura (vazios)

Data: 2026-03-12

## Objetivo
Introduzir `SIC.Domain` e `SIC.Infrastructure` sem mover lógica existente.

## Ações executadas
- Projeto criado: `SIC.Domain` (`net10.0`, class library)
- Projeto criado: `SIC.Infrastructure` (`net10.0`, class library)
- Projetos adicionados à solução: `SIC.slnx`
- Referência adicionada: `SIC.Infrastructure` -> `SIC.Domain`
- Referências adicionadas: `SIC.Api` -> `SIC.Domain`, `SIC.Infrastructure`

## Validação
- `dotnet restore SIC.slnx`: **OK**
- `dotnet build SIC.slnx -c Debug`: **OK**

## Observação importante
A sessão do Visual Studio ainda está com a solução antiga carregada (sem os novos projetos), então o build interno da IDE (`run_build`) pode continuar falhando até recarregar/reabrir a solução.
