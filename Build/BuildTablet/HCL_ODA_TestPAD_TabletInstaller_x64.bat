echo off
setlocal enabledelayedexpansion

REM This is a local script to kick off the full build process without obfuscating.
set	BUILD_CONFIG=Release

for /f %%i in ('"powershell (Get-Date).ToString(\"dd\")"') do set day=%%i
for /f %%i in ('"powershell (Get-Date).ToString(\"MM\")"') do set month=%%i
for /f %%i in ('"powershell (Get-Date).ToString(\"yyyy\")"') do set year=%%i
set VERSION_BUILD=0.%year%.%month%.%day%

echo *** Creating the HCL-ODA-TestPAD (64-bit) installer ... ***
if "%ProgramFiles(x86)%."=="." set PFDir=%ProgramFiles%
if not "%ProgramFiles(x86)%."=="." set PFDir=%ProgramFiles(x86)%

rem To be consistent with default build chain on build agents, check also c:\apps if installation path of NSIS is not found
if not exist "%PFDir%\NSIS" set PFDir=C:\apps

"%PFDir%\NSIS\makensis.exe" /V3 /DVersionBuild=%VERSION_BUILD% /DSourceMainDir=%BUILD_CONFIG% /DFileVersion=%VERSION_BUILD% /DSourceSubDir= "HCL_ODA_TestPAD_NSIS_Installer_x64.nsi"

if errorlevel 1 goto :installerError
popd

echo.
echo ****BUILDING COMPLETE ****
pause

goto :EOF

:installerError
Title INSTALLER ERROR:
echo "!! An error occurred when creating the HCL-ODA-TestPAD installer !!"
pause