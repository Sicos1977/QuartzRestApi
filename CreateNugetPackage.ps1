<#
.SYNOPSIS
    Builds the QuartzRestApi projects in Release mode and packages them into a single 
    multi-targeted NuGet package with dynamic version patching across all projects.
.PARAMETER Version
    Optional. The version number to apply to the projects and NuGet package (e.g., 1.0.2).
#>
param (
    [Parameter(Mandatory = $false)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

$FrameworkProject = "QuartzRestApi.Net462/QuartzRestApi.Net462.csproj"
$CoreProject = "QuartzRestApi/QuartzRestApi.csproj"
$NuspecPath = "QuartzRestApi.nuspec"
$ArtifactsDir = "artifacts"

function Invoke-DotNetCommand {
    param ([scriptblock]$Script)
    & $Script
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`n[ERROR] Command failed with exit code $LASTEXITCODE. Aborting." -ForegroundColor Red
        Exit $LASTEXITCODE
    }
}

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host " Starting QuartzRestApi Build & Package Process   " -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

# 1. Handle Version Patching
$BuildArgs = @()
if (-not [string]::IsNullOrEmpty($Version)) {
    Write-Host "Setting version to $Version across all components..." -ForegroundColor Yellow
    
    # Add version property for the dotnet build commands
    $BuildArgs = @("/p:Version=$Version")

    # Also patch the manual Nuspec file for the NuGet package metadata
    $xml = [xml](Get-Content $NuspecPath)
    $ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
    $ns.AddNamespace("ns", $xml.DocumentElement.NamespaceURI)
    
    $versionNode = $xml.SelectSingleNode("//ns:metadata/ns:version", $ns)
    if ($versionNode) {
        $versionNode.InnerText = $Version
        $xml.Save($NuspecPath)
        Write-Host "Successfully updated version in nuspec!" -ForegroundColor Green
    } else {
        Write-Error "Could not find <version> node in $NuspecPath"
    }
}

# 2. Clean up old artifacts
if (Test-Path $ArtifactsDir) {
    Write-Host "Cleaning up old artifacts directory..." -ForegroundColor Yellow
    Remove-Item $ArtifactsDir -Recurse -Force
}
New-Item -ItemType Directory -Path $ArtifactsDir -Force | Out-Null

# 3. Clean projects
Write-Host "`n[1/3] Cleaning projects..." -ForegroundColor Green
Invoke-DotNetCommand { dotnet clean $FrameworkProject -c Release --nologo }
Invoke-DotNetCommand { dotnet clean $CoreProject -c Release --nologo }

# 4. Build projects with dynamic versioning
Write-Host "`n[2/3] Building projects (Release mode)..." -ForegroundColor Green
Write-Host "Building .NET Framework 4.6.2 variant..." -ForegroundColor Gray
Invoke-DotNetCommand { dotnet build $FrameworkProject -c Release --nologo $BuildArgs }

Write-Host "Building .NET 10 variant..." -ForegroundColor Gray
Invoke-DotNetCommand { dotnet build $CoreProject -c Release --nologo $BuildArgs }

# 5. Pack package
Write-Host "`n[3/3] Creating unified NuGet package..." -ForegroundColor Green
Invoke-DotNetCommand {
    dotnet pack $CoreProject `
        -c Release `
        -o $ArtifactsDir `
        /p:NuspecFile="../$NuspecPath" `
        /p:IntermediateOutputPath="obj/pack/" `
        $BuildArgs `
        --nologo
}

Write-Host "`n==================================================" -ForegroundColor Cyan
Write-Host " Build & Package Successful!" -ForegroundColor Green
Write-Host " Your NuGet package is available in: $ArtifactsDir" -ForegroundColor Yellow
Write-Host "==================================================" -ForegroundColor Cyan
