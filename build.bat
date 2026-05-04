@echo off
setlocal EnableDelayedExpansion

echo.
echo ===============================================
echo    WebFileManager - Build script (Windows x64)
echo ===============================================
echo.

:: Check dotnet
where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] .NET SDK not found. Download from https://dot.net
    pause
    exit /b 1
)

for /f "tokens=*" %%v in ('dotnet --version 2^>nul') do set DOTNET_VER=%%v
echo [OK] .NET SDK %DOTNET_VER%

:: Paths
set ROOT=%~dp0
set PROJ=%ROOT%CompanyFileManager\CompanyFileManager.csproj
set OUT=%ROOT%Release
set DIST=%ROOT%Dist

if not exist "%PROJ%" (
    echo [ERROR] Project file not found: %PROJ%
    pause
    exit /b 1
)

:: Step 1: Clean
echo.
echo [1/5] Cleaning previous builds...
if exist "%OUT%"  rmdir /s /q "%OUT%"
if exist "%DIST%" rmdir /s /q "%DIST%"
mkdir "%OUT%"

:: Step 2: Publish
echo [2/5] Building self-contained single-file exe...
echo.

dotnet publish "%PROJ%" ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output "%OUT%" ^
    -p:PublishSingleFile=true ^
    -p:PublishReadyToRun=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true ^
    -p:DebugType=None ^
    -p:DebugSymbols=false ^
    --verbosity minimal

if errorlevel 1 (
    echo.
    echo [ERROR] dotnet publish failed
    pause
    exit /b 1
)

:: Step 3: Cleanup
echo.
echo [3/5] Final cleanup...

if exist "%OUT%\appsettings.Development.json" del "%OUT%\appsettings.Development.json"
if exist "%OUT%\web.config"                   del "%OUT%\web.config"

if not exist "%OUT%\appsettings.user.json" (
    echo {}> "%OUT%\appsettings.user.json"
)

:: Create simple README
(
    echo WebFileManager - How to run
    echo =============================
    echo.
    echo 1. Run CompanyFileManager.exe
    echo 2. Open in browser: http://localhost:5000
    echo 3. Go to Settings ^(/setup^):
    echo    - Pick a folder to share
    echo    - Set the port ^(default 5000^)
    echo    - Enable UPnP for automatic port forwarding
    echo    - Click Apply - get the public link
    echo 4. Share the link with friends.
    echo    They will see only the file manager ^(no settings^).
    echo.
    echo Folder structure:
    echo   CompanyFileManager.exe  - main file ^(~60 MB with .NET runtime^)
    echo   wwwroot\                - static assets ^(CSS, JS, icons^)
    echo   appsettings.user.json   - your settings ^(auto-created^)
    echo.
    echo Requirements: Windows 10/11 x64. .NET runtime is bundled.
) > "%OUT%\HOW_TO_RUN.txt"

:: Step 4: ZIP archive
echo [4/5] Creating ZIP archive...

mkdir "%DIST%"

set ZIP_NAME=WebFileManager_win-x64.zip
set ZIP_PATH=%DIST%\%ZIP_NAME%

powershell -NoProfile -Command "Compress-Archive -Path '%OUT%\*' -DestinationPath '%ZIP_PATH%' -Force"

if not exist "%ZIP_PATH%" (
    echo [WARN] ZIP not created - continuing anyway
) else (
    for %%f in ("%ZIP_PATH%") do set ZIP_SIZE=%%~zf
    set /a ZIP_MB=!ZIP_SIZE! / 1048576
    echo [OK] Archive: %ZIP_NAME% ^(!ZIP_MB! MB^)
)

:: Step 5: Summary
for %%f in ("%OUT%\CompanyFileManager.exe") do set EXE_SIZE=%%~zf
set /a EXE_MB=!EXE_SIZE! / 1048576

echo.
echo [5/5] Done!
echo.
echo ===============================================
echo    Build successful
echo ===============================================
echo    EXE:  Release\CompanyFileManager.exe (~!EXE_MB! MB^)
if exist "%ZIP_PATH%" echo    ZIP:  Dist\%ZIP_NAME%
echo.
echo    Run:  Release\CompanyFileManager.exe
echo    URL:  http://localhost:5000
echo ===============================================
echo.

set /p Q=Open Dist folder in Explorer? (y/n):
if /i "!Q!"=="y" explorer "%DIST%"

endlocal
