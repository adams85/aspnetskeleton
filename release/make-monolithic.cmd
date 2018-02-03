rd /s /q Service
rd /s /q Api
rd /s /q UI

msbuild ..\source\Web.Monolithic.sln /t:Rebuild /p:Configuration=Release /p:Make=true
rd /s /q Api
