# Integración con n8n - Automatizaciones Externas

## Visión General

El sistema se integra con n8n para ejecutar automatizaciones externas que extienden las capacidades de la plataforma. n8n actúa como orquestador de workflows complejos que el sistema no puede ejecutar directamente.

## Arquitectura de Integración

### Componentes

1. **Sistema Principal (ASP.NET Core)**
   - Dispara automatizaciones cuando ocurren eventos
   - Envía datos a n8n vía webhook
   - Recibe respuestas de n8n vía webhook
   - Controla el estado de las automatizaciones

2. **n8n (Servicio Externo)**
   - Recibe webhooks del sistema
   - Ejecuta workflows complejos
   - Procesa datos con IA externa
   - Envía resultados de vuelta al sistema

3. **Base de Datos**
   - Almacena estado de automatizaciones
   - Registra eventos y resultados
   - Mantiene historial de ejecuciones

## Cuándo se Dispara una Automatización

### Eventos que Disparan Automatizaciones

#### 1. Eventos de Contenido

**Nuevo Contenido Cargado**
- **Trigger**: Usuario carga imagen/video
- **Condición**: Archivo validado y guardado exitosamente
- **Automatización**: Procesamiento con IA externa
- **Prioridad**: Alta

**Contenido Procesado por IA**
- **Trigger**: Sistema genera contenido con IA
- **Condición**: Contenido generado exitosamente
- **Automatización**: Optimización y mejora del contenido
- **Prioridad**: Media

#### 2. Eventos de Campaña

**Nueva Campaña Creada**
- **Trigger**: Usuario crea nueva campaña
- **Condición**: Campaña guardada en BD
- **Automatización**: Análisis de estrategia y recomendaciones
- **Prioridad**: Alta

**Campaña Activada**
- **Trigger**: Usuario activa una campaña
- **Condición**: Campaña cambia de estado a "Active"
- **Automatización**: Inicio de publicación automática
- **Prioridad**: Crítica

**Campaña Finalizada**
- **Trigger**: Campaña alcanza fecha de fin o se detiene
- **Condición**: Estado cambia a "Completed"
- **Automatización**: Análisis de resultados y generación de reporte
- **Prioridad**: Media

#### 3. Eventos de Automatización Interna

**Automatización Completa**
- **Trigger**: Automatización interna completa su ciclo
- **Condición**: Estado cambia a "Completed"
- **Automatización**: Sincronización con sistemas externos
- **Prioridad**: Baja

**Error en Automatización**
- **Trigger**: Automatización interna falla
- **Condición**: Estado cambia a "Error"
- **Automatización**: Notificación y recuperación
- **Prioridad**: Alta

#### 4. Eventos Temporales (Scheduled)

**Ejecución Programada**
- **Trigger**: Cron job o timer
- **Condición**: Hora programada alcanzada
- **Automatización**: Tareas periódicas (análisis, limpieza, reportes)
- **Prioridad**: Media

**Análisis Diario**
- **Trigger**: Cada día a hora específica
- **Condición**: Hora configurada
- **Automatización**: Análisis de métricas y generación de insights
- **Prioridad**: Media

#### 5. Eventos de Usuario

**Feedback del Usuario**
- **Trigger**: Usuario da feedback sobre contenido
- **Condición**: Feedback guardado
- **Automatización**: Ajuste de estrategia basado en feedback
- **Prioridad**: Media

**Cambio de Preferencias**
- **Trigger**: Usuario actualiza preferencias
- **Condición**: Preferencias guardadas
- **Automatización**: Actualización de modelos de IA
- **Prioridad**: Baja

## Qué Datos se Envían a n8n

### Estructura Base de Datos Enviados

#### 1. Metadata de la Solicitud
- **RequestId**: Identificador único de la solicitud
- **TenantId**: ID del tenant (obligatorio)
- **UserId**: ID del usuario que disparó el evento (opcional)
- **EventType**: Tipo de evento que disparó la automatización
- **Timestamp**: Fecha y hora del evento
- **Priority**: Prioridad de la automatización

#### 2. Datos del Evento

**Para Eventos de Contenido:**
- ContentId
- ContentType (Image/Video)
- FileUrl
- OriginalFileName
- FileSize
- IsAiGenerated
- CampaignId (si aplica)
- Tags
- Description

**Para Eventos de Campaña:**
- CampaignId
- CampaignName
- CampaignStatus
- StartDate
- EndDate
- Budget
- SpentAmount
- ContentIds (lista de contenido asociado)
- TargetAudience
- MarketingStrategy

**Para Eventos de Automatización:**
- AutomationId
- AutomationType
- PreviousStatus
- NewStatus
- ExecutionCount
- LastExecutionAt
- ErrorMessage (si hay error)

#### 3. Contexto Adicional
- **UserPreferences**: Preferencias del usuario (si aplica)
- **MemoryContext**: Contexto de memoria relevante
- **SystemMetrics**: Métricas actuales del sistema
- **RelatedData**: Datos relacionados al evento

#### 4. Configuración de la Automatización
- **WorkflowId**: ID del workflow en n8n
- **WorkflowVersion**: Versión del workflow
- **RetryPolicy**: Política de reintentos
- **Timeout**: Tiempo máximo de ejecución
- **ExpectedResponse**: Qué se espera recibir

### Ejemplo de Estructura (Conceptual)

```
Request {
    Metadata {
        RequestId
        TenantId
        UserId
        EventType
        Timestamp
        Priority
    }
    EventData {
        // Datos específicos del evento
    }
    Context {
        UserPreferences
        MemoryContext
        SystemMetrics
    }
    AutomationConfig {
        WorkflowId
        WorkflowVersion
        RetryPolicy
        Timeout
    }
}
```

## Qué Recibe el Sistema de n8n

### Tipos de Respuestas

#### 1. Respuesta de Éxito

**Estructura:**
- **RequestId**: ID de la solicitud original
- **Status**: "Success" o "Completed"
- **ExecutionTime**: Tiempo de ejecución
- **ResultData**: Datos procesados por n8n
- **GeneratedContent**: Contenido generado (si aplica)
- **Insights**: Insights o análisis generados
- **Recommendations**: Recomendaciones generadas
- **Metadata**: Metadata adicional del procesamiento

**Ejemplos de ResultData:**
- Contenido procesado mejorado
- Análisis de estrategia
- Métricas calculadas
- Reportes generados
- URLs de recursos creados

#### 2. Respuesta de Error

**Estructura:**
- **RequestId**: ID de la solicitud original
- **Status**: "Error" o "Failed"
- **ErrorCode**: Código de error
- **ErrorMessage**: Mensaje descriptivo
- **ErrorDetails**: Detalles técnicos
- **Retryable**: Si el error es recuperable
- **SuggestedAction**: Acción sugerida

#### 3. Respuesta de Progreso (Webhook Intermedio)

**Estructura:**
- **RequestId**: ID de la solicitud original
- **Status**: "InProgress" o "Processing"
- **Progress**: Porcentaje de progreso (0-100)
- **CurrentStep**: Paso actual del workflow
- **EstimatedTimeRemaining**: Tiempo estimado restante
- **PartialResults**: Resultados parciales (si aplica)

#### 4. Respuesta de Notificación

**Estructura:**
- **RequestId**: ID de la solicitud original
- **Status**: "Notification"
- **NotificationType**: Tipo de notificación
- **Message**: Mensaje de notificación
- **ActionRequired**: Si requiere acción del usuario
- **ActionUrl**: URL para realizar acción (si aplica)

### Validación de Respuestas

**El sistema valida:**
- ✅ RequestId debe existir en el sistema
- ✅ TenantId debe coincidir
- ✅ Status debe ser válido
- ✅ Timestamp debe ser reciente (no más de 24 horas)
- ✅ Firma/Token de autenticación debe ser válida

## Control de Estado

### Estados de Automatización Externa

#### Estados Principales

1. **Pending**
   - Automatización creada pero no enviada aún
   - Esperando condiciones para disparar

2. **Queued**
   - Enviada a n8n, esperando procesamiento
   - En cola de n8n

3. **InProgress**
   - n8n está procesando el workflow
   - Recibido webhook de progreso

4. **Completed**
   - Workflow completado exitosamente
   - Respuesta recibida y procesada

5. **Failed**
   - Workflow falló en n8n
   - Error recibido y registrado

6. **Timeout**
   - Workflow excedió tiempo máximo
   - No se recibió respuesta a tiempo

7. **Cancelled**
   - Automatización cancelada por usuario
   - No se procesará

8. **Retrying**
   - Reintentando después de error
   - Esperando nueva ejecución

### Flujo de Estados

```
Pending → Queued → InProgress → Completed
                              ↓
                         Failed/Timeout
                              ↓
                         Retrying → InProgress → ...
```

### Gestión de Estado

#### 1. Persistencia

**Tabla: AutomationExecutions**
- Almacena cada ejecución de automatización externa
- Registra estado actual
- Guarda datos enviados y recibidos
- Mantiene historial completo

**Campos:**
- ExecutionId (PK)
- TenantId (FK)
- RequestId (único)
- WorkflowId
- EventType
- Status
- DataSent (JSON)
- DataReceived (JSON)
- ErrorMessage
- RetryCount
- CreatedAt
- StartedAt
- CompletedAt
- UpdatedAt

#### 2. Actualización de Estado

**Métodos de Actualización:**

**Por Webhook (Push):**
- n8n envía webhook cuando cambia estado
- Sistema actualiza inmediatamente
- Método preferido para respuestas rápidas

**Por Polling (Pull):**
- Sistema consulta estado en n8n periódicamente
- Para automatizaciones de larga duración
- Fallback si webhook falla

**Por Timeout:**
- Sistema marca como timeout si no hay respuesta
- Después de tiempo configurado
- Permite reintentos automáticos

#### 3. Reintentos

**Política de Reintentos:**
- **MaxRetries**: 3 intentos máximo
- **RetryDelay**: Exponencial (1min, 5min, 15min)
- **RetryableErrors**: Solo errores recuperables
- **PermanentErrors**: No se reintentan

**Condiciones para Reintentar:**
- Error temporal (timeout, red)
- Error recuperable (rate limit)
- Estado "Retrying"

**Condiciones para NO Reintentar:**
- Error permanente (datos inválidos)
- Máximo de reintentos alcanzado
- Cancelado por usuario

#### 4. Monitoreo

**Métricas Monitoreadas:**
- Tasa de éxito/fallo
- Tiempo promedio de ejecución
- Número de reintentos
- Automatizaciones en cola
- Automatizaciones en progreso

**Alertas:**
- Alta tasa de fallos
- Tiempo de ejecución anormal
- Muchas automatizaciones en cola
- Errores repetidos

## Seguridad y Autenticación

### Autenticación con n8n

**Método 1: API Key**
- Sistema envía API key en header
- n8n valida key antes de procesar
- Método simple y seguro

**Método 2: Webhook Signature**
- n8n firma webhooks con secret
- Sistema valida firma antes de procesar
- Previene webhooks falsos

**Método 3: OAuth 2.0**
- Para integraciones más complejas
- Tokens con expiración
- Refresh tokens automáticos

### Validación de Datos

**Antes de Enviar:**
- Validar TenantId existe
- Validar datos requeridos presentes
- Sanitizar datos sensibles
- Verificar permisos del usuario

**Al Recibir:**
- Validar RequestId existe
- Validar TenantId coincide
- Validar firma/token
- Validar estructura de datos
- Sanitizar datos recibidos

## Configuración

### Configuración por Tenant

**Cada tenant puede tener:**
- URLs de n8n diferentes
- API keys diferentes
- Workflows personalizados
- Políticas de reintento personalizadas
- Timeouts personalizados

### Configuración Global

**Configuración del sistema:**
- URL base de n8n
- Timeout por defecto
- Política de reintentos por defecto
- Intervalo de polling
- Configuración de webhooks

## Próximos Pasos

1. ✅ Diseño lógico completado
2. ⏳ Implementar servicios de integración
3. ⏳ Crear casos de uso para disparar automatizaciones
4. ⏳ Crear endpoints para recibir webhooks
5. ⏳ Implementar control de estado
6. ⏳ Agregar monitoreo y alertas
7. ⏳ Crear UI para gestionar automatizaciones externas

