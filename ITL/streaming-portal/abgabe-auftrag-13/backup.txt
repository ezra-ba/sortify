<#
    Erstellt ein konsistentes MariaDB-Backup mit Zeitstempel.
    Zugangsdaten werden nicht im Skript gespeichert.
#>

param(
    [string]$BackupRoot = "$PSScriptRoot\backups",
    [string]$Database = 'streamingheld',
    [string]$MySqlDump = 'C:\xampp\mysql\bin\mysqldump.exe',
    [string]$User = 'root'
)

$ErrorActionPreference = 'Stop'

# Backup-Ordner und Dateinamen vorbereiten
$timestamp = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'
$monthFolder = Get-Date -Format 'yyyy-MM'
$targetDirectory = Join-Path $BackupRoot $monthFolder
$backupFile = Join-Path $targetDirectory "${Database}_$timestamp.sql"
$checksumFile = "$backupFile.sha256"

New-Item -ItemType Directory -Force -Path $targetDirectory | Out-Null

if (-not (Test-Path -LiteralPath $MySqlDump)) {
    throw "mysqldump nicht gefunden: $MySqlDump"
}

# Datenbank konsistent exportieren
& $MySqlDump `
    "--user=$User" `
    '--single-transaction' `
    '--routines' `
    '--events' `
    '--triggers' `
    '--hex-blob' `
    '--databases' $Database `
    "--result-file=$backupFile"

if ($LASTEXITCODE -ne 0) {
    throw "Backup fehlgeschlagen (Exit-Code $LASTEXITCODE)."
}

# Prüfsumme neben dem Backup speichern
$checksum = (Get-FileHash -Algorithm SHA256 -LiteralPath $backupFile).Hash
"$checksum  $(Split-Path -Leaf $backupFile)" | Set-Content -Path $checksumFile -Encoding ASCII

Write-Host "Backup erstellt: $backupFile"
Write-Host "Prüfsumme gespeichert: $checksumFile"
