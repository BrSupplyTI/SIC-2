-- ============================================================
-- Módulo Projetos — Tabelas de Lookup
-- Arquivo: BR_Projeto_01_TabelasLookup.sql
-- Descrição: Cria as tabelas de domínio (status, prioridade)
--            e insere os dados iniciais (seed).
-- ============================================================

-- ************************************************************
-- 1. BR_ProjetoStatus
-- ************************************************************
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BR_ProjetoStatus')
BEGIN
    CREATE TABLE BR_ProjetoStatus
    (
        ProjetoStatusID         INT IDENTITY(1,1)   NOT NULL,
        NmStatus                VARCHAR(50)         NOT NULL,
        CdCor                   VARCHAR(7)          NOT NULL,
        NrOrdem                 INT                 NOT NULL    CONSTRAINT DF_ProjetoStatus_NrOrdem    DEFAULT 0,
        FlagAtivo               BIT                 NOT NULL    CONSTRAINT DF_ProjetoStatus_FlagAtivo  DEFAULT 1,

        CONSTRAINT PK_ProjetoStatus PRIMARY KEY CLUSTERED (ProjetoStatusID)
    );

    PRINT 'Tabela BR_ProjetoStatus criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela BR_ProjetoStatus já existe — ignorando criação.';
END
GO

-- Seed BR_ProjetoStatus
IF NOT EXISTS (SELECT 1 FROM BR_ProjetoStatus)
BEGIN
    SET IDENTITY_INSERT BR_ProjetoStatus ON;

    INSERT INTO BR_ProjetoStatus (ProjetoStatusID, NmStatus, CdCor, NrOrdem, FlagAtivo)
    VALUES
        (1, 'Planejamento',  '#6c757d', 1, 1),
        (2, 'Em Andamento',  '#0d6efd', 2, 1),
        (3, 'Concluído',     '#198754', 3, 1),
        (4, 'Cancelado',     '#dc3545', 4, 1),
        (5, 'Pausado',       '#ffc107', 5, 1);

    SET IDENTITY_INSERT BR_ProjetoStatus OFF;

    PRINT 'Seed BR_ProjetoStatus inserido (5 registros).';
END
ELSE
BEGIN
    PRINT 'Seed BR_ProjetoStatus ignorado — tabela já contém dados.';
END
GO

-- ************************************************************
-- 2. BR_ProjetoTarefaStatus
-- ************************************************************
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BR_ProjetoTarefaStatus')
BEGIN
    CREATE TABLE BR_ProjetoTarefaStatus
    (
        ProjetoTarefaStatusID   INT IDENTITY(1,1)   NOT NULL,
        NmStatus                VARCHAR(50)         NOT NULL,
        CdCor                   VARCHAR(7)          NOT NULL,
        NrOrdem                 INT                 NOT NULL    CONSTRAINT DF_ProjetoTarefaStatus_NrOrdem    DEFAULT 0,
        FlagAtivo               BIT                 NOT NULL    CONSTRAINT DF_ProjetoTarefaStatus_FlagAtivo  DEFAULT 1,

        CONSTRAINT PK_ProjetoTarefaStatus PRIMARY KEY CLUSTERED (ProjetoTarefaStatusID)
    );

    PRINT 'Tabela BR_ProjetoTarefaStatus criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela BR_ProjetoTarefaStatus já existe — ignorando criação.';
END
GO

-- Seed BR_ProjetoTarefaStatus
IF NOT EXISTS (SELECT 1 FROM BR_ProjetoTarefaStatus)
BEGIN
    SET IDENTITY_INSERT BR_ProjetoTarefaStatus ON;

    INSERT INTO BR_ProjetoTarefaStatus (ProjetoTarefaStatusID, NmStatus, CdCor, NrOrdem, FlagAtivo)
    VALUES
        (1, 'A Fazer',       '#6c757d', 1, 1),
        (2, 'Em Progresso',  '#0d6efd', 2, 1),
        (3, 'Em Revisão',    '#ffc107', 3, 1),
        (4, 'Concluída',     '#198754', 4, 1);

    SET IDENTITY_INSERT BR_ProjetoTarefaStatus OFF;

    PRINT 'Seed BR_ProjetoTarefaStatus inserido (4 registros).';
END
ELSE
BEGIN
    PRINT 'Seed BR_ProjetoTarefaStatus ignorado — tabela já contém dados.';
END
GO

-- ************************************************************
-- 3. BR_ProjetoTarefaPrioridade
-- ************************************************************
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BR_ProjetoTarefaPrioridade')
BEGIN
    CREATE TABLE BR_ProjetoTarefaPrioridade
    (
        ProjetoTarefaPrioridadeID   INT IDENTITY(1,1)   NOT NULL,
        NmPrioridade                VARCHAR(50)         NOT NULL,
        CdCor                       VARCHAR(7)          NOT NULL,
        NrOrdem                     INT                 NOT NULL    CONSTRAINT DF_ProjetoTarefaPrioridade_NrOrdem    DEFAULT 0,
        FlagAtivo                   BIT                 NOT NULL    CONSTRAINT DF_ProjetoTarefaPrioridade_FlagAtivo  DEFAULT 1,

        CONSTRAINT PK_ProjetoTarefaPrioridade PRIMARY KEY CLUSTERED (ProjetoTarefaPrioridadeID)
    );

    PRINT 'Tabela BR_ProjetoTarefaPrioridade criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela BR_ProjetoTarefaPrioridade já existe — ignorando criação.';
END
GO

-- Seed BR_ProjetoTarefaPrioridade
IF NOT EXISTS (SELECT 1 FROM BR_ProjetoTarefaPrioridade)
BEGIN
    SET IDENTITY_INSERT BR_ProjetoTarefaPrioridade ON;

    INSERT INTO BR_ProjetoTarefaPrioridade (ProjetoTarefaPrioridadeID, NmPrioridade, CdCor, NrOrdem, FlagAtivo)
    VALUES
        (1, 'Baixa',    '#198754', 1, 1),
        (2, 'Média',    '#ffc107', 2, 1),
        (3, 'Alta',     '#fd7e14', 3, 1),
        (4, 'Crítica',  '#dc3545', 4, 1);

    SET IDENTITY_INSERT BR_ProjetoTarefaPrioridade OFF;

    PRINT 'Seed BR_ProjetoTarefaPrioridade inserido (4 registros).';
END
ELSE
BEGIN
    PRINT 'Seed BR_ProjetoTarefaPrioridade ignorado — tabela já contém dados.';
END
GO

PRINT '============================================================';
PRINT 'BR_Projeto_01_TabelasLookup.sql executado com sucesso.';
PRINT '============================================================';
GO
