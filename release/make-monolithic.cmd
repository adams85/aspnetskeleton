@ECHO OFF

IF NOT "%1"=="" (
   set TARGET_RUNTIME=%1
)

IF "%1"=="" (
   set TARGET_RUNTIME=ubuntu-x64
)

set BUILD_CONFIGURATION=Release_Monolithic

echo Target runtime: %TARGET_RUNTIME%
echo Build configuration: %BUILD_CONFIGURATION%

rd /s /q Service
rd /s /q Api
rd /s /q UI
rd /s /q DeployTools

dotnet publish ..\source\Web\UI\UI.csproj -c %BUILD_CONFIGURATION% -r %TARGET_RUNTIME%
IF %ERRORLEVEL% NEQ 0 goto:eof

dotnet publish ..\source\Web\Tools\DeployTools\DeployTools.csproj -c %BUILD_CONFIGURATION% -r %TARGET_RUNTIME%
IF %ERRORLEVEL% NEQ 0 goto:eof

md UI
md DeployTools

robocopy ..\source\Web\UI\bin\%TARGET_RUNTIME%\publish UI /mir
robocopy ..\source\Web\Tools\DeployTools\bin\%TARGET_RUNTIME%\publish DeployTools /mir
