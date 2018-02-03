using AspNetSkeleton.Api.Contract;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.Service.Contract;
using System.Net;

namespace AspNetSkeleton.AdminTools
{
    public interface IApiOperationContext : IOperationContext
    {        
        IToolsSettings Settings { get; }
        IApiService ApiService { get; }
        IQueryDispatcher QueryDispatcher { get; }
        ICommandDispatcher CommandDispatcher { get; }

        string ApiAuthToken { get; set; }
        NetworkCredential ApiCredentials { get; set; }

        bool IsNested { get; }
        int ExecuteNested(string[] args);
    }
}
