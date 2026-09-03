/*
    NaoMeEsquece - cada lembrete passa a pertencer a um usuario.

    A tabela Lembrete nao tinha nenhuma ligacao com Usuario: qualquer conta que
    entrasse no aplicativo enxergava os lembretes de todas as outras. Este
    script cria a coluna IdUsuario e a chave estrangeira que faltavam.

    A tabela TipoLembrete ja possui IdUsuario, mas o aplicativo nunca gravava
    valor nela; por isso as linhas existentes tambem precisam ser atribuidas.

    ATENCAO - dados que ja existem:
    Como nao ha registro de quem criou cada linha, os lembretes e tipos sem dono
    sao atribuidos ao usuario mais antigo do banco (o primeiro cadastrado), que
    em um banco de desenvolvimento e quase sempre quem os criou. Confira o
    resultado ao final; se a atribuicao nao corresponder, ajuste manualmente com
    UPDATE antes de usar o aplicativo.

    O script e idempotente: pode ser executado mais de uma vez sem erro.

    Como executar:
        sqlcmd -S localhost\SQLEXPRESS -d NaoMeEsquece -E -i 03_LembretePorUsuario.sql
    ou abra no SSMS com o banco NaoMeEsquece selecionado e execute.
*/

USE NaoMeEsquece;
GO

SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    IF OBJECT_ID('dbo.Lembrete', 'U') IS NULL
        THROW 50001, 'Tabela Lembrete nao encontrada. Execute antes o script 01.', 1;

    IF OBJECT_ID('dbo.Usuario', 'U') IS NULL
        THROW 50002, 'Tabela Usuario nao encontrada. Crie o banco antes de executar este script.', 1;

    /* ---------- 1. Coluna Lembrete.IdUsuario ---------- */
    /* Aceita nulo: o banco pode ter lembretes anteriores a esta mudanca. */

    IF COL_LENGTH('dbo.Lembrete', 'IdUsuario') IS NULL
    BEGIN
        ALTER TABLE dbo.Lembrete ADD IdUsuario INT NULL;
        PRINT 'Coluna Lembrete.IdUsuario criada.';
    END
    ELSE
        PRINT 'Coluna Lembrete.IdUsuario ja existe.';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'Falha ao criar a coluna. Nenhuma alteracao foi aplicada.';
    THROW;
END CATCH

IF @@TRANCOUNT > 0 COMMIT TRANSACTION;
GO

/* A chave estrangeira e o preenchimento ficam em um lote separado: o
   ALTER TABLE acima so passa a valer para o compilador no proximo GO. */

SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    /* ---------- 2. Chave estrangeira ---------- */

    IF OBJECT_ID('dbo.FK_Lembrete_Usuario', 'F') IS NULL
    BEGIN
        ALTER TABLE dbo.Lembrete
            ADD CONSTRAINT FK_Lembrete_Usuario
            FOREIGN KEY (IdUsuario) REFERENCES dbo.Usuario (Id);

        PRINT 'Chave estrangeira FK_Lembrete_Usuario criada.';
    END
    ELSE
        PRINT 'Chave estrangeira FK_Lembrete_Usuario ja existe.';

    /* ---------- 3. Indice de consulta ---------- */
    /* Toda tela do aplicativo filtra por usuario. */

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Lembrete_IdUsuario' AND object_id = OBJECT_ID('dbo.Lembrete'))
    BEGIN
        CREATE INDEX IX_Lembrete_IdUsuario ON dbo.Lembrete (IdUsuario);
        PRINT 'Indice IX_Lembrete_IdUsuario criado.';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TipoLembrete_IdUsuario' AND object_id = OBJECT_ID('dbo.TipoLembrete'))
    BEGIN
        CREATE INDEX IX_TipoLembrete_IdUsuario ON dbo.TipoLembrete (IdUsuario);
        PRINT 'Indice IX_TipoLembrete_IdUsuario criado.';
    END

    /* ---------- 4. Dados existentes ---------- */

    DECLARE @idUsuario INT = (SELECT TOP 1 Id FROM dbo.Usuario ORDER BY Id);

    IF @idUsuario IS NULL
    BEGIN
        PRINT 'Nenhum usuario cadastrado: nada a atribuir.';
    END
    ELSE
    BEGIN
        DECLARE @tipos INT, @lembretes INT;

        /* O tipo vem primeiro: o lembrete sem dono e atribuido pelo dono do
           seu tipo, quando ele existir. */
        UPDATE dbo.TipoLembrete SET IdUsuario = @idUsuario WHERE IdUsuario IS NULL;
        SET @tipos = @@ROWCOUNT;

        UPDATE L
           SET L.IdUsuario = ISNULL(T.IdUsuario, @idUsuario)
          FROM dbo.Lembrete L
          LEFT JOIN dbo.TipoLembrete T ON T.Id = L.IdTipoLembrete
         WHERE L.IdUsuario IS NULL;
        SET @lembretes = @@ROWCOUNT;

        PRINT 'Tipos de lembrete atribuidos: ' + CAST(@tipos AS VARCHAR(10));
        PRINT 'Lembretes atribuidos: ' + CAST(@lembretes AS VARCHAR(10));
        PRINT 'Atribuidos ao usuario de Id ' + CAST(@idUsuario AS VARCHAR(10)) + '.';
    END

    COMMIT TRANSACTION;
    PRINT 'Separacao por usuario concluida com sucesso.';

END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT 'Falha na separacao por usuario. Nenhuma alteracao foi aplicada.';
    THROW;

END CATCH
GO

/* ---------- Conferencia ---------- */
/* Confira se cada conta ficou com os registros que realmente sao dela. */

SELECT U.Id,
       U.Login,
       (SELECT COUNT(*) FROM dbo.TipoLembrete T WHERE T.IdUsuario = U.Id) AS tipos,
       (SELECT COUNT(*) FROM dbo.Lembrete L WHERE L.IdUsuario = U.Id) AS lembretes
FROM dbo.Usuario U
ORDER BY U.Id;

SELECT COUNT(*) AS lembretes_sem_dono FROM dbo.Lembrete WHERE IdUsuario IS NULL;
GO
