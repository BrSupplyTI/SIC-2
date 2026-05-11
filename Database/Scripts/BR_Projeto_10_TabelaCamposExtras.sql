-- ============================================================
-- Módulo Projetos — Tabela de Campos Extras (personalizados)
-- Arquivo: BR_Projeto_10_TabelaCamposExtras.sql
-- Descrição: Cria a tabela BR_ProjetoCampoExtra que permite ao
--            usuário definir até 4 campos personalizados por projeto
--            (par "Nome do campo" + "Valor").
-- Pré-requisito: BR_Projeto_02_TabelasPrincipais.sql
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BR_ProjetoCampoExtra')
BEGIN
    CREATE TABLE BR_ProjetoCampoExtra
    (
        ProjetoCampoExtraID INT IDENTITY(1,1)   NOT NULL,
        ProjetoID           INT                 NOT NULL,
        Ordem               TINYINT             NOT NULL,
        NmCampo             VARCHAR(60)         NOT NULL,
        VlCampo             VARCHAR(500)        NULL,
        DtCriacao           DATETIME            NOT NULL    CONSTRAINT DF_ProjetoCampoExtra_DtCriacao   DEFAULT GETDATE(),
        DtUltimaAtualizacao DATETIME            NULL,

        CONSTRAINT PK_ProjetoCampoExtra             PRIMARY KEY CLUSTERED (ProjetoCampoExtraID),
        CONSTRAINT FK_ProjetoCampoExtra_Projeto     FOREIGN KEY (ProjetoID) REFERENCES BR_Projeto (ProjetoID) ON DELETE CASCADE,
        CONSTRAINT CK_ProjetoCampoExtra_Ordem       CHECK (Ordem BETWEEN 1 AND 4)
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_ProjetoCampoExtra_Projeto_Ordem
        ON BR_ProjetoCampoExtra (ProjetoID, Ordem);

    PRINT 'Tabela BR_ProjetoCampoExtra criada com sucesso (+ 1 índice único).';
END
ELSE
BEGIN
    PRINT 'Tabela BR_ProjetoCampoExtra já existe — ignorando criação.';
END
GO

PRINT '============================================================';
PRINT 'BR_Projeto_10_TabelaCamposExtras.sql executado com sucesso.';
PRINT '============================================================';
GO
