# Sistema de Métricas y Aprendizaje Automático

## 📊 Descripción General

El sistema de métricas permite registrar, visualizar y analizar el rendimiento de campañas y publicaciones en redes sociales. Además, incluye un sistema de aprendizaje automático que actualiza la memoria de marketing basándose en los resultados obtenidos.

## 🎯 Características Principales

### 1. Registro de Métricas

#### Métricas por Campaña (`CampaignMetrics`)
- **Impresiones**: Número de veces que se mostró el contenido
- **Clics**: Número de clics en el contenido
- **Engagement**: Interacciones totales (likes + comentarios + compartidos)
- **Desglose de Engagement**:
  - Likes
  - Comentarios
  - Compartidos
- **Posts Activos**: Número de publicaciones activas en la fecha
- **Fuente**: Manual, API, Facebook, Instagram, TikTok, etc.

#### Métricas por Publicación (`PublishingJobMetrics`)
- **Impresiones**: Vistas de la publicación
- **Clics**: Clics en la publicación
- **Engagement**: Interacciones totales
- **Desglose de Engagement**: Likes, Comentarios, Compartidos
- **Tasas Calculadas**:
  - CTR (Click-Through Rate): (Clics / Impresiones) * 100
  - Engagement Rate: (Engagement / Impresiones) * 100

### 2. Registro Manual

Los usuarios pueden registrar métricas manualmente a través de formularios en la UI:
- **Registro de Métricas de Campaña**: `/Metrics/RegisterCampaign/{campaignId}`
- **Registro de Métricas de Publicación**: `/Metrics/RegisterPublishingJob/{publishingJobId}`

### 3. Visualización de Métricas

#### Listado de Métricas
- **Ruta**: `/Metrics`
- Muestra resumen de métricas de todas las campañas
- Filtros por rango de fechas
- Tarjetas con resumen de impresiones, clics y engagement

#### Detalle de Campaña
- **Ruta**: `/Metrics/Campaign/{campaignId}`
- Resumen total de métricas
- Tasas de rendimiento (CTR, Engagement Rate)
- Tabla de métricas diarias
- Botón para registrar nuevas métricas

#### Detalle de Publicación
- **Ruta**: `/Metrics/PublishingJob/{publishingJobId}`
- Resumen de métricas de la publicación
- Tasas calculadas
- Tabla de métricas diarias
- Botón para registrar nuevas métricas

### 4. Aprendizaje Automático

El sistema analiza automáticamente las métricas y actualiza la memoria de marketing con aprendizajes.

#### Proceso Automático

1. **Trigger Inmediato**: Después de registrar métricas, se dispara automáticamente el proceso de aprendizaje (en background)
2. **Proceso Diario**: Un background service ejecuta análisis diario de todas las métricas recientes (últimos 7 días)

#### Aprendizajes Generados

**Para Campañas:**
- Detecta campañas exitosas (engagement rate > 3%)
- Identifica campañas con bajo rendimiento
- Guarda preferencias de estrategia exitosa
- Analiza rendimiento por canal

**Para Publicaciones:**
- Identifica posts exitosos
- Detecta qué tono funciona mejor en cada canal
- Guarda preferencias de formato y hashtags
- Analiza correlaciones entre contenido y engagement

#### Actualización de Memoria

Los aprendizajes se guardan en `MarketingMemory` con:
- **Tags**: Metrics, Campaign/Post, Success/NeedsImprovement, Channel, etc.
- **Relevancia**: Mayor score para aprendizajes de contenido exitoso
- **Contexto**: Incluye métricas y resultados en formato JSON

### 5. Integración con Motor IA

El Motor IA lee automáticamente la memoria de marketing antes de generar contenido nuevo, permitiendo que:
- Use estrategias que funcionaron bien anteriormente
- Evite formatos o tonos que no funcionaron
- Aplique preferencias detectadas del usuario
- Mejore continuamente la calidad del contenido generado

## 🔧 Arquitectura Técnica

### Entidades

#### `CampaignMetrics`
```csharp
- TenantId (Guid)
- CampaignId (Guid)
- MetricDate (DateTime)
- Impressions (long)
- Clicks (long)
- Engagement (long)
- Likes, Comments, Shares (long)
- ActivePosts (int)
- IsManualEntry (bool)
- Source (string)
- Notes (string)
```

#### `PublishingJobMetrics`
```csharp
- TenantId (Guid)
- PublishingJobId (Guid)
- MetricDate (DateTime)
- Impressions (long)
- Clicks (long)
- Engagement (long)
- Likes, Comments, Shares (long)
- ClickThroughRate (decimal?)
- EngagementRate (decimal?)
- IsManualEntry (bool)
- Source (string)
- Notes (string)
```

### Servicios

#### `IMetricsService`
- `RegisterCampaignMetricsAsync`: Registra métricas de campaña
- `RegisterPublishingJobMetricsAsync`: Registra métricas de publicación
- `GetCampaignMetricsAsync`: Obtiene métricas de campaña
- `GetPublishingJobMetricsAsync`: Obtiene métricas de publicación
- `GetCampaignMetricsSummaryAsync`: Resumen de métricas de campaña
- `GetPublishingJobMetricsSummaryAsync`: Resumen de métricas de publicación
- `GetAllCampaignsMetricsAsync`: Lista métricas de todas las campañas

#### `IMemoryLearningService`
- `LearnFromCampaignMetricsAsync`: Analiza métricas de campaña y actualiza memoria
- `LearnFromPublishingJobMetricsAsync`: Analiza métricas de publicación y actualiza memoria
- `ProcessLearningFromRecentMetricsAsync`: Proceso automático para múltiples métricas

### Background Services

#### `MetricsLearningBackgroundService`
- Se ejecuta diariamente
- Procesa todos los tenants activos
- Analiza métricas de los últimos 7 días
- Actualiza memoria automáticamente

## 📱 Interfaz de Usuario

### Navegación

- **Menú Principal**: Enlace "Métricas" en el sidebar
- **Desde Campañas**: Botón "Ver Métricas" en `Campaigns/Details`
- **Desde Publicaciones**: Botón "Ver Métricas" en `Publishing/Details`

### Formularios

Los formularios de registro incluyen:
- Fecha de las métricas
- Impresiones
- Clics
- Likes, Comentarios, Compartidos
- Fuente (Manual, API, etc.)
- Notas opcionales

### Visualización

- **Tarjetas de Resumen**: Impresiones, Clics, Engagement, Posts
- **Tasas Calculadas**: CTR y Engagement Rate
- **Tablas Diarias**: Historial completo de métricas por fecha
- **Filtros**: Por rango de fechas

## 🔄 Flujo de Trabajo

1. **Publicación**: Se publica contenido en redes sociales
2. **Registro de Métricas**: Usuario registra métricas manualmente o desde API
3. **Aprendizaje Automático**: Sistema analiza métricas y actualiza memoria
4. **Mejora Continua**: Motor IA usa memoria actualizada para generar mejor contenido

## 📈 Métricas Clave

### Engagement Rate
```
Engagement Rate = (Total Engagement / Total Impressions) * 100
```
- **Excelente**: > 5%
- **Bueno**: 3% - 5%
- **Regular**: 1% - 3%
- **Bajo**: < 1%

### Click-Through Rate (CTR)
```
CTR = (Total Clics / Total Impressions) * 100
```
- **Excelente**: > 2%
- **Bueno**: 1% - 2%
- **Regular**: 0.5% - 1%
- **Bajo**: < 0.5%

## 🚀 Mejoras Futuras

- [ ] Integración con APIs de redes sociales (Facebook, Instagram, TikTok)
- [ ] Gráficos y visualizaciones avanzadas
- [ ] Alertas automáticas por bajo rendimiento
- [ ] Comparación de métricas entre campañas
- [ ] Exportación de reportes
- [ ] Dashboard con métricas agregadas
- [ ] Predicción de rendimiento basada en histórico

## 📝 Notas

- Las métricas se almacenan por fecha (sin hora)
- Se permite un solo registro de métricas por campaña/publicación por fecha
- El aprendizaje automático se ejecuta en background para no bloquear la UI
- La memoria se actualiza con tags relevantes para facilitar la búsqueda

