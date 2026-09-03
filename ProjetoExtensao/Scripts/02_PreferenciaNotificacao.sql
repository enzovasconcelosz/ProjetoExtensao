/*
    NaoMeEsquece - preferencias de notificacao do lembrete.

    A tabela PreferenciaUsuario guardava apenas FlNotificar (se o usuario quer
    ou nao ser avisado). Para escolher COMO o aviso chega - somente vibrando ou
    vibrando e tocando - faltavam as duas colunas criadas aqui.

    Padrao dos usuarios que ja existem: vibrar e tocar, que e o comportamento
    esperado de um lembrete e o que o aplicativo fazia antes de a escolha existir.

    O script e idempotente: pode ser executado mais de uma vez sem erro.

    Como executar:
        sqlcmd -S localhost\SQLEXPRESS -d NaoMeEsquece -E -i 02_PreferenciaNotificacao.sql
    ou abra no SSMS com o banco NaoMeEsquece selecionado e execute.
*/

USE NaoMeEsquece;
GO

SET NOCOUNT ON;
BEGIN TRANSACTION;
BEGIN TRY

    IF OBJECT_ID('dbo.PreferenciaUsuario', 'U') IS NULL
        THROW 50001, 'Tabela PreferenciaUsuario nao encontrada. Crie o banco antes de executar este script.', 1;

    /* ---------- FlVibrar ---------- */
    IF COL_LENGTH('dbo.PreferenciaUsuario', 'FlVibrar') IS NULL
    BEGIN
        ALTER TABLE dbo.PreferenciaUsuario
            ADD FlVibrar BIT NOT NULL
            CONSTRAINT DF_PreferenciaUsuario_FlVibrar DEFAULT (1);

        PRINT 'Coluna PreferenciaUsuario.FlVibrar criada.';
    END
    ELSE
        PRINT 'Coluna PreferenciaUsuario.FlVibrar ja existe.';

    /* ---------- FlSom ---------- */
    IF COL_LENGTH('dbo.PreferenciaUsuario', 'FlSom') IS NULL
    BEGIN
        ALTER TABLE dbo.PreferenciaUsuario
            ADD FlSom BIT NOT NULL
            CONSTRAINT DF_PreferenciaUsuario_FlSom DEFAULT (1);

        PRINT 'Coluna PreferenciaUsuario.FlSom criada.';
    END
    ELSE
        PRINT 'Coluna PreferenciaUsuario.FlSom ja existe.';

    COMMIT TRANSACTION;
    PRINT 'Preferencias de notificacao concluidas com sucesso.';

END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT 'Falha ao criar as colunas de notificacao. Nenhuma alteracao foi aplicada.';
    THROW;

END CATCH
GO

/* ---------- Conferencia ---------- */
SELECT c.name AS coluna, t.name AS tipo, c.is_nullable AS aceita_nulo
FROM sys.columns c
JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.PreferenciaUsuario')
ORDER BY c.column_id;
GO
