-- Script para crear la tabla DataProtectionKeys directamente en PostgreSQL
-- Ejecutar este script en la base de datos de Render para crear la tabla manualmente

-- Crear la tabla DataProtectionKeys
CREATE TABLE IF NOT EXISTS "DataProtectionKeys" (
    "Id" SERIAL PRIMARY KEY,
    "FriendlyName" TEXT NULL,
    "Xml" TEXT NULL
);

-- Verificar que la tabla se creó correctamente
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable
FROM 
    information_schema.columns
WHERE 
    table_name = 'DataProtectionKeys'
ORDER BY 
    ordinal_position;
