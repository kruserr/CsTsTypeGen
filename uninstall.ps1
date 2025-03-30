param($installPath, $toolsPath, $package, $project)

Write-Host "Uninstalling CsTsTypeGen..."
Write-Host ""
Write-Host "The TypeScript definition generation will no longer run on build."
Write-Host "Note: The previously generated TypeScript definition file will not be removed."
Write-Host "If you want to remove it, please delete it manually."
Write-Host ""
Write-Host "Uninstallation complete."