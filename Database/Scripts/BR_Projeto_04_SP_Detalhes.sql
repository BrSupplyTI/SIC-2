-- ============================================================
-- Módulo Projetos — Stored Procedures de Leitura (Detalhes)
-- Arquivo: BR_Projeto_04_SP_Detalhes.sql
-- Descrição: SPs de leitura para tela de detalhes do projeto
--            (dados do projeto, tarefas, participantes, histórico).
-- Pré-requisito: BR_Projeto_01 + BR_Projeto_02
-- ============================================================

-- ************************************************************
-- 1. SIC_ProjetoDetalhes
--    Retorna os dados de um projeto específico.
-- ************************************************************
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetoDetalhes') AND type = N'P')
    DROP PROCEDURE SIC_ProjetoDetalhes;
GO

CREATE PROCEDURE SIC_ProjetoDetalhes
    @ProjetoID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.ProjetoID,
        P.NmProjeto,
        P.DsProjeto,
        P.ProjetoStatusID,
        S.NmStatus,
        S.CdCor                     AS CdCorStatus,
        P.DtInicio,
        P.DtPrevisaoFim,
        P.DtFimReal,
        P.UsuarioCriadorID,
        ISNULL(U.NmUsuario, '')     AS NmCriador,
        P.DtCriacao,
        P.DtUltimaAtualizacao,

        -- Contadores de tarefas (apenas ativas)
        ISNULL((
            SELECT COUNT(*)
            FROM BR_ProjetoTarefa T WITH (NOLOCK)
            WHERE T.ProjetoID = P.ProjetoID
              AND T.FlagAtivo = 1
        ), 0) AS QtTarefas,

        ISNULL((
            SELECT COUNT(*)
            FROM BR_ProjetoTarefa T WITH (NOLOCK)
            WHERE T.ProjetoID = P.ProjetoID
              AND T.FlagAtivo = 1
              AND T.ProjetoTarefaStatusID = (
                  SELECT TOP 1 TS.ProjetoTarefaStatusID
                  FROM BR_ProjetoTarefaStatus TS WITH (NOLOCK)
                  WHERE TS.NmStatus = N'Concluída'
                    AND TS.FlagAtivo = 1
              )
        ), 0) AS QtTarefasConcluidas

    FROM BR_Projeto P WITH (NOLOCK)
    INNER JOIN BR_ProjetoStatus S WITH (NOLOCK) ON S.ProjetoStatusID = P.ProjetoStatusID
    LEFT JOIN BrSupply.dbo.BR_Usuario U WITH (NOLOCK) ON U.UsuarioID = P.UsuarioCriadorID
    WHERE P.ProjetoID = @ProjetoID
      AND P.FlagAtivo = 1;
END
GO

PRINT 'Stored Procedure SIC_ProjetoDetalhes criada com sucesso.';
GO

-- ************************************************************
-- 2. SIC_ProjetoTarefasListar
--    Retorna todas as tarefas (incluindo subtarefas) de um
--    projeto. A montagem hierárquica é feita na aplicação
--    usando ProjetoTarefaPaiID.
-- ************************************************************
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetoTarefasListar') AND type = N'P')
    DROP PROCEDURE SIC_ProjetoTarefasListar;
GO

CREATE PROCEDURE SIC_ProjetoTarefasListar
    @ProjetoID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        T.ProjetoTarefaID,
        T.ProjetoID,
        T.NmTarefa,
        T.DsTarefa,
        T.ProjetoTarefaStatusID,
        TS.NmStatus,
        TS.CdCor                        AS CdCorStatus,
        T.ProjetoTarefaPrioridadeID,
        TP.NmPrioridade,
        TP.CdCor                        AS CdCorPrioridade,
        T.UsuarioResponsavelID,
        ISNULL(U.NmUsuario, '')         AS NmResponsavel,
        T.DtInicio,
        T.DtPrevisaoFim,
        T.DtFimReal,
        T.NrOrdem,
        T.ProjetoTarefaPaiID
    FROM BR_ProjetoTarefa T WITH (NOLOCK)
    INNER JOIN BR_ProjetoTarefaStatus TS WITH (NOLOCK) ON TS.ProjetoTarefaStatusID = T.ProjetoTarefaStatusID
    INNER JOIN BR_ProjetoTarefaPrioridade TP WITH (NOLOCK) ON TP.ProjetoTarefaPrioridadeID = T.ProjetoTarefaPrioridadeID
    LEFT JOIN BrSupply.dbo.BR_Usuario U WITH (NOLOCK) ON U.UsuarioID = T.UsuarioResponsavelID
    WHERE T.ProjetoID = @ProjetoID
      AND T.FlagAtivo = 1
    ORDER BY
        ISNULL(T.ProjetoTarefaPaiID, T.ProjetoTarefaID),   -- agrupa pai + filhos
        CASE WHEN T.ProjetoTarefaPaiID IS NULL THEN 0 ELSE 1 END,  -- pai antes dos filhos
        T.NrOrdem;
END
GO

PRINT 'Stored Procedure SIC_ProjetoTarefasListar criada com sucesso.';
GO

-- ************************************************************
-- 3. SIC_ProjetoParticipantesListar
--    Retorna os participantes ativos de um projeto.
-- ************************************************************
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetoParticipantesListar') AND type = N'P')
    DROP PROCEDURE SIC_ProjetoParticipantesListar;
GO

CREATE PROCEDURE SIC_ProjetoParticipantesListar
    @ProjetoID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        PP.ProjetoParticipanteID,
        PP.UsuarioID,
        ISNULL(U.NmUsuario, '')     AS NmUsuario,
        PP.NmPapel,
        PP.DtEntrada
    FROM BR_ProjetoParticipante PP WITH (NOLOCK)
    LEFT JOIN BrSupply.dbo.BR_Usuario U WITH (NOLOCK) ON U.UsuarioID = PP.UsuarioID
    WHERE PP.ProjetoID = @ProjetoID
      AND PP.FlagAtivo = 1
    ORDER BY PP.DtEntrada;
END
GO

PRINT 'Stored Procedure SIC_ProjetoParticipantesListar criada com sucesso.';
GO

-- ************************************************************
-- 4. SIC_ProjetoHistoricoListar
--    Retorna o histórico de ações de um projeto (timeline).
-- ************************************************************
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetoHistoricoListar') AND type = N'P')
    DROP PROCEDURE SIC_ProjetoHistoricoListar;
GO

CREATE PROCEDURE SIC_ProjetoHistoricoListar
    @ProjetoID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        H.ProjetoHistoricoID,
        H.UsuarioID,
        ISNULL(U.NmUsuario, '')     AS NmUsuario,
        H.DsAcao,
        H.DtAcao
    FROM BR_ProjetoHistorico H WITH (NOLOCK)
    LEFT JOIN BrSupply.dbo.BR_Usuario U WITH (NOLOCK) ON U.UsuarioID = H.UsuarioID
    WHERE H.ProjetoID = @ProjetoID
    ORDER BY H.DtAcao DESC;
END
GO

PRINT 'Stored Procedure SIC_ProjetoHistoricoListar criada com sucesso.';
GO

PRINT '============================================================';
PRINT 'BR_Projeto_04_SP_Detalhes.sql executado com sucesso.';
PRINT '============================================================';
GO
