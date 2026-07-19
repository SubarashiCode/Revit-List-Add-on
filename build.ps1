param([ValidateSet('2025','2026','All')][string]$RevitVersion='All')
$ErrorActionPreference='Stop'
$versions = if($RevitVersion -eq 'All'){'2025','2026'}else{$RevitVersion}
foreach($version in $versions){
  $destination=Join-Path $PSScriptRoot "Deploy\Revit$version"
  dotnet build (Join-Path $PSScriptRoot 'ListAddin.csproj') -c Release -p:RevitVersion=$version -o $destination
  if($LASTEXITCODE -ne 0){throw "Revit $version build failed."}
  Copy-Item (Join-Path $PSScriptRoot 'Manifests\ListAddin.addin') $destination -Force
}

