-- ============================================================
-- Módulo Projetos — Tabelas Principais
-- Arquivo: BR_Projeto_02_TabelasPrincipais.sql
-- Descrição: Cria as tabelas transacionais do módulo Projetos
--            (projeto, participantes, tarefas, histórico).
-- Pré-requisito: BR_Projeto_01_TabelasLookup.sql
-- ============================================================

-- ************************************************************
-- 1. BR_Projeto
-- ************************************************************
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BR_Projeto')
BEGIN
    CREATE TABLE BR_Projeto
    (
        ProjetoID               INT IDENTITY(1,1)   NOT NULL,
        NmProjeto               VARCHAR(200)        NOT NULL,
        DsProjeto               VARCHAR(2000)       NOT NULL    CONSTRAINT DF_Projeto_DsProjeto        DEFAULT '',
        ProjetoStatusID         INT                 NOT NULL    CONSTRAINT DF_Projeto_ProjetoStatusID  DEFAULT 1,
        DtInicio                DATE                NULL,
        DtPrevisaoFim           DATE                NULL,
        DtFimReal               DATE                NULL,
        UsuarioCriadorID        INT                 NOT NULL,
        DtCriacao               DATETIME            NOT NULL    CONSTRAINT DF_Projeto_DtCriacao        DEFAULT GETDATE(),
        DtUltimaAtualizacao     DATETIME            NULL,
        FlagAtivo               BIT                 NOT NULL    CONSTRAINT DF_Projeto_FlagAtivo        DEFAULT 1,

        CONSTRAINT PK_Projeto           PRIMARY KEY CLUSTERED (ProjetoID),
        CONSTRAINT FK_Projeto_Status    FOREIGN KEY (ProjetoStatusID)   REFERENCES BR_ProjetoStatus (ProjetoStatusID)
        -- UsuarioCriadorID: FK lógica para BR_Usuario (banco BrSupply) — integridade via JOIN nas SPs
    );

    CREATE NONCLUSTERED INDEX IX_Projeto_StatusID
        ON BR_Projeto (ProjetoStatusID)
        INCLUDE (NmProjeto, DtCriacao, FlagAtivo);

    CREATE NONCLUSTERED INDEX IX_Projeto_Criador
        ON BR_Projeto (UsuarioCriadorID);

    CREATE NONCLUSTERED INDEX IX_Projeto_DtCriacao
        ON BR_Projeto (DtCriacao DESC)
        INCLUDE (ProjetoStatusID, FlagAtivo);

    PRINT 'Tabela BR_Projeto criada com sucesso (+ 3 índices).';
END
ELSE
BEGIN
    PRINT 'Tabela BR_Projeto já existe — ignorando criação.';
END
GO

-- ************************************************************
-- 2. BR_ProjetoParticipante
-- ************************************************************
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BR_ProjetoParticipante')
BEGIN
    CREATE TABLE BR_ProjetoParticipante
    (
        ProjetoParticipanteID   INT IDENTITY(1,1)   NOT NULL,
        ProjetoID               INT                 NOT NULL,
        UsuarioID               INT                 NOT NULL,
        NmPapel                 VARCHAR(100)        NOT NULL    CONSTRAINT DF_ProjetoParticipante_NmPapel   DEFAULT '',
        DtEntrada               DATETIME            NOT NULL    CONSTRAINT DF_ProjetoParticipante_DtEntrada DEFAULT GETDATE(),
        FlagAtivo               BIT                 NOT NULL    CONSTRAINT DF_ProjetoParticipante_FlagAtivo DEFAULT 1,

        CONSTRAINT PK_ProjetoParticipante           PRIMARY KEY CLUSTERED (ProjetoParticipanteID),
        CONSTRAINT FK_ProjetoParticipante_Projeto   FOREIGN KEY (ProjetoID)  REFERENCES BR_Projeto (ProjetoID),
        -- UsuarioID: FK lógica para BR_Usuario (banco BrSupply) — integridade via JOIN nas SPs
        CONSTRAINT UQ_ProjetoParticipante_Unico     UNIQUE (ProjetoID, UsuarioID)
    );

    CREATE NONCLUSTERED INDEX IX_ProjetoParticipante_ProjetoID
        ON BR_ProjetoParticipante (ProjetoID)
        INCLUDE (UsuarioID, NmPapel, FlagAtivo);

    PRINT 'Tabela BR_ProjetoParticipante criada com sucesso (+ 1 índice, 1 unique).';
END
ELSE
BEGIN
    PRINT 'Tabela BR_ProjetoParticipante já existe — ignorando criação.';
END
GO

-- ************************************************************
-- 3. BR_ProjetoTarefa
-- ************************************************************
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BR_ProjetoTarefa')
BEGIN
    CREATE TABLE BR_ProjetoTarefa
    (
        ProjetoTarefaID             INT IDENTITY(1,1)   NOT NULL,
        ProjetoID                   INT                 NOT NULL,
        NmTarefa                    VARCHAR(300)        NOT NULL,
        DsTarefa                    VARCHAR(2000)       NULL,
        ProjetoTarefaStatusID       INT                 NOT NULL    CONSTRAINT DF_ProjetoTarefa_StatusID       DEFAULT 1,
        ProjetoTarefaPrioridadeID   INT                 NOT NULL    CONSTRAINT DF_ProjetoTarefa_PrioridadeID   DEFAULT 2,
        UsuarioResponsavelID        INT                 NULL,
        DtInicio                    DATE                NULL,
        DtPrevisaoFim               DATE                NULL,
        DtFimReal                   DATE                NULL,
        NrOrdem                     INT                 NOT NULL    CONSTRAINT DF_ProjetoTarefa_NrOrdem        DEFAULT 0,
        ProjetoTarefaPaiID          INT                 NULL,
        DtCriacao                   DATETIME            NOT NULL    CONSTRAINT DF_ProjetoTarefa_DtCriacao      DEFAULT GETDATE(),
        DtUltimaAtualizacao         DATETIME            NULL,
        FlagAtivo                   BIT                 NOT NULL    CONSTRAINT DF_ProjetoTarefa_FlagAtivo      DEFAULT 1,

        CONSTRAINT PK_ProjetoTarefa                 PRIMARY KEY CLUSTERED (ProjetoTarefaID),
        CONSTRAINT FK_ProjetoTarefa_Projeto         FOREIGN KEY (ProjetoID)                 REFERENCES BR_Projeto (ProjetoID),
        CONSTRAINT FK_ProjetoTarefa_Status          FOREIGN KEY (ProjetoTarefaStatusID)     REFERENCES BR_ProjetoTarefaStatus (ProjetoTarefaStatusID),
        CONSTRAINT FK_ProjetoTarefa_Prioridade      FOREIGN KEY (ProjetoTarefaPrioridadeID) REFERENCES BR_ProjetoTarefaPrioridade (ProjetoTarefaPrioridadeID),
        -- UsuarioResponsavelID: FK lógica para BR_Usuario (banco BrSupply) — integridade via JOIN nas SPs
        CONSTRAINT FK_ProjetoTarefa_Pai             FOREIGN KEY (ProjetoTarefaPaiID)         REFERENCES BR_ProjetoTarefa (ProjetoTarefaID)
    );

    CREATE NONCLUSTERED INDEX IX_ProjetoTarefa_ProjetoID
        ON BR_ProjetoTarefa (ProjetoID, NrOrdem)
        INCLUDE (ProjetoTarefaStatusID, ProjetoTarefaPrioridadeID, ProjetoTarefaPaiID, FlagAtivo);

    CREATE NONCLUSTERED INDEX IX_ProjetoTarefa_PaiID
        ON BR_ProjetoTarefa (ProjetoTarefaPaiID)
        WHERE ProjetoTarefaPaiID IS NOT NULL;

    CREATE NONCLUSTERED INDEX IX_ProjetoTarefa_Responsavel
        ON BR_ProjetoTarefa (UsuarioResponsavelID)
        WHERE UsuarioResponsavelID IS NOT NULL;

    PRINT 'Tabela BR_ProjetoTarefa criada com sucesso (+ 3 índices).';
END
ELSE
BEGIN
    PRINT 'Tabela BR_ProjetoTarefa já existe — ignorando criação.';
END
GO

-- ************************************************************
-- 4. BR_ProjetoHistorico
-- ************************************************************
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BR_ProjetoHistorico')
BEGIN
    CREATE TABLE BR_ProjetoHistorico
    (
        ProjetoHistoricoID      INT IDENTITY(1,1)   NOT NULL,
        ProjetoID               INT                 NOT NULL,
        UsuarioID               INT                 NOT NULL,
        DsAcao                  VARCHAR(500)        NOT NULL,
        DtAcao                  DATETIME            NOT NULL    CONSTRAINT DF_ProjetoHistorico_DtAcao DEFAULT GETDATE(),

        CONSTRAINT PK_ProjetoHistorico              PRIMARY KEY CLUSTERED (ProjetoHistoricoID),
        CONSTRAINT FK_ProjetoHistorico_Projeto      FOREIGN KEY (ProjetoID) REFERENCES BR_Projeto (ProjetoID)
        -- UsuarioID: FK lógica para BR_Usuario (banco BrSupply) — integridade via JOIN nas SPs
    );

    CREATE NONCLUSTERED INDEX IX_ProjetoHistorico_ProjetoID
        ON BR_ProjetoHistorico (ProjetoID, DtAcao DESC)
        INCLUDE (UsuarioID, DsAcao);

    PRINT 'Tabela BR_ProjetoHistorico criada com sucesso (+ 1 índice).';
END
ELSE
BEGIN
    PRINT 'Tabela BR_ProjetoHistorico já existe — ignorando criação.';
END
GO

PRINT '============================================================';
PRINT 'BR_Projeto_02_TabelasPrincipais.sql executado com sucesso.';
PRINT '============================================================';
GO
