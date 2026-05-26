-- ============================================================
-- Módulo Projetos — Stored Procedures de Escrita (Participante)
-- Arquivo: BR_Projeto_08_SP_ParticipanteEscrita.sql
-- Descrição: SPs de adição, atualização de papel e remoção
--            de participantes de um projeto.
-- Pré-requisito: BR_Projeto_01 + BR_Projeto_02
-- ============================================================

-- ************************************************************
-- 1. SIC_ProjetoParticipanteAdicionar
--    Adiciona um participante ao projeto. Se já existia com
--    FlagAtivo = 0, reativa. Registra no histórico.
-- ************************************************************
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetoParticipanteAdicionar') AND type = N'P')
    DROP PROCEDURE SIC_ProjetoParticipanteAdicionar;
GO

CREATE PROCEDURE SIC_ProjetoParticipanteAdicionar
    @ProjetoID      INT,
    @UsuarioID      INT,
    @NmPapel        VARCHAR(100) = '',
    @UsuarioLogadoID INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Agora DATETIME = GETDATE();
    DECLARE @ParticipanteID INT;

    -- Verificar se o projeto existe e está ativo
    IF NOT EXISTS (SELECT 1 FROM BR_Projeto WITH (NOLOCK) WHERE ProjetoID = @ProjetoID AND FlagAtivo = 1)
    BEGIN
        RAISERROR('Projeto não encontrado ou inativo.', 16, 1);
        RETURN;
    END

    -- Verificar se já é participante ativo
    IF EXISTS (
        SELECT 1 FROM BR_ProjetoParticipante WITH (NOLOCK)
        WHERE ProjetoID = @ProjetoID AND UsuarioID = @UsuarioID AND FlagAtivo = 1
    )
    BEGIN
        RAISERROR('Este usuário já é participante do projeto.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Verificar se existe registro inativo (reativação)
        SELECT @ParticipanteID = ProjetoParticipanteID
        FROM BR_ProjetoParticipante
        WHERE ProjetoID = @ProjetoID AND UsuarioID = @UsuarioID AND FlagAtivo = 0;

        IF @ParticipanteID IS NOT NULL
        BEGIN
            UPDATE BR_ProjetoParticipante
            SET FlagAtivo = 1,
                NmPapel = @NmPapel,
                DtEntrada = @Agora
            WHERE ProjetoParticipanteID = @ParticipanteID;
        END
        ELSE
        BEGIN
            INSERT INTO BR_ProjetoParticipante (ProjetoID, UsuarioID, NmPapel, DtEntrada)
            VALUES (@ProjetoID, @UsuarioID, @NmPapel, @Agora);

            SET @ParticipanteID = SCOPE_IDENTITY();
        END

        -- Buscar nome do usuário para o histórico
        DECLARE @NmUsuario VARCHAR(200);
        SELECT @NmUsuario = ISNULL(U.NmUsuario, 'Usuário')
        FROM BrSupply.dbo.BR_Usuario U WITH (NOLOCK)
        WHERE U.UsuarioID = @UsuarioID;

        INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
        VALUES (@ProjetoID, @UsuarioLogadoID, 'Adicionou o participante ' + @NmUsuario, @Agora);

        COMMIT TRANSACTION;

        SELECT @ParticipanteID AS ProjetoParticipanteID;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SIC_ProjetoParticipanteAdicionar criada com sucesso.';
GO

-- ************************************************************
-- 2. SIC_ProjetoParticipanteAtualizarPapel
--    Atualiza o papel (NmPapel) de um participante.
-- ************************************************************
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetoParticipanteAtualizarPapel') AND type = N'P')
    DROP PROCEDURE SIC_ProjetoParticipanteAtualizarPapel;
GO

CREATE PROCEDURE SIC_ProjetoParticipanteAtualizarPapel
    @ProjetoParticipanteID  INT,
    @NmPapel                VARCHAR(100),
    @UsuarioLogadoID        INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Agora DATETIME = GETDATE();
    DECLARE @ProjetoID INT;
    DECLARE @UsuarioID INT;
    DECLARE @OldNmPapel VARCHAR(100);

    -- Buscar dados atuais
    SELECT @ProjetoID = PP.ProjetoID,
           @UsuarioID = PP.UsuarioID,
           @OldNmPapel = PP.NmPapel
    FROM BR_ProjetoParticipante PP WITH (NOLOCK)
    WHERE PP.ProjetoParticipanteID = @ProjetoParticipanteID
      AND PP.FlagAtivo = 1;

    IF @ProjetoID IS NULL
    BEGIN
        RAISERROR('Participante não encontrado ou inativo.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE BR_ProjetoParticipante
        SET NmPapel = @NmPapel
        WHERE ProjetoParticipanteID = @ProjetoParticipanteID;

        -- Registrar no histórico se houve mudança
        IF @OldNmPapel <> @NmPapel
        BEGIN
            DECLARE @NmUsuario VARCHAR(200);
            SELECT @NmUsuario = ISNULL(U.NmUsuario, 'Usuário')
            FROM BrSupply.dbo.BR_Usuario U WITH (NOLOCK)
            WHERE U.UsuarioID = @UsuarioID;

            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioLogadoID,
                    'Alterou papel de ' + @NmUsuario + ': "' + ISNULL(@OldNmPapel, '') + '" → "' + @NmPapel + '"',
                    @Agora);
        END

        COMMIT TRANSACTION;

        SELECT @ProjetoParticipanteID AS ProjetoParticipanteID;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SIC_ProjetoParticipanteAtualizarPapel criada com sucesso.';
GO

-- ************************************************************
-- 3. SIC_ProjetoParticipanteRemover
--    Remove (soft-delete) um participante do projeto.
--    Não permite remover o criador do projeto.
-- ************************************************************
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetoParticipanteRemover') AND type = N'P')
    DROP PROCEDURE SIC_ProjetoParticipanteRemover;
GO

CREATE PROCEDURE SIC_ProjetoParticipanteRemover
    @ProjetoParticipanteID  INT,
    @UsuarioLogadoID        INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Agora DATETIME = GETDATE();
    DECLARE @ProjetoID INT;
    DECLARE @UsuarioID INT;

    -- Buscar dados do participante
    SELECT @ProjetoID = PP.ProjetoID,
           @UsuarioID = PP.UsuarioID
    FROM BR_ProjetoParticipante PP WITH (NOLOCK)
    WHERE PP.ProjetoParticipanteID = @ProjetoParticipanteID
      AND PP.FlagAtivo = 1;

    IF @ProjetoID IS NULL
    BEGIN
        RAISERROR('Participante não encontrado ou já removido.', 16, 1);
        RETURN;
    END

    -- Não permitir remoção do criador do projeto
    DECLARE @UsuarioCriadorID INT;
    SELECT @UsuarioCriadorID = P.UsuarioCriadorID
    FROM BR_Projeto P WITH (NOLOCK)
    WHERE P.ProjetoID = @ProjetoID;

    IF @UsuarioID = @UsuarioCriadorID
    BEGIN
        RAISERROR('Não é possível remover o criador do projeto.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE BR_ProjetoParticipante
        SET FlagAtivo = 0
        WHERE ProjetoParticipanteID = @ProjetoParticipanteID;

        DECLARE @NmUsuario VARCHAR(200);
        SELECT @NmUsuario = ISNULL(U.NmUsuario, 'Usuário')
        FROM BrSupply.dbo.BR_Usuario U WITH (NOLOCK)
        WHERE U.UsuarioID = @UsuarioID;

        INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
        VALUES (@ProjetoID, @UsuarioLogadoID, 'Removeu o participante ' + @NmUsuario, @Agora);

        COMMIT TRANSACTION;

        SELECT @ProjetoParticipanteID AS ProjetoParticipanteID;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SIC_ProjetoParticipanteRemover criada com sucesso.';
GO

PRINT '============================================================';
PRINT 'BR_Projeto_08_SP_ParticipanteEscrita.sql executado com sucesso.';
PRINT '============================================================';
GO
