-- ============================================================
-- Módulo Projetos — Stored Procedures de Escrita (Projeto)
-- Arquivo: BR_Projeto_05_SP_ProjetoEscrita.sql
-- Descrição: SPs de criação e atualização de projetos.
--            Ambas registram ações no BR_ProjetoHistorico.
-- Pré-requisito: BR_Projeto_01 + BR_Projeto_02
-- ============================================================

-- ************************************************************
-- 1. SIC_ProjetoCriar
--    Cria um novo projeto, adiciona o criador como participante
--    com papel "Gerente de Projeto" e registra no histórico.
--    Retorna o ProjetoID criado.
-- ************************************************************
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetoCriar') AND type = N'P')
    DROP PROCEDURE SIC_ProjetoCriar;
GO

CREATE PROCEDURE SIC_ProjetoCriar
    @NmProjeto          VARCHAR(200),
    @DsProjeto          VARCHAR(2000)   = '',
    @ProjetoStatusID    INT             = 1,
    @DtInicio           DATE            = NULL,
    @DtPrevisaoFim      DATE            = NULL,
    @UsuarioCriadorID   INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ProjetoID INT;
    DECLARE @Agora DATETIME = GETDATE();

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Inserir o projeto
        INSERT INTO BR_Projeto (NmProjeto, DsProjeto, ProjetoStatusID, DtInicio, DtPrevisaoFim, UsuarioCriadorID, DtCriacao)
        VALUES (@NmProjeto, @DsProjeto, @ProjetoStatusID, @DtInicio, @DtPrevisaoFim, @UsuarioCriadorID, @Agora);

        SET @ProjetoID = SCOPE_IDENTITY();

        -- 2. Adicionar o criador como primeiro participante
        INSERT INTO BR_ProjetoParticipante (ProjetoID, UsuarioID, NmPapel, DtEntrada)
        VALUES (@ProjetoID, @UsuarioCriadorID, 'Gerente de Projeto', @Agora);

        -- 3. Registrar no histórico
        INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
        VALUES (@ProjetoID, @UsuarioCriadorID, 'Criou o projeto', @Agora);

        COMMIT TRANSACTION;

        -- 4. Retornar o ID criado
        SELECT @ProjetoID AS ProjetoID;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SIC_ProjetoCriar criada com sucesso.';
GO

-- ************************************************************
-- 2. SIC_ProjetoAtualizar
--    Atualiza dados de um projeto existente e registra as
--    alterações no histórico (descreve o que mudou).
-- ************************************************************
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetoAtualizar') AND type = N'P')
    DROP PROCEDURE SIC_ProjetoAtualizar;
GO

CREATE PROCEDURE SIC_ProjetoAtualizar
    @ProjetoID          INT,
    @NmProjeto          VARCHAR(200),
    @DsProjeto          VARCHAR(2000),
    @ProjetoStatusID    INT,
    @DtInicio           DATE            = NULL,
    @DtPrevisaoFim      DATE            = NULL,
    @DtFimReal          DATE            = NULL,
    @UsuarioID          INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Agora DATETIME = GETDATE();

    -- Capturar valores atuais para detectar mudanças
    DECLARE @OldNmProjeto       VARCHAR(200);
    DECLARE @OldProjetoStatusID INT;
    DECLARE @OldDtInicio        DATE;
    DECLARE @OldDtPrevisaoFim   DATE;
    DECLARE @OldDtFimReal       DATE;

    SELECT
        @OldNmProjeto       = P.NmProjeto,
        @OldProjetoStatusID = P.ProjetoStatusID,
        @OldDtInicio        = P.DtInicio,
        @OldDtPrevisaoFim   = P.DtPrevisaoFim,
        @OldDtFimReal       = P.DtFimReal
    FROM BR_Projeto P WITH (NOLOCK)
    WHERE P.ProjetoID = @ProjetoID
      AND P.FlagAtivo = 1;

    IF @OldNmProjeto IS NULL
    BEGIN
        RAISERROR('Projeto não encontrado ou inativo.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Atualizar o projeto
        UPDATE BR_Projeto
        SET NmProjeto           = @NmProjeto,
            DsProjeto           = @DsProjeto,
            ProjetoStatusID     = @ProjetoStatusID,
            DtInicio            = @DtInicio,
            DtPrevisaoFim       = @DtPrevisaoFim,
            DtFimReal           = @DtFimReal,
            DtUltimaAtualizacao = @Agora
        WHERE ProjetoID = @ProjetoID;

        -- 2. Registrar mudanças no histórico
        IF @OldNmProjeto <> @NmProjeto
        BEGIN
            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioID, 'Alterou o nome do projeto de "' + @OldNmProjeto + '" para "' + @NmProjeto + '"', @Agora);
        END

        IF @OldProjetoStatusID <> @ProjetoStatusID
        BEGIN
            DECLARE @OldNmStatus VARCHAR(50), @NewNmStatus VARCHAR(50);

            SELECT @OldNmStatus = NmStatus FROM BR_ProjetoStatus WITH (NOLOCK) WHERE ProjetoStatusID = @OldProjetoStatusID;
            SELECT @NewNmStatus = NmStatus FROM BR_ProjetoStatus WITH (NOLOCK) WHERE ProjetoStatusID = @ProjetoStatusID;

            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioID, 'Alterou status de "' + ISNULL(@OldNmStatus,'') + '" para "' + ISNULL(@NewNmStatus,'') + '"', @Agora);
        END

        IF ISNULL(CONVERT(VARCHAR(10), @OldDtInicio, 120), '') <> ISNULL(CONVERT(VARCHAR(10), @DtInicio, 120), '')
        BEGIN
            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioID, 'Alterou a data de início para ' + ISNULL(CONVERT(VARCHAR(10), @DtInicio, 103), 'não definida'), @Agora);
        END

        IF ISNULL(CONVERT(VARCHAR(10), @OldDtPrevisaoFim, 120), '') <> ISNULL(CONVERT(VARCHAR(10), @DtPrevisaoFim, 120), '')
        BEGIN
            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioID, 'Alterou a previsão de término para ' + ISNULL(CONVERT(VARCHAR(10), @DtPrevisaoFim, 103), 'não definida'), @Agora);
        END

        IF ISNULL(CONVERT(VARCHAR(10), @OldDtFimReal, 120), '') <> ISNULL(CONVERT(VARCHAR(10), @DtFimReal, 120), '')
        BEGIN
            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioID, 'Alterou a data de conclusão para ' + ISNULL(CONVERT(VARCHAR(10), @DtFimReal, 103), 'não definida'), @Agora);
        END

        -- Se nenhuma mudança específica detectada, registrar atualização genérica
        IF  @OldNmProjeto = @NmProjeto
            AND @OldProjetoStatusID = @ProjetoStatusID
            AND ISNULL(CONVERT(VARCHAR(10), @OldDtInicio, 120), '') = ISNULL(CONVERT(VARCHAR(10), @DtInicio, 120), '')
            AND ISNULL(CONVERT(VARCHAR(10), @OldDtPrevisaoFim, 120), '') = ISNULL(CONVERT(VARCHAR(10), @DtPrevisaoFim, 120), '')
            AND ISNULL(CONVERT(VARCHAR(10), @OldDtFimReal, 120), '') = ISNULL(CONVERT(VARCHAR(10), @DtFimReal, 120), '')
        BEGIN
            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioID, 'Atualizou informações do projeto', @Agora);
        END

        COMMIT TRANSACTION;

        SELECT @ProjetoID AS ProjetoID;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SIC_ProjetoAtualizar criada com sucesso.';
GO

PRINT '============================================================';
PRINT 'BR_Projeto_05_SP_ProjetoEscrita.sql executado com sucesso.';
PRINT '============================================================';
GO
