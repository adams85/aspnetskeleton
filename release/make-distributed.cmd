rd /s /q Service
rd /s /q Api
rd /s /q UI

msbuild ..\source\Web.Distributed.sln /t:Rebuild /p:Configuration=Release /p:Make=true
IF %ERRORLEVEL% NEQ 0 goto:eof
robocopy ..\source\Web\Service.Host\bin Service /e /xf *.log /xf *.xml /xf vshost.*
