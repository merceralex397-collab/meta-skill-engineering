@echo off
REM Build script for WPF edition on Windows

echo ==========================================
echo Meta Skill Studio WPF - Build Script
echo ==========================================
echo.

REM Check for .NET SDK
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK not found!
    echo Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download
    exit /b 1
)

echo Found .NET SDK:
dotnet --version
echo.

REM Step 1: Restore packages
echo Step 1: Restoring NuGet packages...
dotnet restore
if errorlevel 1 goto error

REM Step 2: Build Debug
echo.
echo Step 2: Building Debug configuration...
dotnet build --configuration Debug
if errorlevel 1 goto error

REM Step 3: Build Release
echo.
echo Step 3: Building Release configuration...
dotnet build --configuration Release
if errorlevel 1 goto error

REM Step 4: Publish single-file executable
echo.
echo Step 4: Creating single-file executable...
dotnet publish MetaSkillStudio -c Release -r win-x64 ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true ^
    -o publish
if errorlevel 1 goto error

echo.
echo ==========================================
echo BUILD SUCCESSFUL!
echo ==========================================
echo.
echo Output locations:
echo   Debug:   bin\Debug\net8.0-windows\
echo   Release: bin\Release\net8.0-windows\
echo   Publish: publish\MetaSkillStudio.exe
echo.
echo To run the application:
echo   publish\MetaSkillStudio.exe
echo.

REM Optional: Build installer
echo To create an installer (requires WiX Toolset):
echo   cd installer
echo   powershell -File build-installer.ps1
echo.

goto end

:error
echo.
echo ==========================================
echo BUILD FAILED!
echo ==========================================
exit /b 1

:end
pause
