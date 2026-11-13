-- Tokens de un solo uso para restablecer la contrasena (link de recuperacion por mail y cambio obligatorio al loguearse).
-- Usada por la API: /api/auth/recuperar-password y /api/auth/restablecer-password.
-- Solo se guarda el hash SHA-256 (hex) del token: el token en claro viaja unicamente en el link.
-- Idempotente: no hace nada si la tabla ya existe.
IF OBJECT_ID('dbo.PASSWORD_RESET_TOKENS', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PASSWORD_RESET_TOKENS (
        IdToken         int IDENTITY(1,1) NOT NULL,
        UserName        nvarchar(256)     NOT NULL,
        TokenHash       char(64)          NOT NULL,
        Origen          varchar(20)       NOT NULL,
        FechaCreacion   datetime          NOT NULL CONSTRAINT DF_PASSWORD_RESET_TOKENS_FechaCreacion DEFAULT (GETDATE()),
        FechaExpiracion datetime          NOT NULL,
        FechaUso        datetime          NULL,
        CONSTRAINT PK_PASSWORD_RESET_TOKENS PRIMARY KEY CLUSTERED (IdToken),
        CONSTRAINT UX_PASSWORD_RESET_TOKENS_TokenHash UNIQUE (TokenHash)
    );

    -- Invalidar los tokens de un usuario y contar sus pedidos recientes.
    CREATE INDEX IX_PASSWORD_RESET_TOKENS_UserName ON dbo.PASSWORD_RESET_TOKENS (UserName, FechaCreacion);
END
