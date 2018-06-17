@ECHO OFF

IF NOT "%1"=="" (
   set TARGET_RUNTIME=%1
)

IF "%1"=="" (
   set TARGET_RUNTIME=ubuntu-x64
)

IF NOT "%2"=="" (
   set BUILD_CONFIGURATION=%2
)

IF "%2"=="" (
   set BUILD_CONFIGURATION=Release_Monolithic
)

echo Target runtime: %TARGET_RUNTIME%
echo Build configuration: %BUILD_CONFIGURATION%

rd /s /q Service
rd /s /q Api
rd /s /q UI

dotnet publish ..\source\Web\UI\UI.csproj -c %BUILD_CONFIGURATION% -r %TARGET_RUNTIME%
md UI

robocopy ..\source\Web\UI\bin\%TARGET_RUNTIME%\publish UI /mir