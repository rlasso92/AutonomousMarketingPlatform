# Preparación para Producción - SaaS

## Visión General

Este documento describe las medidas de seguridad, logging, auditoría y manejo de errores implementadas para preparar el sistema para producción como SaaS real.

## 1. Manejo de Secretos

### Estrategia de Secretos

**Niveles de Configuración:**
1. **Variables de Entorno**: Para desarrollo local
2. **Azure Key Vault / AWS Secrets Manager**: Para producción
3. **appsettings.json**: Solo valores no sensibles (estructura, defaults)

### Secretos que NO deben estar en código:
- ❌ Connection strings completas
- ❌ API keys de servicios externos
- ❌ Tokens de autenticación
- ❌ Claves de cifrado
- ❌ Credenciales de n8n
- ❌ Secrets de JWT

### Implementación

**Configuración por Entorno:**
- `appsettings.Development.json`: Valores de desarrollo
- `appsettings.Production.json`: Solo estructura (sin secretos)
- Variables de entorno: Secretos reales

**Uso de IConfiguration:**
```csharp
// Correcto: Leer de configuración
var apiKey = configuration["ExternalServices:n8n:ApiKey"];

// Incorrecto: Hardcoded
var apiKey = "hardcoded-key";
```

## 2. Seguridad Básica

### HTTPS Obligatorio
- ✅ Redirección automática HTTP → HTTPS
- ✅ HSTS habilitado
- ✅ Certificados válidos en producción

### CORS Configurado
- ✅ Orígenes permitidos específicos
- ✅ Métodos permitidos limitados
- ✅ Headers permitidos controlados
- ✅ Credenciales solo cuando necesario

### Rate Limiting
- ✅ Límite de requests por IP
- ✅ Límite por usuario autenticado
- ✅ Límite por endpoint específico
- ✅ Diferentes límites por tipo de operación

### Validación de Entrada
- ✅ Validación de modelos con FluentValidation
- ✅ Sanitización de entrada
- ✅ Protección contra XSS
- ✅ Protección contra SQL Injection (EF Core parametrizado)

### Headers de Seguridad
- ✅ Content-Security-Policy
- ✅ X-Frame-Options
- ✅ X-Content-Type-Options
- ✅ Referrer-Policy
- ✅ Permissions-Policy

## 3. Protección Multi-Tenant

### Verificaciones Obligatorias

**En cada request:**
1. ✅ TenantId debe estar presente
2. ✅ TenantId debe existir en BD
3. ✅ Tenant debe estar activo
4. ✅ Usuario debe pertenecer al tenant
5. ✅ Datos solo del tenant del usuario

**Middleware de Validación:**
- Intercepta todos los requests
- Valida tenant antes de procesar
- Rechaza requests sin tenant válido

**Filtrado Automático:**
- EF Core filtra automáticamente por TenantId
- Imposible acceder a datos de otros tenants
- Validación en repositorios

**Aislamiento de Datos:**
- ✅ Todas las consultas filtradas por TenantId
- ✅ SaveChanges asigna TenantId automáticamente
- ✅ Validación en cada operación de BD

## 4. Logging Estructurado

### Niveles de Log

**Trace**: Información muy detallada (solo desarrollo)
**Debug**: Información de depuración
**Information**: Flujo normal de la aplicación
**Warning**: Situaciones anómalas pero recuperables
**Error**: Errores que requieren atención
**Critical**: Errores críticos que requieren acción inmediata

### Información en Logs

**Cada log incluye:**
- Timestamp
- Nivel de log
- Mensaje
- TenantId (si aplica)
- UserId (si aplica)
- RequestId (correlación)
- Exception (si aplica)
- Contexto adicional

### Destinos de Log

**Desarrollo:**
- Console
- Debug output

**Producción:**
- Application Insights / CloudWatch
- Archivos de log (rotación)
- Alertas para errores críticos

### Logs Sensibles

**NO se loguean:**
- ❌ Contraseñas
- ❌ Tokens completos
- ❌ Datos personales sensibles
- ❌ API keys completas

**Se loguean (sanitizados):**
- ✅ IDs de entidades
- ✅ Tipos de operación
- ✅ Estados
- ✅ Errores (sin datos sensibles)

## 5. Auditoría

### Eventos Auditados

**Autenticación:**
- Login exitoso
- Login fallido
- Logout
- Cambio de contraseña

**Autorización:**
- Acceso denegado
- Cambio de permisos

**Operaciones Críticas:**
- Creación de campañas
- Activación de campañas
- Carga de contenido
- Generación de contenido con IA
- Cambios en configuración
- Cambios en automatizaciones

**Multi-Tenant:**
- Intentos de acceso a datos de otro tenant
- Cambios en configuración de tenant

### Información Auditada

**Cada evento incluye:**
- Timestamp
- TenantId
- UserId
- Action (qué se hizo)
- EntityType (tipo de entidad)
- EntityId (ID de entidad)
- OldValues (valores anteriores, si aplica)
- NewValues (valores nuevos, si aplica)
- IP Address
- User Agent
- Result (Success/Failed)
- ErrorMessage (si falló)

### Almacenamiento

**Tabla: AuditLogs**
- Persistencia en BD
- Retención configurable
- Búsqueda y filtrado
- Exportación para compliance

## 6. Escenarios de Error

### Manejo Global de Errores

**Middleware de Manejo de Errores:**
- Captura todas las excepciones
- Logs estructurados
- Respuestas consistentes
- No expone detalles internos en producción

### Tipos de Errores

**Errores de Validación (400):**
- Datos inválidos
- Validación de modelo fallida
- Respuesta: Mensaje claro de validación

**Errores de Autenticación (401):**
- Token inválido
- Token expirado
- Respuesta: "No autenticado"

**Errores de Autorización (403):**
- Sin permisos
- Acceso a tenant no permitido
- Respuesta: "No autorizado"

**Errores No Encontrados (404):**
- Recurso no existe
- Respuesta: "No encontrado"

**Errores de Conflicto (409):**
- Recurso ya existe
- Estado inválido
- Respuesta: Mensaje de conflicto

**Errores del Servidor (500):**
- Errores inesperados
- Respuesta: Mensaje genérico (sin detalles en producción)
- Log detallado internamente

### Respuestas de Error Consistentes

**Formato estándar:**
```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Mensaje amigable",
    "details": {}, // Solo en desarrollo
    "requestId": "correlation-id",
    "timestamp": "2024-01-01T00:00:00Z"
  }
}
```

### Recuperación de Errores

**Reintentos Automáticos:**
- Operaciones idempotentes
- Límite de reintentos
- Backoff exponencial

**Circuit Breaker:**
- Para servicios externos
- Evita cascadas de fallos
- Recuperación automática

## 7. Configuración de Producción

### Variables de Entorno Requeridas

**Base de Datos:**
- `ConnectionStrings__DefaultConnection`
- `Database__Provider` (PostgreSQL)

**Seguridad:**
- `JWT__SecretKey`
- `JWT__Issuer`
- `JWT__Audience`

**Servicios Externos:**
- `ExternalServices__n8n__BaseUrl`
- `ExternalServices__n8n__ApiKey`
- `ExternalServices__n8n__WebhookSecret`

**Logging:**
- `Logging__ApplicationInsights__ConnectionString`
- `Logging__Level__Default`

**Multi-Tenant:**
- `MultiTenant__ValidationEnabled` (true en producción)

### Health Checks

**Endpoints de salud:**
- `/health`: Salud general
- `/health/ready`: Listo para recibir tráfico
- `/health/live`: Servicio vivo

**Verificaciones:**
- ✅ Conexión a BD
- ✅ Servicios externos
- ✅ Memoria disponible
- ✅ CPU usage

### Monitoreo

**Métricas:**
- Request rate
- Error rate
- Response time
- Active tenants
- Active users
- Database connections

**Alertas:**
- Error rate > threshold
- Response time > threshold
- Health check failures
- Security events

## 8. Checklist de Producción

### Pre-Deployment

- [ ] Todos los secretos en Key Vault
- [ ] HTTPS configurado
- [ ] CORS configurado correctamente
- [ ] Rate limiting habilitado
- [ ] Logging configurado
- [ ] Auditoría habilitada
- [ ] Health checks configurados
- [ ] Manejo de errores global
- [ ] Validación multi-tenant estricta
- [ ] Migraciones de BD aplicadas
- [ ] Backup de BD configurado

### Post-Deployment

- [ ] Verificar health checks
- [ ] Verificar logs
- [ ] Verificar métricas
- [ ] Probar autenticación
- [ ] Probar multi-tenant
- [ ] Probar manejo de errores
- [ ] Verificar auditoría

