-- Tabla de feedback de usuarios (comentarios, sugerencias y reportes de error).
-- Usada por la API: /api/feedback. Idempotente: no hace nada si la tabla ya existe.
IF OBJECT_ID('dbo.FEEDBACK', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.FEEDBACK (
        IDFeedback  int IDENTITY(1,1) NOT NULL,
        Usuario     nvarchar(256)     NOT NULL,
        Email       nvarchar(256)     NULL,
        Comentario  nvarchar(2000)    NOT NULL,
        Tipo        varchar(20)       NOT NULL,
        Fecha       datetime          NOT NULL CONSTRAINT DF_FEEDBACK_Fecha DEFAULT (GETDATE()),
        CONSTRAINT PK_FEEDBACK PRIMARY KEY CLUSTERED (IDFeedback),
        CONSTRAINT CK_FEEDBACK_Tipo CHECK (Tipo IN ('Sugerencia', 'Error', 'Otro'))
    );

    CREATE INDEX IX_FEEDBACK_Usuario ON dbo.FEEDBACK (Usuario);
END
