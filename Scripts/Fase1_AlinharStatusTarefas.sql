-- ============================================================
-- Fase 1: Alinhar status na tabela BR_ProjetoTarefaStatus
--
-- Os 4 status reais do sistema:
--   1. Pendente
--   2. Em Andamento
--   3. Cancelado
--   4. Concluído   ← DEVE ser o último (maior NrOrdem)
--
-- IMPORTANTE: "Concluído" precisa ter o maior NrOrdem porque
-- o Kanban usa o último status como critério de "concluído"
-- no cálculo de progresso das subtarefas:
--   var lastStatusId = statusColumns.LastOrDefault()
-- ============================================================

-- ── 1. Verificação ANTES da correção ──
PRINT '=== ANTES da correção ===';
SELECT ProjetoTarefaStatusID, NmStatus, CdCor, NrOrdem, FlagAtivo
FROM BrWeb.dbo.BR_ProjetoTarefaStatus WITH (NOLOCK)
ORDER BY NrOrdem;

-- ── 2. Desativar status que não são os 4 reais ──
UPDATE BrWeb.dbo.BR_ProjetoTarefaStatus
SET    FlagAtivo = 0
WHERE  NmStatus NOT IN ('Pendente', 'Em Andamento', 'Cancelado', N'Concluído')
  AND  FlagAtivo = 1;

-- ── 3. Garantir que os 4 status existam ──
-- (INSERT apenas se não existirem; preserva cores existentes)

IF NOT EXISTS (SELECT 1 FROM BrWeb.dbo.BR_ProjetoTarefaStatus WHERE NmStatus = 'Pendente')
    INSERT INTO BrWeb.dbo.BR_ProjetoTarefaStatus (NmStatus, CdCor, NrOrdem, FlagAtivo)
    VALUES ('Pendente', '#6c757d', 1, 1);

IF NOT EXISTS (SELECT 1 FROM BrWeb.dbo.BR_ProjetoTarefaStatus WHERE NmStatus = 'Em Andamento')
    INSERT INTO BrWeb.dbo.BR_ProjetoTarefaStatus (NmStatus, CdCor, NrOrdem, FlagAtivo)
    VALUES ('Em Andamento', '#0d6efd', 2, 1);

IF NOT EXISTS (SELECT 1 FROM BrWeb.dbo.BR_ProjetoTarefaStatus WHERE NmStatus = 'Cancelado')
    INSERT INTO BrWeb.dbo.BR_ProjetoTarefaStatus (NmStatus, CdCor, NrOrdem, FlagAtivo)
    VALUES ('Cancelado', '#dc3545', 3, 1);

IF NOT EXISTS (SELECT 1 FROM BrWeb.dbo.BR_ProjetoTarefaStatus WHERE NmStatus = N'Concluído')
    INSERT INTO BrWeb.dbo.BR_ProjetoTarefaStatus (NmStatus, CdCor, NrOrdem, FlagAtivo)
    VALUES (N'Concluído', '#198754', 4, 1);

-- ── 4. Atualizar NrOrdem e ativar os 4 status ──
UPDATE BrWeb.dbo.BR_ProjetoTarefaStatus SET FlagAtivo = 1, NrOrdem = 1 WHERE NmStatus = 'Pendente';
UPDATE BrWeb.dbo.BR_ProjetoTarefaStatus SET FlagAtivo = 1, NrOrdem = 2 WHERE NmStatus = 'Em Andamento';
UPDATE BrWeb.dbo.BR_ProjetoTarefaStatus SET FlagAtivo = 1, NrOrdem = 3 WHERE NmStatus = 'Cancelado';
UPDATE BrWeb.dbo.BR_ProjetoTarefaStatus SET FlagAtivo = 1, NrOrdem = 4 WHERE NmStatus = N'Concluído';

-- ── 5. Verificação: tarefas órfãs (vinculadas a status desativado) ──
PRINT '=== Tarefas com status desativado (se houver, requerem migração) ===';
SELECT T.ProjetoTarefaID, T.NmTarefa, T.ProjetoTarefaStatusID, S.NmStatus AS StatusAtual, S.FlagAtivo
FROM   BrWeb.dbo.BR_ProjetoTarefa T WITH (NOLOCK)
JOIN   BrWeb.dbo.BR_ProjetoTarefaStatus S WITH (NOLOCK) ON S.ProjetoTarefaStatusID = T.ProjetoTarefaStatusID
WHERE  S.FlagAtivo = 0;

-- ── 6. Verificação DEPOIS da correção ──
PRINT '=== DEPOIS da correção ===';
SELECT ProjetoTarefaStatusID, NmStatus, CdCor, NrOrdem, FlagAtivo
FROM BrWeb.dbo.BR_ProjetoTarefaStatus WITH (NOLOCK)
ORDER BY NrOrdem;
