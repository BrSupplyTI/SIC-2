-- ============================================================
-- Módulo Projetos — SPs de Campos Extras
-- Arquivo: BR_Projeto_11_SP_CamposExtras.sql
-- Descrição: Stored Procedures para listar e salvar (merge) os
--            campos extras (personalizados) de um projeto, além de
--            reescrever SIC_ProjetoCriar e SIC_ProjetoAtualizar para
--            aceitar os campos extras via JSON e registrar histórico.
-- Pré-requisito: BR_Projeto_10_TabelaCamposExtras.sql
--                BR_Projeto_05_SP_ProjetoEscrita.sql
-- ============================================================

-- ************************************************************
-- 1. SIC_ProjetoCamposExtrasListar
--    Retorna os campos extras de um projeto em ordem (1..4).
-- ************************************************************
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetoCamposExtrasListar') AND type = N'P')
    DROP PROCEDURE SIC_ProjetoCamposExtrasListar;
GO

CREATE PROCEDURE SIC_ProjetoCamposExtrasListar
    @ProjetoID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ProjetoCampoExtraID,
        ProjetoID,
        Ordem,
        NmCampo,
        VlCampo
    FROM BR_ProjetoCampoExtra WITH (NOLOCK)
    WHERE ProjetoID = @ProjetoID
    ORDER BY Ordem;
END
GO

PRINT 'Stored Procedure SIC_ProjetoCamposExtrasListar criada com sucesso.';
GO

-- ************************************************************
-- 2. SIC_ProjetoCamposExtrasSalvar
--    Faz MERGE dos campos extras do projeto.
--    Recebe um JSON [{ "Ordem": 1, "NmCampo": "...", "VlCampo": "..." }, ...]
--    com até 4 itens. Linhas com NmCampo vazio são removidas.
--    Quando @UsuarioID for informado, registra mudanças no histórico.
-- ************************************************************
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetoCamposExtrasSalvar') AND type = N'P')
    DROP PROCEDURE SIC_ProjetoCamposExtrasSalvar;
GO

CREATE PROCEDURE SIC_ProjetoCamposExtrasSalvar
    @ProjetoID  INT,
    @CamposJson NVARCHAR(MAX) = NULL,
    @UsuarioID  INT           = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Agora DATETIME = GETDATE();

    -- Tabela temporária com a entrada normalizada (somente itens válidos)
    DECLARE @Entrada TABLE
    (
        Ordem   TINYINT     NOT NULL PRIMARY KEY,
        NmCampo VARCHAR(60) NOT NULL,
        VlCampo VARCHAR(500) NULL
    );

    IF @CamposJson IS NOT NULL AND LTRIM(RTRIM(@CamposJson)) <> ''
    BEGIN
        INSERT INTO @Entrada (Ordem, NmCampo, VlCampo)
        SELECT TOP (4)
            CAST(J.Ordem AS TINYINT)                    AS Ordem,
            LTRIM(RTRIM(J.NmCampo))                     AS NmCampo,
            NULLIF(LTRIM(RTRIM(ISNULL(J.VlCampo,''))),'') AS VlCampo
        FROM OPENJSON(@CamposJson)
             WITH (
                Ordem   INT          '$.Ordem',
                NmCampo NVARCHAR(60) '$.NmCampo',
                VlCampo NVARCHAR(500)'$.VlCampo'
             ) AS J
        WHERE J.Ordem BETWEEN 1 AND 4
          AND J.NmCampo IS NOT NULL
          AND LTRIM(RTRIM(J.NmCampo)) <> ''
        ORDER BY J.Ordem;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Snapshot do estado atual para diff de histórico
        DECLARE @AntigoEstado TABLE (Ordem TINYINT, NmCampo VARCHAR(60), VlCampo VARCHAR(500));
        INSERT INTO @AntigoEstado (Ordem, NmCampo, VlCampo)
        SELECT Ordem, NmCampo, VlCampo
        FROM BR_ProjetoCampoExtra WITH (UPDLOCK, HOLDLOCK)
        WHERE ProjetoID = @ProjetoID;

        -- 1. Atualiza existentes
        UPDATE C
           SET C.NmCampo             = E.NmCampo,
               C.VlCampo             = E.VlCampo,
               C.DtUltimaAtualizacao = @Agora
          FROM BR_ProjetoCampoExtra C
          INNER JOIN @Entrada E ON E.Ordem = C.Ordem
         WHERE C.ProjetoID = @ProjetoID;

        -- 2. Insere novos
        INSERT INTO BR_ProjetoCampoExtra (ProjetoID, Ordem, NmCampo, VlCampo, DtCriacao)
        SELECT @ProjetoID, E.Ordem, E.NmCampo, E.VlCampo, @Agora
          FROM @Entrada E
         WHERE NOT EXISTS (
               SELECT 1 FROM BR_ProjetoCampoExtra C
                WHERE C.ProjetoID = @ProjetoID AND C.Ordem = E.Ordem
         );

        -- 3. Remove os que sumiram
        DELETE C
          FROM BR_ProjetoCampoExtra C
         WHERE C.ProjetoID = @ProjetoID
           AND NOT EXISTS (SELECT 1 FROM @Entrada E WHERE E.Ordem = C.Ordem);

        -- 4. Histórico (apenas se @UsuarioID informado)
        IF @UsuarioID IS NOT NULL
        BEGIN
            -- Removidos
            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            SELECT @ProjetoID, @UsuarioID,
                   'Removeu o campo personalizado "' + A.NmCampo + '"',
                   @Agora
              FROM @AntigoEstado A
             WHERE NOT EXISTS (SELECT 1 FROM @Entrada E WHERE E.Ordem = A.Ordem);

            -- Adicionados
            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            SELECT @ProjetoID, @UsuarioID,
                   'Adicionou o campo personalizado "' + E.NmCampo + '"' +
                   CASE WHEN E.VlCampo IS NOT NULL
                        THEN ' com valor "' + E.VlCampo + '"'
                        ELSE '' END,
                   @Agora
              FROM @Entrada E
             WHERE NOT EXISTS (SELECT 1 FROM @AntigoEstado A WHERE A.Ordem = E.Ordem);

            -- Alterados (nome ou valor)
            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            SELECT @ProjetoID, @UsuarioID,
                   CASE
                     WHEN A.NmCampo <> E.NmCampo
                       THEN 'Renomeou o campo personalizado "' + A.NmCampo + '" para "' + E.NmCampo + '"'
                     ELSE 'Alterou o valor do campo "' + E.NmCampo + '" de "' +
                          ISNULL(A.VlCampo,'') + '" para "' + ISNULL(E.VlCampo,'') + '"'
                   END,
                   @Agora
              FROM @Entrada E
              INNER JOIN @AntigoEstado A ON A.Ordem = E.Ordem
             WHERE A.NmCampo <> E.NmCampo
                OR ISNULL(A.VlCampo,'') <> ISNULL(E.VlCampo,'');
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SIC_ProjetoCamposExtrasSalvar criada com sucesso.';
GO

-- ************************************************************
-- 3. SIC_ProjetoCriar (recriada para aceitar @CamposExtrasJson)
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
    @UsuarioCriadorID   INT,
    @CamposExtrasJson   NVARCHAR(MAX)   = NULL
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

        -- 4. Persistir campos extras (sem gerar histórico de "adicionou", já que é criação)
        IF @CamposExtrasJson IS NOT NULL
        BEGIN
            EXEC SIC_ProjetoCamposExtrasSalvar
                @ProjetoID  = @ProjetoID,
                @CamposJson = @CamposExtrasJson,
                @UsuarioID  = NULL;
        END

        COMMIT TRANSACTION;

        -- 5. Retornar o ID criado
        SELECT @ProjetoID AS ProjetoID;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

PRINT 'Stored Procedure SIC_ProjetoCriar recriada com sucesso.';
GO

-- ************************************************************
-- 4. SIC_ProjetoAtualizar (recriada para aceitar @CamposExtrasJson)
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
    @UsuarioID          INT,
    @CamposExtrasJson   NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Agora DATETIME = GETDATE();

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

        IF @ProjetoStatusID = 3 AND @DtFimReal IS NULL
            SET @DtFimReal = CAST(@Agora AS DATE);

        IF @OldProjetoStatusID = 3 AND @ProjetoStatusID <> 3
            SET @DtFimReal = NULL;

        UPDATE BR_Projeto
        SET NmProjeto           = @NmProjeto,
            DsProjeto           = @DsProjeto,
            ProjetoStatusID     = @ProjetoStatusID,
            DtInicio            = @DtInicio,
            DtPrevisaoFim       = @DtPrevisaoFim,
            DtFimReal           = @DtFimReal,
            DtUltimaAtualizacao = @Agora
        WHERE ProjetoID = @ProjetoID;

        IF @OldNmProjeto <> @NmProjeto
            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioID, 'Alterou o nome do projeto de "' + @OldNmProjeto + '" para "' + @NmProjeto + '"', @Agora);

        IF @OldProjetoStatusID <> @ProjetoStatusID
        BEGIN
            DECLARE @OldNmStatus VARCHAR(50), @NewNmStatus VARCHAR(50);
            SELECT @OldNmStatus = NmStatus FROM BR_ProjetoStatus WITH (NOLOCK) WHERE ProjetoStatusID = @OldProjetoStatusID;
            SELECT @NewNmStatus = NmStatus FROM BR_ProjetoStatus WITH (NOLOCK) WHERE ProjetoStatusID = @ProjetoStatusID;

            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioID, 'Alterou status de "' + ISNULL(@OldNmStatus,'') + '" para "' + ISNULL(@NewNmStatus,'') + '"', @Agora);
        END

        IF ISNULL(CONVERT(VARCHAR(10), @OldDtInicio, 120), '') <> ISNULL(CONVERT(VARCHAR(10), @DtInicio, 120), '')
            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioID, 'Alterou a data de início para ' + ISNULL(CONVERT(VARCHAR(10), @DtInicio, 103), 'não definida'), @Agora);

        IF ISNULL(CONVERT(VARCHAR(10), @OldDtPrevisaoFim, 120), '') <> ISNULL(CONVERT(VARCHAR(10), @DtPrevisaoFim, 120), '')
            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioID, 'Alterou a previsão de término para ' + ISNULL(CONVERT(VARCHAR(10), @DtPrevisaoFim, 103), 'não definida'), @Agora);

        IF ISNULL(CONVERT(VARCHAR(10), @OldDtFimReal, 120), '') <> ISNULL(CONVERT(VARCHAR(10), @DtFimReal, 120), '')
            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioID, 'Alterou a data de conclusão para ' + ISNULL(CONVERT(VARCHAR(10), @DtFimReal, 103), 'não definida'), @Agora);

        IF  @OldNmProjeto = @NmProjeto
            AND @OldProjetoStatusID = @ProjetoStatusID
            AND ISNULL(CONVERT(VARCHAR(10), @OldDtInicio, 120), '') = ISNULL(CONVERT(VARCHAR(10), @DtInicio, 120), '')
            AND ISNULL(CONVERT(VARCHAR(10), @OldDtPrevisaoFim, 120), '') = ISNULL(CONVERT(VARCHAR(10), @DtPrevisaoFim, 120), '')
            AND ISNULL(CONVERT(VARCHAR(10), @OldDtFimReal, 120), '') = ISNULL(CONVERT(VARCHAR(10), @DtFimReal, 120), '')
            AND @CamposExtrasJson IS NULL
        BEGIN
            INSERT INTO BR_ProjetoHistorico (ProjetoID, UsuarioID, DsAcao, DtAcao)
            VALUES (@ProjetoID, @UsuarioID, 'Atualizou informações do projeto', @Agora);
        END

        -- Persistir campos extras (gera histórico granular dentro da SP)
        IF @CamposExtrasJson IS NOT NULL
        BEGIN
            EXEC SIC_ProjetoCamposExtrasSalvar
                @ProjetoID  = @ProjetoID,
                @CamposJson = @CamposExtrasJson,
                @UsuarioID  = @UsuarioID;
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

PRINT 'Stored Procedure SIC_ProjetoAtualizar recriada com sucesso.';
GO

PRINT '============================================================';
PRINT 'BR_Projeto_11_SP_CamposExtras.sql executado com sucesso.';
PRINT '============================================================';
GO
