-- ============================================================
-- Módulo Projetos — Stored Procedures de Leitura (Lista)
-- Arquivo: BR_Projeto_03_SP_Listar.sql
-- Descrição: SP de listagem de projetos com filtros,
--            paginação e contadores de tarefas/participantes.
-- Pré-requisito: BR_Projeto_01 + BR_Projeto_02
-- ============================================================

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'SIC_ProjetosListar') AND type = N'P')
    DROP PROCEDURE SIC_ProjetosListar;
GO

CREATE PROCEDURE SIC_ProjetosListar
    @PageNumber         INT = 1,
    @PageSize           INT = 12,
    @Texto              VARCHAR(200) = '',
    @ProjetoStatusID    INT = 0,
    @OrderBy            VARCHAR(50) = 'Recentes'
AS
BEGIN
    SET NOCOUNT ON;

    -- --------------------------------------------------------
    -- CTE principal: monta os dados do projeto + agregações
    -- --------------------------------------------------------
    ;WITH CTE_Projetos AS
    (
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
                      WHERE TS.NmStatus = 'Concluída'
                        AND TS.FlagAtivo = 1
                  )
            ), 0) AS QtTarefasConcluidas,

            -- Contador de participantes ativos
            ISNULL((
                SELECT COUNT(*)
                FROM BR_ProjetoParticipante PP WITH (NOLOCK)
                WHERE PP.ProjetoID = P.ProjetoID
                  AND PP.FlagAtivo = 1
            ), 0) AS QtParticipantes

        FROM BR_Projeto P WITH (NOLOCK)
        INNER JOIN BR_ProjetoStatus S WITH (NOLOCK) ON S.ProjetoStatusID = P.ProjetoStatusID
        -- JOIN cross-database para BR_Usuario (BrSupply) — FK lógica
        LEFT JOIN BrSupply.dbo.BR_Usuario U WITH (NOLOCK) ON U.UsuarioID = P.UsuarioCriadorID
        WHERE P.FlagAtivo = 1
          AND (@Texto = '' OR P.NmProjeto LIKE '%' + @Texto + '%')
          AND (@ProjetoStatusID = 0 OR P.ProjetoStatusID = @ProjetoStatusID)
    )

    -- --------------------------------------------------------
    -- Resultado paginado
    -- --------------------------------------------------------
    SELECT
        C.*,
        COUNT(*) OVER() AS TotalRegistros
    FROM CTE_Projetos C
    ORDER BY
        CASE WHEN @OrderBy = 'Nome (A-Z)' THEN C.NmProjeto END ASC,
        CASE WHEN @OrderBy = 'Nome (Z-A)' THEN C.NmProjeto END DESC,
        C.DtCriacao DESC  -- 'Recentes' (default) e fallback
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

PRINT 'Stored Procedure SIC_ProjetosListar criada com sucesso.';
GO
