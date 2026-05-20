# Export-OpenApi.ps1
#
# Haalt de OpenAPI-specificatie op van een draaiende QuartzRestApi-host
# en slaat het resultaat op als docs/openapi.json.
#
# Gebruik:
#   .\Export-OpenApi.ps1
#   .\Export-OpenApi.ps1 -HostUrl "http://localhost:5000"

param(
    [string]$HostUrl  = "http://localhost:44344",
    [string]$Output   = "$PSScriptRoot/docs/openapi.json"
)

$url = "$HostUrl/openapi/v1.json"
Write-Host "Ophalen van $url ..."

try
{
    $json = Invoke-RestMethod -Uri $url -Method Get
    $json | ConvertTo-Json -Depth 20 | Set-Content -Encoding UTF8 $Output
    Write-Host "Opgeslagen als $Output"
}
catch
{
    Write-Error "Kan '$url' niet bereiken. Zorg dat de SchedulerHost actief is voor u dit script uitvoert."
    exit 1
}
