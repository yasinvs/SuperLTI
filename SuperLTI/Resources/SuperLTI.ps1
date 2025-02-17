﻿$env:SuperLTI = $true
$SuperLTI.WindowTitle = "SuperLTI"
New-EventLog -LogName "Application" -Source "SuperLTI"
Start-Transcript -Path "{FOLDERPATH}\SuperLTI.log"
Write-Progress -Activity "Preparing..." -Status "Saving ExecutionPolicy..." -PercentComplete 0
$cppsxp = Get-ExecutionPolicy
Write-Progress -Activity "Preparing..." -Status "Setting ExecutionPolicy..." -PercentComplete 3
Set-ExecutionPolicy -ExecutionPolicy Bypass -Force
Write-Progress -Activity "Running..." -Status "Running SuperLTI Script..." -PercentComplete 50
Set-Location -Path "{FOLDERPATH}\"
&"{FOLDERPATH}\SuperLTI.ps1"
Set-Location -Path "C:\"
Write-Progress -Activity "Cleaning up..." -Status "Restoring ExecutionPolicy..." -PercentComplete 90
Set-ExecutionPolicy -ExecutionPolicy $cppsxp -Force
Write-Progress -Activity "Cleaning up..." -Status "Writing Event Log..." -PercentComplete 95
Stop-Transcript
Copy-Item -Path "{FOLDERPATH}\SuperLTI.log" -Destination "C:\SuperLTI.log"
Write-EventLog -EventId 0 -LogName "Application" -Message (Get-Content -Path "{FOLDERPATH}\SuperLTI.log" | Out-String) -Source "SuperLTI" -EntryType Information
Write-Progress -Activity "Cleaning up..." -Status "Removing SuperLTI Directory..." -PercentComplete 100
Remove-Item -Recurse -Force -Path "{FOLDERPATH}\"