# Aplicar Migración de ApplicationLogs en Render

## Opción 1: Ejecutar SQL directamente en Render (RECOMENDADO)

1. **Conectarse a la base de datos de Render:**
   - Ve a tu dashboard de Render
   - Selecciona tu base de datos PostgreSQL
   - Haz clic en "Connect" o "PSQL"
   - O usa el comando PSQL desde tu terminal:

```bash
PGPASSWORD=0kAW5J0EWX3hR7GwDAhOUpv4ieV1IqN1 psql -h dpg-d5a8afv5r7bs739m2vlg-a.virginia-postgres.render.com -U admin autonomousmarketingplatform
```

2. **Ejecutar el script SQL:**
   - Copia el contenido de `docs/CREAR_TABLA_APPLICATION_LOGS.sql`
   - Pégalo en la consola de PSQL
   - Presiona Enter para ejecutar

## Opción 2: Usar EF Core Migrations (desde tu máquina local)

Si tienes acceso a la base de datos desde tu máquina local:

```bash
# Configurar la variable de entorno con la conexión de Render
$env:ConnectionStrings__DefaultConnection="Host=dpg-d5a8afv5r7bs739m2vlg-a.virginia-postgres.render.com;Port=5432;Database=autonomousmarketingplatform;Username=admin;Password=0kAW5J0EWX3hR7GwDAhOUpv4ieV1IqN1;SSL Mode=Require;"

# Aplicar la migración
dotnet ef database update --project src/AutonomousMarketingPlatform.Infrastructure/AutonomousMarketingPlatform.Infrastructure.csproj --startup-project src/AutonomousMarketingPlatform.Web/AutonomousMarketingPlatform.Web.csproj
```

## Opción 3: Ejecutar automáticamente en el startup de la aplicación

La aplicación puede aplicar migraciones automáticamente al iniciar. Esto ya está configurado en `Program.cs` si tienes el código de migración automática.

## Verificar que la tabla se creó correctamente

Después de ejecutar el script, verifica con:

```sql
-- Verificar que la tabla existe
\dt ApplicationLogs

-- Ver la estructura de la tabla
\d ApplicationLogs

-- Verificar índices
\di ApplicationLogs
```

## Notas importantes

- La tabla `ApplicationLogs` se creará automáticamente cuando la aplicación inicie si usas migraciones automáticas
- Los logs se persistirán automáticamente para todos los logs de nivel **Warning** o superior
- Los logs incluyen información del tenant, usuario, request, y excepciones completas
- Los índices están optimizados para consultas por nivel, tenant, usuario, fecha y source

