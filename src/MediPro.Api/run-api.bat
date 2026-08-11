@echo off
echo Starting MediPro API...
set DOTNET_EnableWriteXorExecute=0
set COMPlus_EnableWriteXorExecute=0
set DOTNET_JITMinOpts=1
cd /d "%~dp0"
dotnet run --no-launch-profile --urls "http://localhost:5020"
pause
