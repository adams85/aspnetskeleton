using AspNetSkeleton.Api.Contract;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.Service.Contract;
using Karambolo.Common;
using System.Net;
using System.Threading;

namespace AspNetSkeleton.AdminTools
{
    public abstract class ApiOperation : Operation
    {
        public const string ApiUserNameOption = "api-usr";
        public const string ApiPasswordOption = "api-pwd";

        protected ApiOperation(string[] args, IApiOperationContext context) : base(args, context)
        {
            if (!context.IsNested &
                OptionalArgs.TryGetValue(ApiUserNameOption, out string apiUserName) &&
                OptionalArgs.TryGetValue(ApiPasswordOption, out string apiPassword))
                context.ApiCredentials = new NetworkCredential(apiUserName, apiPassword);
        }

        protected abstract void ExecuteCore();

        public sealed override void Execute()
        {
            try { ExecuteCore(); }
            catch (ApiErrorException ex) { throw new OperationErrorException(ex.Message, ex); }
        }

        void EnsureCredentials()
        {
            if (Context.ApiAuthToken == null && Context.ApiCredentials == null)
            {
                if (!Context.InteractiveMode)
                    throw new OperationErrorException("Credentials were not provided.");

                Context.Out.Write("API username: ");
                var apiUserName = Context.In.ReadLine();

                Context.Out.Write("API password: ");
                var apiPassword = Context.ReadPassword();

                Context.ApiCredentials = new NetworkCredential(apiUserName, apiPassword);

                if (Context.InteractiveMode)
                    Context.Out.WriteLine();
            }
        }

        protected TResult Query<TResult>(IQuery<TResult> query)
        {
            EnsureCredentials();
            return Context.QueryDispatcher.DispatchAsync(query, CancellationToken.None).WaitAndUnwrap();
        }

        protected void Command(ICommand command)
        {
            EnsureCredentials();
            Context.CommandDispatcher.DispatchAsync(command, CancellationToken.None).WaitAndUnwrap();
        }

        protected new IApiOperationContext Context => base.Context.As<IApiOperationContext>();
    }
}
