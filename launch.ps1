$projectPath = $PSScriptRoot
Set-Location $projectPath
Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run" 