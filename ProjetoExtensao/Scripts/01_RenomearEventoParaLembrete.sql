/*
    NaoMeEsquece - renomeia a terminologia "Evento" para "Lembrete".

    Renomeia tabelas, colunas e constraints, preservando os dados existentes.
    O script e idempotente: pode ser executado mais de uma vez sem erro, pois
    cada passo so age se o objeto de origem ainda existir.

    Como executar:
        sqlcmd -S localhost\SQLEXPRESS -d NaoMeEsquece -E -i 01_RenomearEventoParaLembrete.sql
    ou abra no SSMS com o banco NaoMeEsquece selecionado e execute.
*/

USE NaoMeEsquece;
GO

SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    /* ---------- 1. Colunas ---------- */
    /* Renomeadas antes das tabelas: sp_rename usa o nome atual da tabela. */

    IF COL_LENGTH('dbo.Evento', 'DataHoraEvento') IS NOT NULL
    BEGIN
        EXEC sp_rename 'dbo.Evento.DataHoraEvento', 'DataHoraLembrete', 'COLUMN';
        PRINT 'Coluna Evento.DataHoraEvento -> DataHoraLembrete';
    END

    IF COL_LENGTH('dbo.Evento', 'IdTipoEvento') IS NOT NULL
    BEGIN
        EXEC sp_rename 'dbo.Evento.IdTipoEvento', 'IdTipoLembrete', 'COLUMN';
        PRINT 'Coluna Evento.IdTipoEvento -> IdTipoLembrete';
    END

    /* ---------- 2. Constraints ---------- */
    /* Os nomes gerados automaticamente (PK__/DF__) variam por instalacao,
       entao sao localizados dinamicamente. */

    DECLARE @nome SYSNAME, @novo SYSNAME, @tabela SYSNAME;

    DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
        SELECT o.name, OBJECT_NAME(o.parent_object_id)
        FROM sys.objects o
        WHERE o.type IN ('PK', 'F', 'UQ', 'D', 'C')
          AND OBJECT_NAME(o.parent_object_id) IN ('Evento', 'TipoEvento')
          AND o.name LIKE '%Even%';

    OPEN cur;
    FETCH NEXT FROM cur INTO @nome, @tabela;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        /* As duas primeiras trocas cobrem os nomes escolhidos manualmente
           (FK_Evento_TipoEvento). As duas ultimas cobrem os nomes gerados pelo
           SQL Server, que vem truncados (PK__TipoEven__3214EC07...). */
        SET @novo = REPLACE(@nome,  'TipoEvento', 'TipoLembrete');
        SET @novo = REPLACE(@novo,  'Evento',     'Lembrete');
        SET @novo = REPLACE(@novo,  'TipoEven',   'TipoLembr');
        SET @novo = REPLACE(@novo,  'Even',       'Lembr');

        IF @novo <> @nome AND OBJECT_ID(@novo) IS NULL
        BEGIN
            EXEC sp_rename @nome, @novo, 'OBJECT';
            PRINT 'Constraint ' + @nome + ' -> ' + @novo;
        END

        FETCH NEXT FROM cur INTO @nome, @tabela;
    END

    CLOSE cur;
    DEALLOCATE cur;

    /* ---------- 3. Tabelas ---------- */
    /* TipoEvento antes de Evento: assim o REPLACE acima nao colide. */

    IF OBJECT_ID('dbo.TipoEvento', 'U') IS NOT NULL
       AND OBJECT_ID('dbo.TipoLembrete', 'U') IS NULL
    BEGIN
        EXEC sp_rename 'dbo.TipoEvento', 'TipoLembrete';
        PRINT 'Tabela TipoEvento -> TipoLembrete';
    END

    IF OBJECT_ID('dbo.Evento', 'U') IS NOT NULL
       AND OBJECT_ID('dbo.Lembrete', 'U') IS NULL
    BEGIN
        EXEC sp_rename 'dbo.Evento', 'Lembrete';
        PRINT 'Tabela Evento -> Lembrete';
    END

    COMMIT TRANSACTION;
    PRINT 'Renomeacao concluida com sucesso.';

END TRY
BEGIN CATCH

    IF CURSOR_STATUS('local', 'cur') >= 0
    BEGIN
        CLOSE cur;
        DEALLOCATE cur;
    END

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT 'Falha na renomeacao. Nenhuma alteracao foi aplicada.';
    THROW;

END CATCH
GO

/* ---------- Conferencia ---------- */
SELECT t.name AS tabela, c.name AS coluna
FROM sys.tables t
JOIN sys.columns c ON c.object_id = t.object_id
WHERE t.name IN ('Lembrete', 'TipoLembrete')
ORDER BY t.name, c.column_id;
GO
