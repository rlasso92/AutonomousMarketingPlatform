# Sistema de API Key Encriptada en Base de Datos

## Resumen

Se implementó un sistema completo para almacenar API keys de IA de forma encriptada en la base de datos, configurable desde el frontend por cada tenant.

## Características Implementadas

### 1. Entidad TenantAIConfig
- **Ubicación**: `Domain/Entities/TenantAIConfig.cs`
- Almacena configuración de IA por tenant
- API key encriptada con AES-256
- Modelo, proveedor, estadísticas de uso
- Índice único por (TenantId, Provider)

### 2. Servicio de Encriptación
- **Ubicación**: `Infrastructure/Services/EncryptionService.cs`
- Encriptación/desencriptación AES-256
- Clave configurable desde `appsettings.json`
- **IMPORTANTE**: En producción, usar Azure Key Vault o similar

### 3. Endpoints y UI
- **Controller**: `Web/Controllers/AIConfigController.cs`
- **Vista**: `Web/Views/AIConfig/Index.cshtml`
- `GET /AIConfig/Index`: Vista para configurar API key
- `POST /AIConfig/Save`: Guarda/actualiza API key encriptada
- Solo accesible para roles **Owner/Admin**
- Muestra estadísticas de uso (último uso, veces usado)

### 4. OpenAIProvider Actualizado
- **Ubicación**: `Infrastructure/Services/AI/OpenAIProvider.cs`
- Prioriza API key desde base de datos (encriptada)
- Fallback a configuración si no hay en DB
- Desencripta automáticamente al usar
- Actualiza estadísticas de uso automáticamente

### 5. Migración
- **Nombre**: `AddTenantAIConfig`
- Crea tabla `TenantAIConfigs` con índices apropiados

## Flujo de Funcionamiento

1. **Configuración Inicial**:
   - Usuario Owner/Admin va a `/AIConfig/Index`
   - Ingresa API key de OpenAI
   - Sistema encripta y guarda en DB

2. **Uso de IA**:
   - `OpenAIProvider` busca configuración en DB para el tenant actual
   - Si existe, desencripta y usa la API key
   - Si no existe, usa configuración de `appsettings.json` (fallback)
   - Actualiza estadísticas de uso

3. **Seguridad**:
   - API keys nunca se exponen en logs
   - Encriptación AES-256
   - Multi-tenant: cada tenant tiene su propia configuración
   - Auditoría de cambios

## Configuración

### Desarrollo
```json
{
  "Encryption": {
    "Key": "CHANGE_THIS_IN_PRODUCTION_USE_32_CHAR_KEY!!"
  }
}
```

### Producción
```json
{
  "Encryption": {
    "Key": "CONFIGURE_FROM_ENVIRONMENT_OR_KEY_VAULT_32_CHARS"
  }
}
```

**IMPORTANTE**: La clave de encriptación debe ser de exactamente 32 caracteres para AES-256.

## Uso desde Frontend

1. Navegar a `/AIConfig/Index`
2. Seleccionar proveedor (OpenAI, etc.)
3. Ingresar API key
4. Seleccionar modelo
5. Guardar

La API key se encripta automáticamente antes de guardarse.

## Seguridad

✅ API keys encriptadas en base de datos  
✅ No se exponen en configuración (opcional)  
✅ Configuración por tenant (multi-tenant)  
✅ Auditoría de cambios  
✅ Solo Owner/Admin pueden configurar  
✅ Clave de encriptación configurable desde secrets

## Próximos Pasos

1. En producción, usar Azure Key Vault para la clave de encriptación
2. Implementar rotación de claves de encriptación
3. Agregar validación de API keys antes de guardar
4. Implementar soporte para múltiples proveedores (Anthropic, etc.)


