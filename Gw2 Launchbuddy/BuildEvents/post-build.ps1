param(
    [string]$SolutionDir,
    [string]$TargetDir,
    [string]$OutDir,
    [string]$ProjectDir
)
trap {"Error: $_"}

#Write-Host "SolutionDir $SolutionDir"
#Write-Host "TargetDir $TargetDir"
#Write-Host "OutDir $OutDir"
#Write-Host "ProjectDir $ProjectDir"

# Copy plugins to the plugin folder
$path = "$SolutionDir\Plugins\TestPlugin\$OutDir\*.dll"
$destination = "$TargetDir\\Plugins"
Get-ChildItem -Path $path | Copy-Item -Destination $destination