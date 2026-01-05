# Configuración de Variables de Entorno para n8n

## Variables Requeridas

Para que el workflow funcione correctamente, necesitas configurar las siguientes variables de entorno en tu instancia de n8n:

### Variables de n8n (Configuración del Host)

```bash
N8N_HOST=n8n.bashpty.com
N8N_PROTOCOL=https
N8N_PORT=443
WEBHOOK_URL=https://n8n.bashpty.com
N8N_EDITOR_BASE_URL=https://n8n.bashpty.com
NODE_ENV=production
```

### Variables de Backend API

```bash
# URL base del backend de Autonomous Marketing Platform
BACKEND_URL=https://autonomousmarketingplatform.onrender.com
```

**Nota:** Si `BACKEND_URL` no está configurada o es `http://localhost:5000`, el workflow usará `https://autonomousmarketingplatform.onrender.com` como fallback.

### Variables Opcionales para Publicación

```bash
# URLs de APIs de redes sociales (opcionales)
INSTAGRAM_API_URL=https://api.instagram.com/v1/media
FACEBOOK_API_URL=https://graph.facebook.com/v18.0/me/feed
TIKTOK_API_URL=https://open-api.tiktok.com/video/upload

# Modelo de OpenAI (opcional, por defecto usa gpt-4)
OPENAI_MODEL=gpt-4
```

## Cómo Configurar Variables de Entorno en n8n

### Opción 1: Variables de Entorno del Sistema

Si estás ejecutando n8n con Docker o directamente, configura las variables en tu archivo `.env` o en el sistema:

```bash
# .env file
N8N_HOST=n8n.bashpty.com
N8N_PROTOCOL=https
N8N_PORT=443
WEBHOOK_URL=https://n8n.bashpty.com
N8N_EDITOR_BASE_URL=https://n8n.bashpty.com
NODE_ENV=production
BACKEND_URL=https://autonomousmarketingplatform.onrender.com
```

### Opción 2: Variables de Entorno en n8n (Recomendado)

1. Ve a **Settings** → **Environment Variables** en tu instancia de n8n
2. Agrega las variables necesarias:
   - `BACKEND_URL`: URL de tu backend API
   - `INSTAGRAM_API_URL`: (Opcional) URL de API de Instagram
   - `FACEBOOK_API_URL`: (Opcional) URL de API de Facebook
   - `TIKTOK_API_URL`: (Opcional) URL de API de TikTok
   - `OPENAI_MODEL`: (Opcional) Modelo de OpenAI a usar

## URL del Webhook

El webhook del workflow está disponible en:

```
https://n8n.bashpty.com/webhook/marketing-request
```

## Verificación de Configuración

Para verificar que las variables están configuradas correctamente:

1. Abre el workflow en n8n
2. Revisa cualquier nodo HTTP Request
3. Las URLs deberían mostrar expresiones como:
   ```
   {{ ($env.BACKEND_URL && $env.BACKEND_URL !== 'http://localhost:5000') ? $env.BACKEND_URL : 'https://autonomousmarketingplatform.onrender.com' }}/api/...
   ```

## Solución de Problemas

### Error: "Cannot connect to backend"

1. Verifica que `BACKEND_URL` esté configurada correctamente
2. Verifica que el backend esté accesible desde tu instancia de n8n
3. Revisa los logs de n8n para ver errores de conexión

### Error: "Webhook not found"

1. Verifica que `WEBHOOK_URL` esté configurada correctamente
2. Verifica que el workflow esté activo
3. Verifica que el path del webhook sea correcto: `/webhook/marketing-request`

### Error: "Environment variable not found"

1. Asegúrate de que las variables estén configuradas en n8n
2. Reinicia n8n después de agregar nuevas variables
3. Verifica que los nombres de las variables sean exactos (case-sensitive)

## Notas Importantes

- Las variables de entorno son **case-sensitive**
- Después de cambiar variables de entorno, **reinicia n8n**
- El workflow usa fallbacks seguros si las variables no están configuradas
- Todas las URLs del backend ahora usan `$env.BACKEND_URL` en lugar de URLs hardcodeadas

