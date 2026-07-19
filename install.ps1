param([ValidateSet('2025','2026','All')][string]$RevitVersion='All')
& (Join-Path $PSScriptRoot 'build.ps1') -RevitVersion $RevitVersion
$versions = if($RevitVersion -eq 'All'){'2025','2026'}else{$RevitVersion}
foreach($version in $versions){
  $target=Join-Path $env:APPDATA "Autodesk\Revit\Addins\$version\ListAddin"
  New-Item -ItemType Directory -Path $target -Force | Out-Null
  Copy-Item (Join-Path $PSScriptRoot "Deploy\Revit$version\*") $target -Recurse -Force
  $manifest=Join-Path $env:APPDATA "Autodesk\Revit\Addins\$version\ListAddin.addin"
  $xml=Get-Content (Join-Path $PSScriptRoot 'Manifests\ListAddin.addin') -Raw
  $xml=$xml.Replace('<Assembly>ListAddin.dll</Assembly>',"<Assembly>$target\ListAddin.dll</Assembly>")
  Set-Content -Path $manifest -Value $xml -Encoding utf8
}

