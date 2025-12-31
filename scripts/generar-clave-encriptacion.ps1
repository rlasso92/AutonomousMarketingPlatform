# Script para generar una clave de encriptación de 32 caracteres
# Uso: .\scripts\generar-clave-encriptacion.ps1

Write-Host "🔐 Generando clave de encriptación de 32 caracteres..." -ForegroundColor Cyan
Write-Host ""

# Generar clave aleatoria de 32 caracteres (alfanumérica)
$chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
$key = ""
for ($i = 0; $i -lt 32; $i++) {
    $key += $chars[(Get-Random -Maximum $chars.Length)]
}

Write-Host "✅ Clave generada:" -ForegroundColor Green
Write-Host ""
Write-Host $key -ForegroundColor Yellow
Write-Host ""
Write-Host "📋 Copia esta clave y úsala como valor de 'Encryption__Key' en Render" -ForegroundColor Cyan
Write-Host ""
Write-Host "⚠️  IMPORTANTE: Guarda esta clave en un lugar seguro. No la compartas." -ForegroundColor Red
Write-Host ""

# Copiar al portapapeles si está disponible
try {
    $key | Set-Clipboard
    Write-Host "✅ Clave copiada al portapapeles" -ForegroundColor Green
} catch {
    Write-Host "⚠️  No se pudo copiar al portapapeles. Cópiala manualmente." -ForegroundColor Yellow
}

