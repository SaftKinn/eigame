@echo off
rem Baut und startet "Rettet das Ei". Logs landen in logs\ (dotnet-build.log, import.log, run.log).
setlocal
cd /d "%~dp0"
set "GODOT="
for /d %%D in ("%LOCALAPPDATA%\Microsoft\WinGet\Packages\GodotEngine.GodotEngine.Mono_*") do (
  if exist "%%D\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64_console.exe" set "GODOT=%%D\Godot_v4.6.2-stable_mono_win64\Godot_v4.6.2-stable_mono_win64_console.exe"
)
if not defined GODOT (
  echo Godot 4.6.2 .NET wurde nicht gefunden. > logs-fehlt-godot.txt
  echo Godot 4.6.2 .NET wurde nicht gefunden.
  pause
  exit /b 1
)
if not exist logs mkdir logs
echo [1/3] Baue den C#-Code ...
dotnet build game\RettetDasEi.csproj -nologo -v q > logs\dotnet-build.log 2>&1
if errorlevel 1 (
  echo Der Build ist fehlgeschlagen. Details stehen in logs\dotnet-build.log
  echo BUILD FAILED >> logs\dotnet-build.log
  pause
  exit /b 1
)
echo [2/3] Importiere Bilder ...
"%GODOT%" --headless --path game --import > logs\import.log 2>&1
echo [3/3] Starte das Spiel ...
"%GODOT%" --path game > logs\run.log 2>&1
