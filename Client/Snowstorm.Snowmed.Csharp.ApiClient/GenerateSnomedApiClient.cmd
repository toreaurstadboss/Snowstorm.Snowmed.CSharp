@echo off
setlocal EnableDelayedExpansion

rem ============================================================
rem ANSI Colors
rem ============================================================

for /F %%a in ('echo prompt $E ^| cmd') do set "ESC=%%a"

set "GREEN=%ESC%[92m"
set "RED=%ESC%[91m"
set "YELLOW=%ESC%[93m"
set "CYAN=%ESC%[96m"
set "GRAY=%ESC%[90m"
set "RESET=%ESC%[0m"

cls

echo(
echo %CYAN%=========================================================================%RESET%
echo %CYAN%               SNOWSTORM SNOMED CT API CLIENT GENERATOR                 %RESET%
echo %CYAN%=========================================================================%RESET%
echo(

echo %YELLOW%Make sure you are running Snowstorm SNOMED CT when generating the client.%RESET%
echo(
echo %YELLOW%Make sure the OpenAPI document is available here:%RESET%
echo(
echo   %CYAN%https://localhost:7130/openapi/v1.json%RESET%
echo(



echo %GRAY%Starting API client generation...%RESET%
echo(

set "START=%TIME%"

echo ❄️ Snowstorm API Client Generator ❄️

dotnet msbuild ^
C:\src\Snowstorm.Snowmed.Csharp\Client\Snowstorm.Snowmed.Csharp.ApiClient\Snowstorm.Snowmed.Csharp.ApiClient.csproj ^
/t:GenerateApiClient ^
/p:OpenApiUrl=https://localhost:7130/openapi/v1.json ^
/v:minimal

set EXITCODE=%ERRORLEVEL%

echo(

if not "%EXITCODE%"=="0" (
    echo %RED%=========================================================================%RESET%
    echo %RED%API CLIENT GENERATION FAILED%RESET%
    echo %RED%Duration: %HH%:%MM%:%SS%.%FF%%RESET%
    echo %RED%Exit code: %EXITCODE%%RESET%
    echo %RED%=========================================================================%RESET%
    echo(
    pause
    exit /b %EXITCODE%
)

echo %GREEN%=========================================================================%RESET%
echo %GREEN%Api client generation: successful%RESET%
echo %GREEN%=========================================================================%RESET%
echo(


