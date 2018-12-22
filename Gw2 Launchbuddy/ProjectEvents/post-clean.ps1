param(
    [string]$SolutionDir,
    [string]$TargetDir,
    [string]$OutDir,
    [string]$ProjectDir
)
trap {"Error: $_"}

# Delete remaining files and folders from the target directory to ensure a clean environment.
$ToDelete = "$TargetDir\*"
Remove-Item -Path $ToDelete -Recurse -Force