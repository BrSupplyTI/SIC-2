-- ============================================================
-- Módulo Projetos — Stored Procedure de Busca de Usuários
-- Arquivo: BR_Projeto_07_SP_UsuariosBuscar.sql
-- Descrição: SP para buscar usuários (autocomplete) ao
--            adicionar participantes a um projeto.
--            Consulta cross-database em BrSupply.dbo.BR_Usuario
--            e exclui usuários já participantes do projeto.
-- Pré-requisito: BR_Projeto_01 + BR_Projeto_02
-- ============================================================

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetoUsuariosBuscar') AND type = N'P')
    DROP PROCEDURE SIC_ProjetoUsuariosBuscar;
GO

CREATE PROCEDURE SIC_ProjetoUsuariosBuscar
    @Texto      VARCHAR(200) = '',
    @ProjetoID  INT = 0
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 20
        U.UsuarioID,
        U.NmUsuario
    FROM BrSupply.dbo.BR_Usuario U WITH (NOLOCK)
    WHERE U.NmUsuario LIKE '%' + @Texto + '%'
      -- Exclui usuários já participantes ativos do projeto (quando informado)
      AND (@ProjetoID = 0 OR NOT EXISTS (
          SELECT 1
          FROM BR_ProjetoParticipante PP WITH (NOLOCK)
          WHERE PP.ProjetoID  = @ProjetoID
            AND PP.UsuarioID  = U.UsuarioID
            AND PP.FlagAtivo  = 1
      ))
    ORDER BY U.NmUsuario;
END
GO

PRINT 'Stored Procedure SIC_ProjetoUsuariosBuscar criada com sucesso.';
GO
