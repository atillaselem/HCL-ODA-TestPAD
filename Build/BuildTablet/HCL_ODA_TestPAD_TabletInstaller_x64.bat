@echo off
setlocal enabledelayedexpansion

REM This is a local script to kick off the full build process without obfuscating.
set	BUILD_CONFIG=Release

REM Version Build is current date in 'yyyy.mm.dd' format => 0.23023.01.31'.
set VERSION_BUILD=0.%date:~10,4%.%date:~4,2%.%date:~7,2%
REM FOR /F "TOKENS=1 eol=/ DELIMS=/ " %%A IN ('DATE/T') DO SET dd=%%A
REM FOR /F "TOKENS=1,2 eol=/ DELIMS=/ " %%A IN ('DATE/T') DO SET mm=%%B
REM FOR /F "TOKENS=1,2,3 eol=/ DELIMS=/ " %%A IN ('DATE/T') DO SET yyyy=%%C
REM set VERSION_BUILD=0.%yyyy%.%dd%.%mm%

echo *** Creating the HCL-ODA-TestPAD installer ... ***
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
