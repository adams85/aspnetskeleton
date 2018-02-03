To get Package Manager Console commands work:
* AspNetSkeleton.DeployTools project must be built
* AspNetSkeleton.DataAccess assembly must be set to Copy Local
* Configuration of ADO.NET, EF and connections strings in App.Config must be valid

Enable migrations:
> Enable-Migrations -ProjectName DeployTools -ContextProjectName DataAccess -StartUpProjectName DeployTools

Add migration:
> Add-Migration InitialCreate -ProjectName DeployTools -StartUpProjectName DeployTools