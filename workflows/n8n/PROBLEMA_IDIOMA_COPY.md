# Problema: Copy Generado en Inglés

## 🔍 Análisis del Problema

### **Qué hace el nodo "OpenAI - Generate Copy":**

Este nodo **genera el copy de marketing** (textos cortos y largos) que se publicará en redes sociales.

**Función:**
- Genera `shortCopy` (para stories, tweets)
- Genera `longCopy` (para posts)
- Genera variantes A/B para testing
- Genera headlines, CTAs, hashtags, emojis

### **Por qué está generando en inglés:**

1. **El prompt NO especifica idioma:**
   ```
   'Eres un copywriter experto de marketing. 
   Tu tarea es generar copy de marketing estructurado...'
   ```
   - El prompt está en español pero NO dice "genera en español"

2. **La estrategia que recibe ya está en inglés:**
   ```json
   {
     "mainMessage": "Increase engagement and visibility with our brand!",
     "cta": "Engage with us now",
     "headline": "Boost Your Engagement with Our Brand"
   }
   ```
   - OpenAI ve inglés en la estrategia y genera en inglés

3. **No hay referencia al idioma original:**
   - El nodo recibe `strategy`, `analysis`, `advancedMemory`
   - Pero no recibe la instrucción original en español
   - OpenAI no sabe que debe generar en español

---

## ✅ **Solución: Especificar Idioma en el Prompt**

Necesito agregar instrucciones explícitas para que el copy se genere en español.

