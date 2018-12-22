param(
    [string]$SolutionDir,
    [string]$TargetDir,
    [string]$OutDir,
    [string]$ProjectDir
)
trap {"Error: $_"}

# Copy "Update Helper.exe" to the Resources folder, to be compiled into the final LaunchBuddy executable.
$path = "$SolutionDir\Update Helper\$OutDir\Update Helper.exe"
$destination = "$ProjectDir\Resources\"
Copy-Item -Path $path -Destination $destination