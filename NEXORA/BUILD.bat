@echo off
setlocal
where msbuild >nul 2>nul
if errorlevel 1 (
  echo MSBuild was not found. Install Visual Studio with .NET desktop development.
  pause
  exit /b 1
)
msbuild "%~dp0NEXORA.sln" /p:Configuration=Release /p:Platform="Any CPU" /m
if errorlevel 1 (
  echo Build failed. Check the error list above.
  pause
  exit /b 1
)
echo.
echo Build completed. Output: src\NEXORA\bin\Release\NEXORA.exe
pause
