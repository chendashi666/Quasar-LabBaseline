@echo off
rem Quasar Detection Evaluation Laboratory - one click packaging console launcher.
setlocal
set "REPO=%~dp0..\.."
set "EXE=%REPO%\dist\ControlConsole\Quasar.exe"
if not exist "%EXE%" set "EXE=%REPO%\bin\Release\net452\Quasar.exe"
if not exist "%EXE%" (
    echo [LAB] Quasar control console not found.
    echo [LAB] Run build_release.ps1 first.
    pause
    exit /b 1
)
start "" "%EXE%" --lab-packager
