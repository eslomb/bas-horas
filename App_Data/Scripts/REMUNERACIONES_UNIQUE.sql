-- REMUNERACIONES: una sola remuneracion por usuario, anio y mes (indice UNIQUE).
-- Lo asume la API (/api/remuneraciones: el alta devuelve 409 y la importacion hace upsert por esa clave).
-- Idempotente: no hace nada si el indice ya existe.
--
-- Antes de crear el indice resuelve las repetidas que ya existan:
--   - Si todas las filas repetidas de una misma combinacion tienen el MISMO monto, deja la de menor IDRem y borra el resto.
--   - Si alguna combinacion repetida tiene montos DISTINTOS no decide por nadie: muestra esas filas, avisa y sale sin
--     cambiar nada. Hay que corregirlas a mano y volver a correr el script.
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = 'UX_REMUNERACIONES_Usuario_Anio_Mes' AND object_id = OBJECT_ID('dbo.REMUNERACIONES'))
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.REMUNERACIONES
               GROUP BY Usuario, Anio, Mes
               HAVING COUNT(*) > 1 AND MIN(Remuneracion) <> MAX(Remuneracion))
    BEGIN
        SELECT r.*
        FROM dbo.REMUNERACIONES r
        JOIN (SELECT Usuario, Anio, Mes FROM dbo.REMUNERACIONES
              GROUP BY Usuario, Anio, Mes
              HAVING COUNT(*) > 1 AND MIN(Remuneracion) <> MAX(Remuneracion)) d
          ON d.Usuario = r.Usuario AND d.Anio = r.Anio AND d.Mes = r.Mes
        ORDER BY r.Usuario, r.Anio, r.Mes, r.IDRem;

        RAISERROR('Hay remuneraciones repetidas (mismo usuario, anio y mes) con montos distintos. Corregirlas a mano y volver a ejecutar. No se hizo ningun cambio.', 16, 1);
        RETURN;
    END

    ;WITH ordenadas AS (
        SELECT ROW_NUMBER() OVER (PARTITION BY Usuario, Anio, Mes ORDER BY IDRem) AS rn
        FROM dbo.REMUNERACIONES
    )
    DELETE FROM ordenadas WHERE rn > 1;

    CREATE UNIQUE NONCLUSTERED INDEX UX_REMUNERACIONES_Usuario_Anio_Mes
        ON dbo.REMUNERACIONES (Usuario, Anio, Mes);
END
