# Flujo Completo del Workflow - Paso a Paso

## 📋 Índice
1. [Payload Inicial](#1-payload-inicial)
2. [Recepción y Normalización](#2-recepción-y-normalización)
3. [Validación](#3-validación)
4. [Preservación de Datos](#4-preservación-de-datos)
5. [Carga de Memoria](#5-carga-de-memoria)
6. [Análisis con IA](#6-análisis-con-ia)
7. [Generación de Estrategia](#7-generación-de-estrategia)
8. [Generación de Copy](#8-generación-de-copy)
9. [Generación de Prompts Visuales](#9-generación-de-prompts-visuales)
10. [Motor de Decisiones Cognitivo](#10-motor-de-decisiones-cognitivo)
11. [Construcción del Marketing Pack](#11-construcción-del-marketing-pack)
12. [Preparación de Jobs de Publicación](#12-preparación-de-jobs-de-publicación)
13. [Publicación por Canal](#13-publicación-por-canal)
14. [Procesamiento de Resultados](#14-procesamiento-de-resultados)

---

## 1. Payload Inicial

### 📥 **Nodo: Webhook - Receive Request**

**Recibe:**
```json
{
  "headers": {
    "host": "localhost:5678",
    "user-agent": "AutonomousMarketingPlatform/1.0",
    "content-type": "application/json; charset=utf-8"
  },
  "body": {
    "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
    "userId": "532b8976-25e8-4f84-953e-289cec40aebf",
    "instruction": "Crear contenido de marketing para redes sociales con tono profesional y moderno, orientado a aumentar el engagement y la visibilidad de la marca en Instagram y LinkedIn.",
    "channels": ["instagram", "facebook", "tiktok"],
    "requiresApproval": false,
    "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
    "assets": []
  }
}
```

**Pasa a siguiente nodo:**
```json
{
  "body": {
    "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
    "userId": "532b8976-25e8-4f84-953e-289cec40aebf",
    "instruction": "Crear contenido de marketing...",
    "channels": ["instagram", "facebook", "tiktok"],
    "requiresApproval": false,
    "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
    "assets": []
  }
}
```

---

## 2. Recepción y Normalización

### 🔄 **Nodo: Normalize Payload**

**Recibe:**
```json
{
  "body": {
    "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
    "userId": "532b8976-25e8-4f84-953e-289cec40aebf",
    "instruction": "Crear contenido de marketing...",
    "channels": ["instagram", "facebook", "tiktok"],
    "requiresApproval": false,
    "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
    "assets": []
  }
}
```

**Preserva el body completo:**
```json
{
  "body": {
    "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
    "userId": "532b8976-25e8-4f84-953e-289cec40aebf",
    "instruction": "Crear contenido de marketing...",
    "channels": ["instagram", "facebook", "tiktok"],
    "requiresApproval": false,
    "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
    "assets": []
  }
}
```

**Acción:** Preserva el `body` completo para uso posterior.

---

## 3. Validación

### ✅ **Nodo: Validate Required Fields**

**Recibe:**
```json
{
  "body": {
    "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
    "userId": "532b8976-25e8-4f84-953e-289cec40aebf",
    "instruction": "Crear contenido de marketing...",
    "channels": ["instagram", "facebook", "tiktok"],
    "requiresApproval": false,
    "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
    "assets": []
  }
}
```

**Valida:**
1. ✅ `body.tenantId` no está vacío → "94a41b59-d900-474f-9834-c8806c6db537"
2. ✅ `body.userId` no está vacío → "532b8976-25e8-4f84-953e-289cec40aebf"
3. ✅ `body.instruction` no está vacío → "Crear contenido de marketing..."
4. ✅ `body.channels.length > 0` → 3 canales

**Si todas las validaciones pasan → Continúa al siguiente nodo**

---

## 4. Preservación de Datos

### 💾 **Nodo: Set Validated Data**

**Recibe:**
```json
{
  "body": {
    "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
    "userId": "532b8976-25e8-4f84-953e-289cec40aebf",
    "instruction": "Crear contenido de marketing...",
    "channels": ["instagram", "facebook", "tiktok"],
    "requiresApproval": false,
    "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
    "assets": []
  }
}
```

**Crea dos objetos:**
```json
{
  "body": {
    "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
    "userId": "532b8976-25e8-4f84-953e-289cec40aebf",
    "instruction": "Crear contenido de marketing...",
    "channels": ["instagram", "facebook", "tiktok"],
    "requiresApproval": false,
    "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
    "assets": []
  },
  "validatedData": {
    "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
    "userId": "532b8976-25e8-4f84-953e-289cec40aebf",
    "instruction": "Crear contenido de marketing...",
    "channels": ["instagram", "facebook", "tiktok"],
    "requiresApproval": false,
    "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
    "assets": [],
    "contentId": null,
    "receivedAt": "2026-01-08T12:00:00.000Z"
  }
}
```

**Importante:** Este nodo preserva los canales originales: `["instagram", "facebook", "tiktok"]`

---

### 🔄 **Nodo: Set Cognitive Engine Version**

**Recibe:**
```json
{
  "body": {
    "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
    "userId": "532b8976-25e8-4f84-953e-289cec40aebf",
    "instruction": "Crear contenido de marketing...",
    "channels": ["instagram", "facebook", "tiktok"],
    "requiresApproval": false,
    "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
    "assets": []
  },
  "validatedData": { ... }
}
```

**Preserva el body:**
```json
{
  "body": {
    "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
    "userId": "532b8976-25e8-4f84-953e-289cec40aebf",
    "instruction": "Crear contenido de marketing...",
    "channels": ["instagram", "facebook", "tiktok"],
    "requiresApproval": false,
    "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
    "assets": []
  }
}
```

---

## 5. Carga de Memoria

### 🧠 **Nodo: HTTP Request - Load Marketing Memory**

**Hace request a:** `http://host.docker.internal:5000/api/Memory/context?tenantId=94a41b59-d900-474f-9834-c8806c6db537&userId=532b8976-25e8-4f84-953e-289cec40aebf&campaignId=ac4fe3cd-e592-4773-b60a-729e7e4f5cf4`

**Recibe del backend:**
```json
{
  "preferences": {
    "preferredTone": "profesional",
    "preferredFormats": ["post", "story"],
    "preferredChannels": ["instagram", "facebook"]
  },
  "learnings": {
    "bestPerformingChannels": ["instagram", "facebook"],
    "channelKPIs": {
      "instagram": { "ctr": 0.025, "engagement": 0.08 },
      "facebook": { "ctr": 0.018, "engagement": 0.06 }
    },
    "bestTimes": ["09:00", "13:00", "18:00"],
    "bestDays": ["lunes", "miércoles", "viernes"]
  },
  "restrictions": []
}
```

---

### 🔄 **Nodo: Normalize Memory**

**Recibe:**
```json
{
  "preferences": { ... },
  "learnings": { ... },
  "restrictions": []
}
```

**Preserva canales originales del body:**
```json
{
  "memory": {
    "preferences": { ... },
    "learnings": { ... },
    "restrictions": []
  },
  "channels": ["instagram", "facebook", "tiktok"],
  "instruction": "Crear contenido de marketing...",
  "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
  "userId": "532b8976-25e8-4f84-953e-289cec40aebf",
  "requiresApproval": false,
  "assets": []
}
```

**⚠️ IMPORTANTE:** Los canales `["instagram", "facebook", "tiktok"]` se preservan del body original, NO de la memoria.

---

## 6. Análisis con IA

### 🤖 **Nodo: OpenAI - Analyze Instruction (Cognitive)**

**Recibe:**
```json
{
  "memory": { ... },
  "channels": ["instagram", "facebook", "tiktok"],
  "instruction": "Crear contenido de marketing...",
  "preferences": { ... },
  "learnings": { ... },
  "restrictions": []
}
```

**Envía a OpenAI:**
```
System: Eres un experto analista de marketing cognitivo...
User: Analiza la siguiente instrucción:
- Instrucción: "Crear contenido de marketing para redes sociales..."
- Canales Solicitados: instagram, facebook, tiktok
- Preferencias: ...
- Aprendizajes: ...
```

**OpenAI responde:**
```json
{
  "objective": "Aumentar engagement y visibilidad de marca",
  "tone": "profesional",
  "urgency": "medium",
  "contentType": "post",
  "targetAudience": "Profesionales y empresas",
  "keyMessages": ["Engagement", "Visibilidad", "Marca"],
  "hashtags": ["marketing", "redessociales", "engagement"],
  "channels": ["instagram", "facebook", "tiktok"]
}
```

---

### 🔄 **Nodo: Parse Analysis**

**Recibe de OpenAI:**
```json
{
  "choices": [{
    "text": "{\"objective\":\"Aumentar engagement...\",\"channels\":[\"instagram\",\"facebook\",\"tiktok\"]}"
  }]
}
```

**Procesa y prioriza canales del body original:**
```json
{
  "analysis": {
    "objective": "Aumentar engagement y visibilidad de marca",
    "tone": "profesional",
    "urgency": "medium",
    "contentType": "post",
    "targetAudience": "Profesionales y empresas",
    "keyMessages": ["Engagement", "Visibilidad", "Marca"],
    "hashtags": ["marketing", "redessociales", "engagement"],
    "channels": ["instagram", "facebook", "tiktok"],
    "originalInstruction": "Crear contenido de marketing...",
    "analyzedAt": "2026-01-08T12:00:05.000Z",
    "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
    "userId": "532b8976-25e8-4f84-953e-289cec40aebf"
  },
  "channels": ["instagram", "facebook", "tiktok"],
  "memory": { ... }
}
```

**⚠️ IMPORTANTE:** Usa `normalizeMemoryData.channels` (del body) antes que `analysis.channels` (de OpenAI).

---

## 7. Generación de Estrategia

### 🤖 **Nodo: OpenAI - Generate Strategy**

**Recibe:**
```json
{
  "analysis": { ... },
  "channels": ["instagram", "facebook", "tiktok"],
  "advancedMemory": { ... }
}
```

**Envía a OpenAI:**
```
System: Eres un experto estratega de marketing...
User: Genera una estrategia basada en:
- Análisis: { ... }
- Memoria Avanzada: { ... }
- Canales Solicitados (USAR ESTOS EXACTAMENTE): instagram, facebook, tiktok

IMPORTANTE: Los canales especificados arriba son los que el usuario seleccionó. 
DEBES usar esos canales exactamente en el campo "channels" del JSON de respuesta, 
NO infieras otros canales basándote en la instrucción.
```

**OpenAI responde:**
```json
{
  "mainMessage": "Impulsa tu marca con contenido profesional y moderno",
  "cta": "Descubre más en nuestro perfil",
  "recommendedFormat": "post",
  "suggestedSchedule": {
    "bestDays": ["lunes", "miércoles", "viernes"],
    "bestTimes": ["09:00", "13:00", "18:00"],
    "timezone": "UTC"
  },
  "contentStructure": {
    "headline": "Contenido Profesional para Redes Sociales",
    "body": "Aumenta tu engagement y visibilidad...",
    "hashtags": ["marketing", "redessociales", "engagement"],
    "mentions": []
  },
  "channels": ["instagram", "facebook", "tiktok"],
  "tone": "profesional",
  "targetAudience": "Profesionales y empresas",
  "keyPoints": ["Engagement", "Visibilidad", "Marca"]
}
```

---

### 🔄 **Nodo: Parse Strategy**

**Recibe de OpenAI:**
```json
{
  "choices": [{
    "text": "{\"mainMessage\":\"Impulsa tu marca...\",\"channels\":[\"instagram\",\"facebook\",\"tiktok\"]}"
  }]
}
```

**Procesa y prioriza canales del body original:**
```json
{
  "strategy": {
    "mainMessage": "Impulsa tu marca con contenido profesional y moderno",
    "cta": "Descubre más en nuestro perfil",
    "recommendedFormat": "post",
    "suggestedSchedule": {
      "bestDays": ["lunes", "miércoles", "viernes"],
      "bestTimes": ["09:00", "13:00", "18:00"],
      "timezone": "UTC"
    },
    "contentStructure": {
      "headline": "Contenido Profesional para Redes Sociales",
      "body": "Aumenta tu engagement y visibilidad...",
      "hashtags": ["marketing", "redessociales", "engagement"],
      "mentions": []
    },
    "channels": ["instagram", "facebook", "tiktok"],
    "tone": "profesional",
    "targetAudience": "Profesionales y empresas",
    "keyPoints": ["Engagement", "Visibilidad", "Marca"],
    "metadata": {
      "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
      "generatedAt": "2026-01-08T12:00:10.000Z"
    }
  },
  "analysis": { ... },
  "channels": ["instagram", "facebook", "tiktok"]
}
```

**⚠️ IMPORTANTE:** Prioriza `$('Set Validated Data').item.json.body.channels` sobre `strategy.channels`.

---

## 8. Generación de Copy

### 🤖 **Nodo: OpenAI - Generate Copy**

**Recibe:**
```json
{
  "strategy": {
    "mainMessage": "Impulsa tu marca...",
    "channels": ["instagram", "facebook", "tiktok"],
    ...
  },
  "analysis": { ... },
  "advancedMemory": { ... }
}
```

**OpenAI responde:**
```json
{
  "shortCopy": "Impulsa tu marca con contenido profesional. Descubre más en nuestro perfil.",
  "longCopy": "Aumenta tu engagement y visibilidad de marca con contenido profesional y moderno diseñado para redes sociales. Nuestro enfoque estratégico te ayuda a conectar con tu audiencia objetivo y maximizar el impacto de tus publicaciones.",
  "hashtags": ["marketing", "redessociales", "engagement", "profesional"],
  "variants": {
    "variantA": {
      "shortCopy": "Contenido profesional para tu marca",
      "longCopy": "Eleva tu presencia en redes sociales con contenido estratégico...",
      "hashtags": ["marca", "redessociales", "estrategia"]
    },
    "variantB": {
      "shortCopy": "Maximiza tu engagement ahora",
      "longCopy": "Transforma tu estrategia de marketing digital...",
      "hashtags": ["engagement", "marketing", "digital"]
    }
  },
  "headline": "Contenido Profesional para Redes Sociales",
  "cta": "Descubre más en nuestro perfil",
  "emojiSuggestions": ["🚀", "💼", "📱"],
  "mentions": []
}
```

---

### 🔄 **Nodo: Parse Copy**

**Recibe de OpenAI:**
```json
{
  "choices": [{
    "text": "{\"shortCopy\":\"Impulsa tu marca...\",\"longCopy\":\"Aumenta tu engagement...\"}"
  }]
}
```

**Procesa y crea formatos por canal:**
```json
{
  "copy": {
    "shortCopy": "Impulsa tu marca con contenido profesional. Descubre más en nuestro perfil.",
    "longCopy": "Aumenta tu engagement y visibilidad de marca con contenido profesional y moderno diseñado para redes sociales...",
    "hashtags": ["marketing", "redessociales", "engagement", "profesional"],
    "variants": { ... },
    "headline": "Contenido Profesional para Redes Sociales",
    "cta": "Descubre más en nuestro perfil",
    "emojiSuggestions": ["🚀", "💼", "📱"],
    "mentions": [],
    "publishFormat": {
      "instagram": {
        "caption": "Aumenta tu engagement y visibilidad de marca con contenido profesional y moderno diseñado para redes sociales...\n\n#marketing #redessociales #engagement #profesional",
        "storyText": "Impulsa tu marca con contenido profesional. Descubre más en nuestro perfil.",
        "hashtags": ["marketing", "redessociales", "engagement", "profesional"]
      },
      "facebook": {
        "post": "Aumenta tu engagement y visibilidad de marca con contenido profesional y moderno diseñado para redes sociales...",
        "hashtags": ["marketing", "redessociales", "engagement", "profesional"]
      },
      "twitter": {
        "tweet": "Impulsa tu marca con contenido profesional. Descubre más en nuestro perfil.",
        "thread": "Aumenta tu engagement y visibilidad de marca con contenido profesional y moderno diseñado para redes sociales...",
        "hashtags": ["marketing", "redessociales", "engagement", "profesional"]
      }
    },
    "metadata": {
      "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
      "generatedAt": "2026-01-08T12:00:15.000Z",
      "format": "post",
      "channels": ["instagram", "facebook", "tiktok"],
      "tone": "profesional"
    }
  },
  "strategy": { ... },
  "channels": ["instagram", "facebook", "tiktok"]
}
```

---

## 9. Generación de Prompts Visuales

### 🤖 **Nodo: OpenAI - Generate Visual Prompts**

**Recibe:**
```json
{
  "strategy": { ... },
  "copy": { ... },
  "advancedMemory": { ... }
}
```

**OpenAI responde:**
```json
{
  "imagePrompt": "High-quality marketing image showing professional and modern social media content strategy. Clean, vibrant design with professional color palette. Focus on engagement and brand visibility.",
  "videoPrompt": "Professional marketing video showcasing social media content strategy. Smooth transitions, modern aesthetics, professional tone. Emphasize engagement and brand visibility.",
  "imageStyle": "professional",
  "colorPalette": ["azul corporativo", "gris", "blanco"],
  "mood": "professional",
  "aspectRatio": "1:1",
  "technicalSpecs": {
    "resolution": "1080p",
    "quality": "alta",
    "lighting": "luz de día",
    "composition": "enfoque en el contenido"
  }
}
```

---

### 🔄 **Nodo: Parse Visual Prompts**

**Recibe de OpenAI:**
```json
{
  "choices": [{
    "text": "{\"imagePrompt\":\"High-quality marketing image...\",\"videoPrompt\":\"Professional marketing video...\"}"
  }]
}
```

**Procesa:**
```json
{
  "visualPrompts": {
    "imagePrompt": "High-quality marketing image showing professional and modern social media content strategy...",
    "videoPrompt": "Professional marketing video showcasing social media content strategy...",
    "imageStyle": "professional",
    "colorPalette": ["azul corporativo", "gris", "blanco"],
    "mood": "professional",
    "aspectRatio": "1:1",
    "technicalSpecs": {
      "resolution": "1080p",
      "quality": "alta",
      "lighting": "luz de día",
      "composition": "enfoque en el contenido"
    },
    "metadata": {
      "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
      "generatedAt": "2026-01-08T12:00:20.000Z",
      "format": "post",
      "channels": ["instagram", "facebook", "tiktok"],
      "tone": "profesional"
    }
  },
  "copy": { ... },
  "strategy": { ... },
  "channels": ["instagram", "facebook", "tiktok"]
}
```

---

## 10. Motor de Decisiones Cognitivo

### 🧠 **Nodo: Cognitive Decision Engine**

**Recibe:**
```json
{
  "strategy": {
    "channels": ["instagram", "facebook", "tiktok"],
    "recommendedFormat": "post",
    "tone": "profesional",
    ...
  },
  "analysis": {
    "urgency": "medium",
    ...
  },
  "advancedMemory": {
    "performanceMemory": {
      "bestPerformingChannels": ["instagram", "facebook"],
      "channelKPIs": {
        "instagram": { "ctr": 0.025, "engagement": 0.08 },
        "facebook": { "ctr": 0.018, "engagement": 0.06 }
      }
    },
    "patternMemory": {
      "successfulPatterns": [...],
      "failedPatterns": [...],
      "blockedPatterns": []
    }
  },
  "confidenceWeights": { ... }
}
```

**Calcula confidenceScore:**
- Factor 1 (30%): Canales con mejor performance
  - Instagram: CTR 0.025, Engagement 0.08 → Peso alto
  - Facebook: CTR 0.018, Engagement 0.06 → Peso medio
  - TikTok: Sin historial → Peso neutro
- Factor 2 (20%): Formato apropiado para urgencia
- Factor 3 (20%): Tono con mayor engagement
- Factor 4 (15%): Evitar patrones fallidos
- Factor 5 (10%): Preferencias del tenant
- Factor 6 (5%): Restricciones cumplidas

**Resultado:**
```json
{
  "cognitiveDecision": {
    "confidenceScore": 0.75,
    "adaptiveTemperature": 0.7,
    "shouldReduceVariants": false,
    "decisionRationale": "Canales seleccionados tienen alta performance histórica (75%); Formato post es exitoso para urgencia medium (70%); Tono profesional tiene alto engagement histórico (72%); Patrón no está en lista de fallos históricos",
    "learningSources": ["PerformanceMemory", "PatternMemory", "PreferenceMemory"],
    "cognitiveVersion": 2,
    "channelConfidence": 0.75,
    "formatConfidence": 0.70,
    "toneConfidence": 0.72,
    "patternViolations": 0,
    "calculatedAt": "2026-01-08T12:00:25.000Z"
  },
  "strategy": { ... },
  "copy": { ... },
  "visualPrompts": { ... },
  "channels": ["instagram", "facebook", "tiktok"]
}
```

---

## 11. Construcción del Marketing Pack

### 📦 **Nodo: Build Marketing Pack**

**Recibe:**
```json
{
  "strategy": { ... },
  "copy": { ... },
  "visualPrompts": { ... },
  "cognitiveDecision": {
    "confidenceScore": 0.75,
    "cognitiveVersion": 2,
    ...
  },
  "channels": ["instagram", "facebook", "tiktok"]
}
```

**Obtiene datos del body original:**
```javascript
const originalChannels = $('Set Validated Data').item?.json?.body?.channels || [];
// Resultado: ["instagram", "facebook", "tiktok"]

const tenantId = $('Set Validated Data').item?.json?.body?.tenantId;
// Resultado: "94a41b59-d900-474f-9834-c8806c6db537"

const userId = $('Set Validated Data').item?.json?.body?.userId;
// Resultado: "532b8976-25e8-4f84-953e-289cec40aebf"

const campaignId = $('Set Validated Data').item?.json?.body?.campaignId;
// Resultado: "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4"

const requiresApproval = $('Set Validated Data').item?.json?.body?.requiresApproval;
// Resultado: false

const assets = $('Set Validated Data').item?.json?.body?.assets || [];
// Resultado: []
```

**Construye GeneratedCopies:**
```json
{
  "generatedCopies": [
    {
      "id": "uuid-long-copy",
      "copyType": "long",
      "content": "Aumenta tu engagement y visibilidad de marca...",
      "hashtags": "marketing, redessociales, engagement, profesional",
      "suggestedChannel": "instagram",
      "publicationChecklist": {
        "hasCopy": true,
        "hasHashtags": true,
        "hasMedia": false,
        "readyForPublication": true
      }
    },
    {
      "id": "uuid-short-copy",
      "copyType": "short",
      "content": "Impulsa tu marca con contenido profesional...",
      "hashtags": "marketing, redessociales, engagement, profesional",
      "suggestedChannel": "instagram",
      "publicationChecklist": { ... }
    },
    {
      "id": "uuid-variant-a",
      "copyType": "variant-a",
      "content": "Eleva tu presencia en redes sociales...",
      "hashtags": "marca, redessociales, estrategia",
      "suggestedChannel": "instagram",
      "publicationChecklist": { ... }
    },
    {
      "id": "uuid-variant-b",
      "copyType": "variant-b",
      "content": "Transforma tu estrategia de marketing digital...",
      "hashtags": "engagement, marketing, digital",
      "suggestedChannel": "instagram",
      "publicationChecklist": { ... }
    }
  ]
}
```

**Construye MarketingAssetPrompts:**
```json
{
  "assetPrompts": [
    {
      "id": "uuid-image-prompt",
      "assetType": "image",
      "prompt": "High-quality marketing image showing professional and modern social media content strategy...",
      "negativePrompt": null,
      "parameters": {
        "style": "professional",
        "aspectRatio": "1:1",
        "colorPalette": ["azul corporativo", "gris", "blanco"],
        "mood": "professional",
        "resolution": "1080p",
        "quality": "alta",
        "lighting": "luz de día",
        "composition": "enfoque en el contenido"
      },
      "suggestedChannel": "instagram"
    },
    {
      "id": "uuid-video-prompt",
      "assetType": "video",
      "prompt": "Professional marketing video showcasing social media content strategy...",
      "negativePrompt": null,
      "parameters": {
        "aspectRatio": "16:9",
        "colorPalette": ["azul corporativo", "gris", "blanco"],
        "mood": "professional",
        "resolution": "1080p",
        "quality": "alta",
        "lighting": "luz de día",
        "composition": "enfoque en el contenido"
      },
      "suggestedChannel": "instagram"
    }
  ]
}
```

**Construye MarketingPack final:**
```json
{
  "marketingPack": {
    "id": "uuid-marketing-pack",
    "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
    "userId": "532b8976-25e8-4f84-953e-289cec40aebf",
    "contentId": "uuid-content-id",
    "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
    "strategy": "{\"mainMessage\":\"Impulsa tu marca...\",\"channels\":[\"instagram\",\"facebook\",\"tiktok\"]}",
    "status": "Ready",
    "version": 1,
    "metadata": "{\"tenantId\":\"94a41b59-d900-474f-9834-c8806c6db537\",\"channels\":[\"instagram\",\"facebook\",\"tiktok\"],\"requiresApproval\":false,...}",
    "copies": [ ... ],
    "assetPrompts": [ ... ],
    "channels": ["instagram", "facebook", "tiktok"],
    "media": [],
    "requiresApproval": false,
    "createdAt": "2026-01-08T12:00:30.000Z",
    "cognitiveVersion": 2,
    "confidenceScore": 0.75,
    "learningSources": ["PerformanceMemory", "PatternMemory", "PreferenceMemory"],
    "decisionRationale": "Canales seleccionados tienen alta performance histórica..."
  },
  "id": "uuid-marketing-pack",
  "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
  "userId": "532b8976-25e8-4f84-953e-289cec40aebf",
  "contentId": "uuid-content-id",
  "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
  "strategy": "...",
  "status": "Ready",
  "version": 1,
  "metadata": "...",
  "copies": [ ... ],
  "assetPrompts": [ ... ]
}
```

**⚠️ IMPORTANTE:** 
- `channels` viene del body original: `["instagram", "facebook", "tiktok"]`
- `status` es `"Ready"` porque `requiresApproval: false`
- `requiresApproval: false` viene del body original

---

## 12. Preparación de Jobs de Publicación

### 📝 **Nodo: Prepare Publish Jobs**

**Recibe:**
```json
{
  "marketingPack": {
    "channels": ["instagram", "facebook", "tiktok"],
    "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
    "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
    "id": "uuid-marketing-pack",
    "copies": [ ... ],
    "media": []
  },
  "copy": {
    "publishFormat": {
      "instagram": {
        "caption": "Aumenta tu engagement...\n\n#marketing #redessociales...",
        "storyText": "Impulsa tu marca...",
        "hashtags": ["marketing", "redessociales", "engagement", "profesional"]
      },
      "facebook": {
        "post": "Aumenta tu engagement y visibilidad de marca...",
        "hashtags": ["marketing", "redessociales", "engagement", "profesional"]
      },
      "twitter": {
        "tweet": "Impulsa tu marca...",
        "thread": "Aumenta tu engagement...",
        "hashtags": ["marketing", "redessociales", "engagement", "profesional"]
      }
    }
  }
}
```

**Procesa cada canal dinámicamente:**
```javascript
channels.forEach(channel => {
  // channel = "instagram"
  // channel = "facebook"
  // channel = "tiktok"
  
  const channelCopy = copy.publishFormat?.[channel.toLowerCase()] || copy.longCopy || '';
  const hashtags = copy.hashtags || [];
  const hashtagsString = hashtags.map(h => '#' + h.replace(/^#/, '')).join(' ');
  
  publishJobs.push({
    channel: channel.toLowerCase(),
    content: channelCopy,
    hashtags: hashtagsString,
    mediaUrl: marketingPack.media && marketingPack.media.length > 0 ? marketingPack.media[0] : null,
    tenantId: marketingPack.tenantId,
    campaignId: marketingPack.campaignId,
    marketingPackId: marketingPack.id,
    generatedCopyId: marketingPack.copies && marketingPack.copies.length > 0 ? marketingPack.copies[0].id : null
  });
});
```

**Crea 3 jobs (uno por cada canal):**

**Job 1 - Instagram:**
```json
{
  "channel": "instagram",
  "content": {
    "caption": "Aumenta tu engagement y visibilidad de marca con contenido profesional y moderno diseñado para redes sociales...\n\n#marketing #redessociales #engagement #profesional",
    "storyText": "Impulsa tu marca con contenido profesional. Descubre más en nuestro perfil.",
    "hashtags": ["marketing", "redessociales", "engagement", "profesional"]
  },
  "hashtags": "#marketing #redessociales #engagement #profesional",
  "mediaUrl": null,
  "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
  "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
  "marketingPackId": "uuid-marketing-pack",
  "generatedCopyId": "uuid-long-copy"
}
```

**Job 2 - Facebook:**
```json
{
  "channel": "facebook",
  "content": {
    "post": "Aumenta tu engagement y visibilidad de marca con contenido profesional y moderno diseñado para redes sociales...",
    "hashtags": ["marketing", "redessociales", "engagement", "profesional"]
  },
  "hashtags": "#marketing #redessociales #engagement #profesional",
  "mediaUrl": null,
  "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
  "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
  "marketingPackId": "uuid-marketing-pack",
  "generatedCopyId": "uuid-long-copy"
}
```

**Job 3 - TikTok:**
```json
{
  "channel": "tiktok",
  "content": "Aumenta tu engagement y visibilidad de marca con contenido profesional y moderno diseñado para redes sociales...",
  "hashtags": "#marketing #redessociales #engagement #profesional",
  "mediaUrl": null,
  "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
  "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
  "marketingPackId": "uuid-marketing-pack",
  "generatedCopyId": "uuid-long-copy"
}
```

**Retorna:** Array con 3 items (uno por cada canal)

---

## 13. Publicación por Canal

### 🔍 **Nodo: Check - Instagram**

**Recibe Job 1:**
```json
{
  "channel": "instagram",
  "content": { ... },
  "hashtags": "#marketing #redessociales #engagement #profesional",
  ...
}
```

**Evalúa:** `$json.channel.toLowerCase() === "instagram"` → `true`

**Ruta TRUE → Publish - Instagram**

---

### 📤 **Nodo: Publish - Instagram**

**Recibe:**
```json
{
  "channel": "instagram",
  "content": { ... },
  "hashtags": "#marketing #redessociales #engagement #profesional",
  ...
}
```

**Hace POST a:** `https://api.instagram.com/v1/media`

**Headers:**
```json
{
  "X-Publish-Data": "{\"channel\":\"instagram\",\"content\":{...},\"hashtags\":\"#marketing #redessociales...\",\"tenantId\":\"94a41b59-d900-474f-9834-c8806c6db537\",\"campaignId\":\"ac4fe3cd-e592-4773-b60a-729e7e4f5cf4\",\"marketingPackId\":\"uuid-marketing-pack\",\"generatedCopyId\":\"uuid-long-copy\"}"
}
```

**Respuesta (simulada - sin credenciales):**
```json
{
  "error": "No credentials configured",
  "statusCode": 401
}
```

---

### 🔍 **Nodo: Check - Facebook**

**Recibe Job 2:**
```json
{
  "channel": "facebook",
  "content": { ... },
  "hashtags": "#marketing #redessociales #engagement #profesional",
  ...
}
```

**Evalúa:** `$json.channel.toLowerCase() === "facebook"` → `true`

**Ruta TRUE → Publish - Facebook**

---

### 📤 **Nodo: Publish - Facebook**

**Recibe:**
```json
{
  "channel": "facebook",
  "content": { ... },
  "hashtags": "#marketing #redessociales #engagement #profesional",
  ...
}
```

**Hace POST a:** `https://graph.facebook.com/v18.0/me/feed`

**Respuesta (simulada - sin credenciales):**
```json
{
  "error": "No credentials configured",
  "statusCode": 401
}
```

---

### 🔍 **Nodo: Check - TikTok**

**Recibe Job 3:**
```json
{
  "channel": "tiktok",
  "content": "Aumenta tu engagement...",
  "hashtags": "#marketing #redessociales #engagement #profesional",
  ...
}
```

**Evalúa:** `$json.channel.toLowerCase() === "tiktok"` → `true`

**Ruta TRUE → Publish - TikTok**

---

### 📤 **Nodo: Publish - TikTok**

**Recibe:**
```json
{
  "channel": "tiktok",
  "content": "Aumenta tu engagement...",
  "hashtags": "#marketing #redessociales #engagement #profesional",
  ...
}
```

**Hace POST a:** `https://open-api.tiktok.com/video/upload`

**Respuesta (simulada - sin credenciales):**
```json
{
  "error": "No credentials configured",
  "statusCode": 401
}
```

---

## 14. Procesamiento de Resultados

### 🔄 **Nodo: Process Publish Result**

**Recibe (de cada nodo de publicación):**
```json
{
  "error": "No credentials configured",
  "statusCode": 401,
  "channel": "instagram",
  "content": { ... },
  "hashtags": "#marketing #redessociales #engagement #profesional",
  "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
  "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
  "marketingPackId": "uuid-marketing-pack",
  "generatedCopyId": "uuid-long-copy"
}
```

**Detecta:** `isSimulated = true` (porque no hay `id`, `post_id`, `data`, `success`)

**Genera respuesta simulada:**
```json
{
  "success": true,
  "simulated": true,
  "channel": "instagram",
  "postId": "sim_1704715230000_abc123def",
  "publishedUrl": "https://instagram.com/posts/sim_1704715230000",
  "publishedAt": "2026-01-08T12:00:35.000Z",
  "message": "Publication simulated for instagram (no credentials configured)",
  "content": { ... },
  "hashtags": "#marketing #redessociales #engagement #profesional",
  "mediaUrl": null,
  "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
  "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
  "marketingPackId": "uuid-marketing-pack",
  "generatedCopyId": "uuid-long-copy"
}
```

**Se ejecuta 3 veces (una por cada canal):**

**Resultado 1 - Instagram:**
```json
{
  "success": true,
  "simulated": true,
  "channel": "instagram",
  "postId": "sim_1704715230000_abc123def",
  "publishedUrl": "https://instagram.com/posts/sim_1704715230000",
  ...
}
```

**Resultado 2 - Facebook:**
```json
{
  "success": true,
  "simulated": true,
  "channel": "facebook",
  "postId": "sim_1704715230001_xyz789ghi",
  "publishedUrl": "https://facebook.com/posts/sim_1704715230001",
  ...
}
```

**Resultado 3 - TikTok:**
```json
{
  "success": true,
  "simulated": true,
  "channel": "tiktok",
  "postId": "sim_1704715230002_mno456jkl",
  "publishedUrl": "https://tiktok.com/posts/sim_1704715230002",
  ...
}
```

---

### 💾 **Nodo: HTTP Request - Save Publishing Job**

**Recibe cada resultado y guarda en backend:**
```json
{
  "success": true,
  "simulated": true,
  "channel": "instagram",
  "postId": "sim_1704715230000_abc123def",
  "publishedUrl": "https://instagram.com/posts/sim_1704715230000",
  "publishedAt": "2026-01-08T12:00:35.000Z",
  "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
  "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
  "marketingPackId": "uuid-marketing-pack",
  "generatedCopyId": "uuid-long-copy"
}
```

**POST a:** `http://localhost:5000/api/publishing-jobs`

**Respuesta del backend:**
```json
{
  "id": "uuid-publishing-job-instagram",
  "channel": "instagram",
  "status": "Success",
  "publishedUrl": "https://instagram.com/posts/sim_1704715230000",
  "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
  "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
  "marketingPackId": "uuid-marketing-pack",
  "createdAt": "2026-01-08T12:00:36.000Z"
}
```

**Se ejecuta 3 veces (una por cada canal)**

---

### 📊 **Nodo: Consolidate Publish Results**

**Recibe los 3 resultados del backend:**
```json
[
  {
    "id": "uuid-publishing-job-instagram",
    "channel": "instagram",
    "status": "Success",
    "publishedUrl": "https://instagram.com/posts/sim_1704715230000"
  },
  {
    "id": "uuid-publishing-job-facebook",
    "channel": "facebook",
    "status": "Success",
    "publishedUrl": "https://facebook.com/posts/sim_1704715230001"
  },
  {
    "id": "uuid-publishing-job-tiktok",
    "channel": "tiktok",
    "status": "Success",
    "publishedUrl": "https://tiktok.com/posts/sim_1704715230002"
  }
]
```

**Consolida:**
```json
{
  "tenantId": "94a41b59-d900-474f-9834-c8806c6db537",
  "campaignId": "ac4fe3cd-e592-4773-b60a-729e7e4f5cf4",
  "marketingPackId": "uuid-marketing-pack",
  "publishingJobIds": [
    "uuid-publishing-job-instagram",
    "uuid-publishing-job-facebook",
    "uuid-publishing-job-tiktok"
  ],
  "publishingJobs": [
    {
      "id": "uuid-publishing-job-instagram",
      "channel": "instagram",
      "status": "Success",
      "publishedUrl": "https://instagram.com/posts/sim_1704715230000"
    },
    {
      "id": "uuid-publishing-job-facebook",
      "channel": "facebook",
      "status": "Success",
      "publishedUrl": "https://facebook.com/posts/sim_1704715230001"
    },
    {
      "id": "uuid-publishing-job-tiktok",
      "channel": "tiktok",
      "status": "Success",
      "publishedUrl": "https://tiktok.com/posts/sim_1704715230002"
    }
  ],
  "channels": ["instagram", "facebook", "tiktok"],
  "allPublished": true,
  "success": true
}
```

---

## 📝 Resumen del Flujo por Canal

### Para cada canal en `["instagram", "facebook", "tiktok"]`:

1. **Prepare Publish Jobs** crea un job individual
2. **Check - [Canal]** evalúa si coincide
3. **Publish - [Canal]** intenta publicar (o simula si no hay credenciales)
4. **Process Publish Result** procesa el resultado
5. **HTTP Request - Save Publishing Job** guarda en backend
6. **Consolidate Publish Results** consolida todos los resultados

### Resultado Final:
- ✅ 3 jobs de publicación creados
- ✅ 3 publicaciones simuladas (sin credenciales)
- ✅ 3 jobs guardados en backend
- ✅ Todos los canales procesados dinámicamente

---

## 🔄 Escenarios con Diferentes Cantidades de Canales

### Escenario 1: 1 Canal
```json
"channels": ["instagram"]
```
- **Prepare Publish Jobs** crea 1 job
- **Check - Instagram** → TRUE → **Publish - Instagram**
- **Resultado:** 1 publicación

### Escenario 2: 5 Canales
```json
"channels": ["instagram", "facebook", "tiktok", "linkedin", "twitter"]
```
- **Prepare Publish Jobs** crea 5 jobs
- **Check - Instagram** → TRUE → **Publish - Instagram**
- **Check - Facebook** → TRUE → **Publish - Facebook**
- **Check - TikTok** → TRUE → **Publish - TikTok**
- **Check - LinkedIn** → FALSE → **Process Publish Result** (simulado)
- **Check - Twitter** → FALSE → **Process Publish Result** (simulado)
- **Resultado:** 5 publicaciones (3 específicas + 2 simuladas)

### Escenario 3: 10 Canales
```json
"channels": ["instagram", "facebook", "tiktok", "linkedin", "twitter", "youtube", "pinterest", "snapchat", "telegram", "whatsapp"]
```
- **Prepare Publish Jobs** crea 10 jobs dinámicamente
- Solo Instagram, Facebook y TikTok tienen nodos específicos
- Los otros 7 canales pasan a **Process Publish Result** (simulados)
- **Resultado:** 10 publicaciones (3 específicas + 7 simuladas)

---

## ✅ Puntos Clave del Flujo

1. **Canales se preservan del body original** en todos los nodos
2. **Procesamiento dinámico** con `forEach` - sin límites hardcodeados
3. **Cada canal se procesa independientemente**
4. **Publicación simulada** para canales sin credenciales configuradas
5. **Todos los resultados se consolidan** al final
6. **Sin valores hardcodeados** - todo viene del payload o de OpenAI

---

## 🎯 Conclusión

El workflow está completamente preparado para:
- ✅ Recibir cualquier cantidad de canales (1, 3, 5, 10, etc.)
- ✅ Procesar cada canal dinámicamente
- ✅ Preservar los canales originales del payload
- ✅ Simular publicaciones cuando no hay credenciales
- ✅ Consolidar todos los resultados al final

**Todo el flujo es dinámico y no tiene límites hardcodeados.**

