-- Script SQL para crear la tabla ApplicationLogs en PostgreSQL
-- Ejecutar este script directamente en Render usando PSQL o el cliente de PostgreSQL

CREATE TABLE IF NOT EXISTS "ApplicationLogs" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Level" VARCHAR(50) NOT NULL,
    "Message" TEXT NOT NULL,
    "Source" VARCHAR(255) NOT NULL,
    "TenantId" UUID,
    "UserId" UUID,
    "StackTrace" TEXT,
    "ExceptionType" VARCHAR(500),
    "InnerException" TEXT,
    "RequestId" VARCHAR(255),
    "Path" VARCHAR(500),
    "HttpMethod" VARCHAR(10),
    "AdditionalData" TEXT,
    "IpAddress" VARCHAR(50),
    "UserAgent" TEXT,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP,
    "IsActive" BOOLEAN NOT NULL DEFAULT true
);

-- Crear índices para mejorar el rendimiento de consultas
CREATE INDEX IF NOT EXISTS "IX_ApplicationLogs_Level" ON "ApplicationLogs" ("Level");
CREATE INDEX IF NOT EXISTS "IX_ApplicationLogs_TenantId" ON "ApplicationLogs" ("TenantId");
CREATE INDEX IF NOT EXISTS "IX_ApplicationLogs_UserId" ON "ApplicationLogs" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_ApplicationLogs_CreatedAt" ON "ApplicationLogs" ("CreatedAt" DESC);
CREATE INDEX IF NOT EXISTS "IX_ApplicationLogs_Source" ON "ApplicationLogs" ("Source");
CREATE INDEX IF NOT EXISTS "IX_ApplicationLogs_RequestId" ON "ApplicationLogs" ("RequestId");

-- Comentarios para documentación
COMMENT ON TABLE "ApplicationLogs" IS 'Tabla para persistir logs de aplicación en la base de datos';
COMMENT ON COLUMN "ApplicationLogs"."Level" IS 'Nivel del log: Error, Warning, Information, Debug, Critical';
COMMENT ON COLUMN "ApplicationLogs"."Source" IS 'Origen del log (ej: AccountController, TenantResolver)';
COMMENT ON COLUMN "ApplicationLogs"."TenantId" IS 'Identificador del tenant (opcional)';
COMMENT ON COLUMN "ApplicationLogs"."UserId" IS 'Identificador del usuario (opcional)';

