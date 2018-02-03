using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace AspNetSkeleton.AdminTools
{
    [HandlerFor(Name)]
    [DisplayName(Hint)]
    public class SessionOperation : ApiOperation
    {
        public const string Name = "session";
        public const string Hint = "Starts a batch processing session.";

        public SessionOperation(string[] args, IApiOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 0;

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name}";
        }

        protected override void ExecuteCore()
        {
            if (Context.IsNested)
                throw new InvalidOperationException("Session has already been started.");

            if (Context.InteractiveMode)
            {
                Context.Out.WriteLine("Session started. Waiting for commands...");
                Context.Out.WriteLine();
            }

            while (true)
            {
                Context.Out.Write("@>");
                var command = Context.In.ReadLine();

                if (command == null || 
                    string.Equals(command = command.Trim(), "quit", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(command, "exit", StringComparison.OrdinalIgnoreCase))
                    break;

                if (!Context.InteractiveMode)
                    Context.Out.WriteLine(command);

                if (command.Length == 0 || command.StartsWith(";"))
                    continue;

                string[] args = null;
                var result = 0;
                try { args = ConsoleUtils.SplitCommandLine(command).ToArray(); }
                catch { result = 2; }

                if (result == 0)
                    result = Context.As<IApiOperationContext>().ExecuteNested(args);

                Context.Out.WriteLine();
                Context.Out.Write("@@");
                if (result == 0)
                    Context.Out.WriteLine("Command succeeded.");
                else if (result > 0)
                    Context.Out.WriteLine("Command is invalid.");
                else
                    Context.Out.WriteLine($"Command failed with code {result}.");

                Context.Out.WriteLine();

                if (!Context.InteractiveMode && result != 0)
                    throw new OperationErrorException("Batch execution failed.");
            }

            if (Context.InteractiveMode)
                Context.Out.WriteLine("Session ended.");
        }
    }
}
