-- ============================================================
-- Módulo Projetos — Stored Procedures de Escrita (Tarefa)
-- Arquivo: BR_Projeto_06_SP_TarefaEscrita.sql
-- Descrição: SPs de criação, atualização e exclusão (lógica)
--            de tarefas. Todas registram no BR_ProjetoHistorico.
-- Pré-requisito: BR_Projeto_01 + BR_Projeto_02
-- ============================================================

-- ************************************************************
-- 1. SIC_ProjetoTarefaCriar
--    Cria uma tarefa (ou subtarefa) no projeto.
--    Calcula NrOrdem automaticamente.
--    Retorna o ProjetoTarefaID criado.
-- ************************************************************
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetoTarefaCriar') AND type = N'P')
    DROP PROCEDURE SIC_ProjetoTarefaCriar;
GO

CREATE PROCEDURE SIC_ProjetoTarefaCriar
    @ProjetoID                  INT,
    @NmTarefa                   VARCHAR(300),
    @DsTarefa                   VARCHAR(2000)   = NULL,
    @ProjetoTarefaStatusID      INT             = 1,
    @ProjetoTarefaPrioridadeID  INT             = 2,
    @UsuarioResponsavelID       INT             = NULL,
    @DtInicio                   DATE            = NULL,
    @DtPrevisaoFim              DATE            = NULL,
    @ProjetoTarefaPaiID         INT             = NULL,
    @UsuarioID                  INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ProjetoTarefaID INT;
    DECLARE @Agora DATETIME = GETDATE();
    DECLARE @NrOrdem INT;

    -- Calcular próximo NrOrdem dentro do mesmo nível (pai ou raiz)
    SELECT @NrOrdem = ISNULL(MAX(T.NrOrdem), 0) + 1
    FROM BR_ProjetoTarefa T WITH (NOLOCK)
    WHERE T.ProjetoID = @ProjetoID
      AND T.FlagAtivo = 1
      AND (
            (@ProjetoTarefaPaiID IS NULL AND T.ProjetoTarefaPaiID IS NULL)
            OR T.ProjetoTarefaPaiID = @ProjetoTarefaPaiID
          );

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Inserir a tarefa
        INSERT INTO BR_ProjetoTarefa (
            ProjetoID, NmTarefa, DsTarefa,
            ProjetoTarefaStatusID, ProjetoTarefaPrioridadeID,
            UsuarioResponsavelID, DtInicio, DtPrevisaoFim,
            NrOrdem, ProjetoTarefaPaiID, DtCriacao
        )
        VALUES (
            @ProjetoID, @NmTarefa, @DsTarefa,
            @ProjetoTarefaStatusID, @ProjetoTarefaPrioridadeID,
            @UsuarioResponsavelID, @DtInicio, @DtPrevisaoFim,
            @NrOrdem, @ProjetoTarefaPaiID, @Agora
        );

        SET @ProjetoTarefaID = SCOPE_IDENTITY();

        -- 2. Registrar no histórico
        DECLARE @DsAcao VARCHAR(500);

        IF @ProjetoTarefaPaiID IS NOT NULL
        BEGIN
            DECLARE @NmTarefaPai VARCHAR(300);
            SELECT @NmTarefaPai = NmTarefa FROM BR_ProjetoTarefa WITH (NOLOCK) WHERE ProjetoTarefaID = @ProjetoTarefaPaiID;
            SET @DsAcao = 'Criou a subtarefa "' + @NmTarefa + '" em "' + ISNULL(@NmTarefaPai, '') + '"';
        END
        ELSE
        BEGIN
            SET @DsAcao = 'Criou a tarefa "' + @NmTarefa + '"';
        END

        INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
        VALUES (@ProjetoID, @UsuarioID, @DsAcao, @Agora);

        -- 3. Atualizar DtUltimaAtualizacao do projeto
        UPDATE BR_Projeto SET DtUltimaAtualizacao = @Agora WHERE ProjetoID = @ProjetoID;

        COMMIT TRANSACTION;

        SELECT @ProjetoTarefaID AS ProjetoTarefaID;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SIC_ProjetoTarefaCriar criada com sucesso.';
GO

-- ************************************************************
-- 2. SIC_ProjetoTarefaAtualizar
--    Atualiza dados de uma tarefa e registra no histórico.
-- ************************************************************
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetoTarefaAtualizar') AND type = N'P')
    DROP PROCEDURE SIC_ProjetoTarefaAtualizar;
GO

CREATE PROCEDURE SIC_ProjetoTarefaAtualizar
    @ProjetoTarefaID            INT,
    @NmTarefa                   VARCHAR(300),
    @DsTarefa                   VARCHAR(2000)   = NULL,
    @ProjetoTarefaStatusID      INT,
    @ProjetoTarefaPrioridadeID  INT,
    @UsuarioResponsavelID       INT             = NULL,
    @DtInicio                   DATE            = NULL,
    @DtPrevisaoFim              DATE            = NULL,
    @DtFimReal                  DATE            = NULL,
    @UsuarioID                  INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Agora DATETIME = GETDATE();

    -- Capturar valores atuais
    DECLARE @ProjetoID              INT;
    DECLARE @OldNmTarefa            VARCHAR(300);
    DECLARE @OldTarefaStatusID      INT;
    DECLARE @OldTarefaPrioridadeID  INT;

    SELECT
        @ProjetoID              = T.ProjetoID,
        @OldNmTarefa            = T.NmTarefa,
        @OldTarefaStatusID      = T.ProjetoTarefaStatusID,
        @OldTarefaPrioridadeID  = T.ProjetoTarefaPrioridadeID
    FROM BR_ProjetoTarefa T WITH (NOLOCK)
    WHERE T.ProjetoTarefaID = @ProjetoTarefaID
      AND T.FlagAtivo = 1;

    IF @ProjetoID IS NULL
    BEGIN
        RAISERROR('Tarefa não encontrada ou inativa.', 16, 1);
        RETURN;
    END

    -- Bloqueio: tarefa-pai não pode ser concluída com subtarefas pendentes
    IF @OldTarefaStatusID <> @ProjetoTarefaStatusID
    BEGIN
        DECLARE @StatusConcluidaID INT;
        SELECT TOP 1 @StatusConcluidaID = ProjetoTarefaStatusID
        FROM BR_ProjetoTarefaStatus WITH (NOLOCK)
        WHERE NmStatus = N'Concluída' AND FlagAtivo = 1;

        IF @ProjetoTarefaStatusID = @StatusConcluidaID
        BEGIN
            DECLARE @QtSubtarefasPendentes INT;
            SELECT @QtSubtarefasPendentes = COUNT(*)
            FROM BR_ProjetoTarefa WITH (NOLOCK)
            WHERE ProjetoTarefaPaiID = @ProjetoTarefaID
              AND FlagAtivo = 1
              AND ProjetoTarefaStatusID <> @StatusConcluidaID;

            IF @QtSubtarefasPendentes > 0
            BEGIN
                RAISERROR('Não é possível concluir esta tarefa. Existem %d subtarefa(s) pendente(s).', 16, 1, @QtSubtarefasPendentes);
                RETURN;
            END
        END
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Atualizar a tarefa
        UPDATE BR_ProjetoTarefa
        SET NmTarefa                    = @NmTarefa,
            DsTarefa                    = @DsTarefa,
            ProjetoTarefaStatusID       = @ProjetoTarefaStatusID,
            ProjetoTarefaPrioridadeID   = @ProjetoTarefaPrioridadeID,
            UsuarioResponsavelID        = @UsuarioResponsavelID,
            DtInicio                    = @DtInicio,
            DtPrevisaoFim              = @DtPrevisaoFim,
            DtFimReal                   = @DtFimReal,
            DtUltimaAtualizacao         = @Agora
        WHERE ProjetoTarefaID = @ProjetoTarefaID;

        -- 2. Registrar mudanças relevantes no histórico
        IF @OldTarefaStatusID <> @ProjetoTarefaStatusID
        BEGIN
            DECLARE @OldNmStatus VARCHAR(50), @NewNmStatus VARCHAR(50);
            SELECT @OldNmStatus = NmStatus FROM BR_ProjetoTarefaStatus WITH (NOLOCK) WHERE ProjetoTarefaStatusID = @OldTarefaStatusID;
            SELECT @NewNmStatus = NmStatus FROM BR_ProjetoTarefaStatus WITH (NOLOCK) WHERE ProjetoTarefaStatusID = @ProjetoTarefaStatusID;

            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioID,
                'Alterou status da tarefa "' + @OldNmTarefa + '" de "' + ISNULL(@OldNmStatus,'') + '" para "' + ISNULL(@NewNmStatus,'') + '"',
                @Agora);
        END

        IF @OldNmTarefa <> @NmTarefa
        BEGIN
            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioID,
                'Renomeou a tarefa "' + @OldNmTarefa + '" para "' + @NmTarefa + '"',
                @Agora);
        END

        IF @OldTarefaPrioridadeID <> @ProjetoTarefaPrioridadeID
        BEGIN
            DECLARE @NewNmPrioridade VARCHAR(50);
            SELECT @NewNmPrioridade = NmPrioridade FROM BR_ProjetoTarefaPrioridade WITH (NOLOCK) WHERE ProjetoTarefaPrioridadeID = @ProjetoTarefaPrioridadeID;

            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioID,
                'Alterou prioridade da tarefa "' + @NmTarefa + '" para "' + ISNULL(@NewNmPrioridade,'') + '"',
                @Agora);
        END

        -- Se nenhuma mudança chave, registrar genérico
        IF  @OldNmTarefa = @NmTarefa
            AND @OldTarefaStatusID = @ProjetoTarefaStatusID
            AND @OldTarefaPrioridadeID = @ProjetoTarefaPrioridadeID
        BEGIN
            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioID, 'Atualizou a tarefa "' + @NmTarefa + '"', @Agora);
        END

        -- 3. Atualizar DtUltimaAtualizacao do projeto
        UPDATE BR_Projeto SET DtUltimaAtualizacao = @Agora WHERE ProjetoID = @ProjetoID;

        COMMIT TRANSACTION;

        SELECT @ProjetoTarefaID AS ProjetoTarefaID;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SIC_ProjetoTarefaAtualizar criada com sucesso.';
GO

-- ************************************************************
-- 3. SIC_ProjetoTarefaExcluir
--    Exclusão lógica (FlagAtivo = 0). Também desativa
--    subtarefas filhas. Registra no histórico.
-- ************************************************************
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetoTarefaExcluir') AND type = N'P')
    DROP PROCEDURE SIC_ProjetoTarefaExcluir;
GO

CREATE PROCEDURE SIC_ProjetoTarefaExcluir
    @ProjetoTarefaID    INT,
    @UsuarioID          INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Agora DATETIME = GETDATE();
    DECLARE @ProjetoID INT;
    DECLARE @NmTarefa VARCHAR(300);

    SELECT
        @ProjetoID = T.ProjetoID,
        @NmTarefa  = T.NmTarefa
    FROM BR_ProjetoTarefa T WITH (NOLOCK)
    WHERE T.ProjetoTarefaID = @ProjetoTarefaID
      AND T.FlagAtivo = 1;

    IF @ProjetoID IS NULL
    BEGIN
        RAISERROR('Tarefa não encontrada ou já excluída.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Desativar a tarefa
        UPDATE BR_ProjetoTarefa
        SET FlagAtivo = 0, DtUltimaAtualizacao = @Agora
        WHERE ProjetoTarefaID = @ProjetoTarefaID;

        -- 2. Desativar subtarefas filhas
        UPDATE BR_ProjetoTarefa
        SET FlagAtivo = 0, DtUltimaAtualizacao = @Agora
        WHERE ProjetoTarefaPaiID = @ProjetoTarefaID
          AND FlagAtivo = 1;

        -- 3. Registrar no histórico
        INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
        VALUES (@ProjetoID, @UsuarioID, 'Excluiu a tarefa "' + @NmTarefa + '"', @Agora);

        -- 4. Atualizar DtUltimaAtualizacao do projeto
        UPDATE BR_Projeto SET DtUltimaAtualizacao = @Agora WHERE ProjetoID = @ProjetoID;

        COMMIT TRANSACTION;

        SELECT @ProjetoTarefaID AS ProjetoTarefaID;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SIC_ProjetoTarefaExcluir criada com sucesso.';
GO

PRINT '============================================================';
PRINT 'BR_Projeto_06_SP_TarefaEscrita.sql executado com sucesso.';
PRINT '============================================================';
GO
