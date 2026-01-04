# Script para crear tabla DataProtectionKeys

## Problema
La tabla `DataProtectionKeys` no existe en la base de datos, causando errores de DataProtection.

## Solución
Ejecutar el script SQL directamente en PostgreSQL de Render.

## Pasos para ejecutar en Render

### Opción 1: Desde el Dashboard de Render
1. Ve a tu base de datos PostgreSQL en Render
2. Haz clic en "Connect" o "Open in pgAdmin"
3. Abre la consola SQL
4. Copia y pega el contenido de `create_dataprotectionkeys_table.sql`
5. Ejecuta el script

### Opción 2: Desde psql (línea de comandos)
```bash
# Conectarte a la base de datos de Render
psql "Host=dpg-d5a8afv5r7bs739m2vlg-a.virginia-postgres.render.com;Port=5432;Database=autonomousmarketingplatform;Username=admin;Password=0kAW5J0EWX3hR7GwDAhOUpv4ieV1IqN1;SSL Mode=Require"

# O desde el archivo
psql "Host=dpg-d5a8afv5r7bs739m2vlg-a.virginia-postgres.render.com;Port=5432;Database=autonomousmarketingplatform;Username=admin;Password=0kAW5J0EWX3hR7GwDAhOUpv4ieV1IqN1;SSL Mode=Require" -f scripts/create_dataprotectionkeys_table.sql
```

### Opción 3: Desde pgAdmin o DBeaver
1. Conecta a la base de datos de Render
2. Abre una nueva consulta SQL
3. Copia y pega el contenido de `create_dataprotectionkeys_table.sql`
4. Ejecuta

## Verificación
Después de ejecutar el script, verifica que la tabla existe:
```sql
SELECT * FROM "DataProtectionKeys";
```

Si no hay errores, la tabla se creó correctamente.

## Estructura de la tabla
- **Id**: INTEGER (SERIAL, auto-increment, PRIMARY KEY)
- **FriendlyName**: TEXT (nullable)
- **Xml**: TEXT (nullable)

Esta es la estructura exacta que Entity Framework Core espera para DataProtection keys.

