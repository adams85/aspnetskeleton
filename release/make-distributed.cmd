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
   set BUILD_CONFIGURATION=Release_Distributed
)

echo Target runtime: %TARGET_RUNTIME%
echo Build configuration: %BUILD_CONFIGURATION%

rd /s /q Service
rd /s /q Api
rd /s /q UI

dotnet publish ..\source\Web\Service.Host\Service.Host.csproj -c %BUILD_CONFIGURATION% -r %TARGET_RUNTIME%
dotnet publish ..\source\Web\Api\Api.csproj -c %BUILD_CONFIGURATION% -r %TARGET_RUNTIME%
dotnet publish ..\source\Web\UI\UI.csproj -c %BUILD_CONFIGURATION% -r %TARGET_RUNTIME%

md Service
md Api
md UI

robocopy ..\source\Web\Service.Host\bin\Release_Distributed\%TARGET_RUNTIME%\publish Service /mir
robocopy ..\source\Web\Api\bin\%TARGET_RUNTIME%\publish Api /mir
robocopy ..\source\Web\UI\bin\%TARGET_RUNTIME%\publish UI /mir