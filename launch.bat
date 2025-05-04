@echo off
cd %~dp0
start "Space Pirates" cmd.exe /c "mode con cols=120 lines=100 & title Space Pirates & dotnet run & pause" 