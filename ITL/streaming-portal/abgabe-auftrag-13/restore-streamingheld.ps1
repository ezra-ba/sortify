<#
    Stellt ein Backup in einer getrennten Testdatenbank wieder her.
    Die Produktionsdatenbank wird standardmäßig nicht verwendet.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$BackupFile,
    [string]$Database = 'streamingheld_test',
    [string]$MySql = 'C:\xampp\mysql\bin\mysql.exe',
    [string]$User = 'root'
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $MySql)) {
    throw "mysql nicht gefunden: $MySql"
}

if (-not (Test-Path -LiteralPath $BackupFile)) {
    throw "Backup-Datei nicht gefunden: $BackupFile"
}

# Nur sichere Zeichen im Datenbanknamen zulassen
$safeDatabase = $Database -replace '[^A-Za-z0-9_]', ''
if ([string]::IsNullOrWhiteSpace($safeDatabase)) {
    throw 'Ungültiger Datenbankname.'
}

# Prüfsumme prüfen, falls eine Prüfsummendatei vorhanden ist
$checksumFile = "$BackupFile.sha256"
if (Test-Path -LiteralPath $checksumFile) {
    $expectedChecksum = ((Get-Content -Raw -LiteralPath $checksumFile) -split '\s+')[0]
    $actualChecksum = (Get-FileHash -Algorithm SHA256 -LiteralPath $BackupFile).Hash

    if ($expectedChecksum -ne $actualChecksum) {
        throw 'Integritätsprüfung fehlgeschlagen.'
    }
}

# Testdatenbank anlegen und Backup importieren
$createDatabase = "CREATE DATABASE IF NOT EXISTS `$safeDatabase` CHARACTER SET utf8mb4;"
& $MySql "--user=$User" -e $createDatabase
if ($LASTEXITCODE -ne 0) {
    throw 'Testdatenbank konnte nicht angelegt werden.'
}

Get-Content -Raw -LiteralPath $BackupFile | & $MySql "--user=$User" $safeDatabase
if ($LASTEXITCODE -ne 0) {
    throw "Restore fehlgeschlagen (Exit-Code $LASTEXITCODE)."
}

Write-Host "Restore erfolgreich nach $safeDatabase"
