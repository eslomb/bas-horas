-- Tabla de monitoreo de uso del frontend: una fila por visita (usuario, momento, URL).
-- Usada por la API: /api/monitoreo. Idempotente: no hace nada si la tabla ya existe.
IF OBJECT_ID('dbo.MONITOREO', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MONITOREO (
        IDMonitoreo int IDENTITY(1,1) NOT NULL,
        Usuario     nvarchar(256)     NOT NULL,
        Fecha       datetime          NOT NULL CONSTRAINT DF_MONITOREO_Fecha DEFAULT (GETDATE()),
        Url         nvarchar(2000)    NOT NULL,
        CONSTRAINT PK_MONITOREO PRIMARY KEY CLUSTERED (IDMonitoreo)
    );

    -- Las lecturas filtran por rango de fechas y agrupan por usuario.
    -- No se indexa Url: nvarchar(2000) excede el limite de 900 bytes de una clave de indice.
    CREATE INDEX IX_MONITOREO_Fecha_Usuario ON dbo.MONITOREO (Fecha, Usuario);
END
