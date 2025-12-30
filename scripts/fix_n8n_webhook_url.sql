-- Script para corregir la URL del webhook de Marketing Request
-- Este script actualiza la configuración para TODOS los tenants activos

-- Primero, verificar la configuración actual
SELECT 
    "Id",
    "TenantId",
    "BaseUrl",
    "DefaultWebhookUrl",
    "WebhookUrlsJson",
    "IsActive"
FROM "TenantN8nConfigs"
WHERE "IsActive" = true;

-- Actualizar WebhookUrlsJson para incluir MarketingRequest
UPDATE "TenantN8nConfigs"
SET 
    "WebhookUrlsJson" = jsonb_set(
        COALESCE("WebhookUrlsJson"::jsonb, '{}'::jsonb),
        '{MarketingRequest}',
        '"https://n8n.bashpty.com/webhook-test/marketing-request"'::jsonb
    )::text,
    "UpdatedAt" = NOW()
WHERE "IsActive" = true;

-- Verificar que se actualizó correctamente
SELECT 
    "TenantId",
    "WebhookUrlsJson"::jsonb->>'MarketingRequest' as "MarketingRequest URL",
    "DefaultWebhookUrl",
    "IsActive"
FROM "TenantN8nConfigs"
WHERE "IsActive" = true;

