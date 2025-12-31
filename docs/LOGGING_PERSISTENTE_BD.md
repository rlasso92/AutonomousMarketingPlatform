# Sistema de Logging Persistente en Base de Datos

## ✅ Implementación Completada

Se ha implementado un sistema completo de logging persistente que guarda logs críticos directamente en la base de datos PostgreSQL.

## 📋 Componentes Implementados

### 1. Entidad `ApplicationLog`
- **Ubicación:** `src/AutonomousMarketingPlatform.Domain/Entities/ApplicationLog.cs`
- **Campos principales:**
  - `Id` (UUID)
  - `Level` (Error, Warning, Information, Debug, Critical)
  - `Message` (mensaje del log)
  - `Source` (origen: AccountController, TenantResolver, etc.)
  - `TenantId` (opcional, para multi-tenant)
  - `UserId` (opcional)
  - `StackTrace` (para errores)
  - `ExceptionType` (tipo de excepción)
  - `RequestId` (para correlación)
  - `Path`, `HttpMethod` (información HTTP)
  - `IpAddress`, `UserAgent` (información del cliente)
  - `CreatedAt`, `UpdatedAt`, `IsActive`

### 2. Servicio `ILoggingService`
- **Interfaz:** `src/AutonomousMarketingPlatform.Application/Services/ILoggingService.cs`
- **Implementación:** `src/AutonomousMarketingPlatform.Infrastructure/Services/LoggingService.cs`
- **Métodos disponibles:**
  - `LogAsync()` - Log genérico
  - `LogErrorAsync()` - Log de error
  - `LogWarningAsync()` - Log de warning
  - `LogInformationAsync()` - Log de información
  - `LogCriticalAsync()` - Log crítico

### 3. Proveedor de Logging `DatabaseLoggerProvider`
- **Ubicación:** `src/AutonomousMarketingPlatform.Infrastructure/Logging/DatabaseLoggerProvider.cs`
- **Funcionalidad:**
  - Intercepta automáticamente todos los logs de nivel **Warning o superior**
  - Extrae información del contexto HTTP (tenant, usuario, request, etc.)
  - Persiste los logs de forma asíncrona (fire-and-forget)
  - No bloquea el flujo principal de la aplicación

### 4. Configuración en `Program.cs`
- Registrado como `Singleton` para evitar dependencias circulares
- Integrado con el sistema de logging de ASP.NET Core
- Solo persiste logs de nivel Warning o superior para no saturar la BD

## 🗄️ Base de Datos

### Migración
- **Archivo:** `src/AutonomousMarketingPlatform.Infrastructure/Migrations/20250101000001_AddApplicationLogsTable.cs`
- **Tabla:** `ApplicationLogs`
- **Índices creados:**
  - `IX_ApplicationLogs_Level` - Para filtrar por nivel
  - `IX_ApplicationLogs_TenantId` - Para filtrar por tenant
  - `IX_ApplicationLogs_UserId` - Para filtrar por usuario
  - `IX_ApplicationLogs_CreatedAt` - Para ordenar por fecha
  - `IX_ApplicationLogs_Source` - Para filtrar por origen
  - `IX_ApplicationLogs_RequestId` - Para correlación de requests

## 🚀 Aplicar en Render

### Opción 1: Ejecutar SQL directamente (RECOMENDADO)

1. Conectarse a la base de datos de Render usando PSQL:
```bash
PGPASSWORD=0kAW5J0EWX3hR7GwDAhOUpv4ieV1IqN1 psql -h dpg-d5a8afv5r7bs739m2vlg-a.virginia-postgres.render.com -U admin autonomousmarketingplatform
```

2. Ejecutar el script SQL:
   - Ver archivo: `docs/CREAR_TABLA_APPLICATION_LOGS.sql`
   - O copiar y pegar el contenido en la consola de PSQL

### Opción 2: Migración automática
La aplicación aplicará automáticamente las migraciones al iniciar si está configurado en `Program.cs`.

## 📊 Uso del Sistema

### Automático (Recomendado)
El sistema funciona automáticamente. Todos los logs de nivel **Warning o superior** se persisten automáticamente en la base de datos con:
- Información del tenant (si está disponible)
- Información del usuario (si está autenticado)
- Request ID para correlación
- Stack trace completo para errores
- Información HTTP (path, method, IP, User-Agent)

### Manual (Opcional)
También puedes usar el servicio directamente en tus controladores o servicios:

```csharp
public class MyController : Controller
{
    private readonly ILoggingService _loggingService;

    public MyController(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public async Task<IActionResult> MyAction()
    {
        try
        {
            // Tu código aquí
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync(
                "Error al procesar acción",
                "MyController",
                ex,
                tenantId: GetTenantId(),
                userId: GetUserId(),
                requestId: HttpContext.TraceIdentifier,
                path: HttpContext.Request.Path,
                httpMethod: HttpContext.Request.Method
            );
            throw;
        }
    }
}
```

## 🔍 Consultar Logs

### Por nivel de error:
```sql
SELECT * FROM "ApplicationLogs" 
WHERE "Level" = 'Error' 
ORDER BY "CreatedAt" DESC 
LIMIT 100;
```

### Por tenant:
```sql
SELECT * FROM "ApplicationLogs" 
WHERE "TenantId" = 'TENANT_ID_AQUI'
ORDER BY "CreatedAt" DESC;
```

### Por usuario:
```sql
SELECT * FROM "ApplicationLogs" 
WHERE "UserId" = 'USER_ID_AQUI'
ORDER BY "CreatedAt" DESC;
```

### Por request (correlación):
```sql
SELECT * FROM "ApplicationLogs" 
WHERE "RequestId" = 'REQUEST_ID_AQUI'
ORDER BY "CreatedAt" ASC;
```

### Errores recientes:
```sql
SELECT 
    "Level",
    "Message",
    "Source",
    "ExceptionType",
    "CreatedAt",
    "Path"
FROM "ApplicationLogs" 
WHERE "Level" IN ('Error', 'Critical')
AND "CreatedAt" >= NOW() - INTERVAL '24 hours'
ORDER BY "CreatedAt" DESC;
```

## ⚙️ Configuración

### Cambiar nivel mínimo de logs
Edita `DatabaseLogger.cs` y modifica el método `IsEnabled()`:

```csharp
public bool IsEnabled(LogLevel logLevel)
{
    // Cambiar a LogLevel.Information para persistir más logs
    // o LogLevel.Error para solo errores críticos
    return logLevel >= LogLevel.Warning;
}
```

### Desactivar logging persistente
Comenta o elimina el registro en `Program.cs`:

```csharp
// builder.Services.AddSingleton<ILoggingService, LoggingService>();
// builder.Services.AddSingleton<ILoggerProvider, DatabaseLoggerProvider>();
```

## 🎯 Casos de Uso Específicos

### 1. Errores de Campañas
Los errores al crear/actualizar campañas se persisten automáticamente con:
- Tenant ID
- User ID
- Stack trace completo
- Request ID para correlación

### 2. Publicaciones Fallidas
Los errores de publicación se capturan con:
- Información del job de publicación
- Excepción completa
- Contexto del tenant

### 3. Errores de IA
Los errores de generación de contenido con IA incluyen:
- Prompt utilizado
- Respuesta de la API
- Tipo de error

### 4. Problemas Multi-Tenant
Los errores de resolución de tenant se registran con:
- Host del request
- Subdomain intentado
- Usuario autenticado (si aplica)

## 📝 Notas Importantes

1. **Rendimiento:** Los logs se persisten de forma asíncrona (fire-and-forget) para no bloquear el flujo principal
2. **Volumen:** Solo se persisten logs de Warning o superior para no saturar la base de datos
3. **Dependencias Circulares:** El `LoggingService` no usa `ILogger` para evitar dependencias circulares
4. **Índices:** Los índices están optimizados para consultas comunes por nivel, tenant, usuario y fecha
5. **Retención:** Considera implementar un job de limpieza para eliminar logs antiguos (ej: > 90 días)

## 🔐 Seguridad

- Los logs pueden contener información sensible
- Considera sanitizar datos antes de persistir (passwords, tokens, etc.)
- Implementa acceso controlado a la tabla `ApplicationLogs` en producción
- Los logs incluyen IP addresses y User-Agents para auditoría

