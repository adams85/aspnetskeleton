@ECHO OFF

IF NOT "%1"=="" (
   set TARGET_RUNTIME=%1
)

IF "%1"=="" (
   set TARGET_RUNTIME=win-x64
)

echo Target runtime: %TARGET_RUNTIME%

rd /s /q Service
rd /s /q Api
rd /s /q UI

dotnet publish ..\source\Web\UI\UI.csproj -c Release_Monolithic -r %TARGET_RUNTIME%
md UI

robocopy ..\source\Web\UI\bin\%TARGET_RUNTIME%\publish UI /mir