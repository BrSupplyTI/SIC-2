-- ============================================================
-- Módulo Projetos — Stored Procedure de Verificação de Participante
-- Arquivo: BR_Projeto_09_SP_VerificarParticipante.sql
-- Descrição: SP para verificar se um usuário é participante
--            ativo de um projeto.
-- Pré-requisito: BR_Projeto_01 + BR_Projeto_02
-- ============================================================

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetoVerificarParticipante') AND type = N'P')
    DROP PROCEDURE SIC_ProjetoVerificarParticipante;
GO

CREATE PROCEDURE SIC_ProjetoVerificarParticipante
    @ProjetoID  INT,
    @UsuarioID  INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM BR_ProjetoParticipante WITH (NOLOCK)
        WHERE ProjetoID = @ProjetoID
          AND UsuarioID = @UsuarioID
          AND FlagAtivo = 1
    )
        SELECT CAST(1 AS BIT) AS EhParticipante;
    ELSE
        SELECT CAST(0 AS BIT) AS EhParticipante;
END
GO

PRINT 'Stored Procedure SIC_ProjetoVerificarParticipante criada com sucesso.';
GO
