using Karambolo.Common.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Karambolo.Common;

namespace AspNetSkeleton.Common.Cli
{
    public interface IOperationContext : IOperationHostIO
    {
        string AppName { get; }
        IReadOnlyCollection<string> HelpArgs { get; }

        TContext As<TContext>() where TContext : IOperationContext;
    }

    public delegate Operation OperationFactory(string[] args, IOperationContext context);

    public abstract class Operation
    {
        public const string AutoConfirmOption = "yes";

        protected struct ListColumnDef
        {
            public int Width;
            public bool RightJustified;
        }

        static string BuildListFormatString(IReadOnlyOrderedDictionary<string, ListColumnDef> columnDefs)
        {
            var n = columnDefs.Count;
            if (n == 0)
                return string.Empty;

            var sb = new StringBuilder();
            var i = 0;
            while (true)
            {
                var columnDef = columnDefs[i];

                sb.Append('{');
                sb.Append(i);
                sb.Append(',');
                if (!columnDef.RightJustified)
                    sb.Append('-');
                sb.Append(columnDef.Width);
                sb.Append('}');

                if (++i >= n)
                    break;

                sb.Append(' ');
            }

            return sb.ToString();
        }

        protected Operation(string[] args, IOperationContext context)
        {
            Context = context;

            if (args.Length == 2 && Context.HelpArgs.Contains(args[1]))
                throw new UsageException(GetUsage());

            ParseArgs(args);
        }

        protected string[] MandatoryArgs { get; private set; }
        protected IReadOnlyDictionary<string, string> OptionalArgs { get; private set; }

        protected abstract int MandatoryArgCount { get; }
        protected abstract IEnumerable<string> GetUsage();

        void ParseArgs(string[] args)
        {
            if (args.Length < MandatoryArgCount + 1)
                throw CreateSyntaxError();

            MandatoryArgs = new string[MandatoryArgCount];
            Array.Copy(args, 1, MandatoryArgs, 0, MandatoryArgCount);

            var optionalArgs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            OptionalArgs = optionalArgs;

            for (var i = MandatoryArgCount + 1; i < args.Length; i++)
            {
                var optionalArg = args[i];

                if (string.IsNullOrWhiteSpace(optionalArg) || optionalArg[0] != '/')
                    throw CreateSyntaxError();

                var index = optionalArg.IndexOf('=');

                string key, value;
                if (index >= 0)
                {
                    key = optionalArg.Substring(1, index - 1);
                    value = optionalArg.Remove(0, index + 1);
                }
                else
                {
                    key = optionalArg.Remove(0, 1);
                    value = null;
                }

                optionalArgs.Add(key, value);
            }
        }

        protected IOperationContext Context { get; }

        protected UsageException CreateSyntaxError()
        {
            return new UsageException("Syntax error.", GetUsage());
        }

        protected bool PromptForConfirmation()
        {
            if (!Context.InteractiveMode)
                return true;

            if (OptionalArgs.ContainsKey(AutoConfirmOption))
                return true;

            Context.Out.WriteLine("Are you sure? [y/n]");
            var answer = Context.In.ReadLine();

            Context.Out.WriteLine();

            return string.Equals(answer.Trim(), "y", StringComparison.OrdinalIgnoreCase);
        }

        protected void PrintList<T>(IReadOnlyOrderedDictionary<string, ListColumnDef> columnDefs, IEnumerable<T> rows, Func<T, object[]> columnValuesSelector)
        {
            var formatString = BuildListFormatString(columnDefs);

            var columnNames = columnDefs.Keys.ToArray();
            Context.Out.WriteLine(formatString, columnNames);
            Context.Out.WriteLine(formatString, Array.ConvertAll(columnNames, cn => new string('-', cn.Length)));

            var n = 0;
            foreach (var row in rows)
            {
                Context.Out.WriteLine(formatString, columnValuesSelector(row));
                n++;
            }

            Context.Out.WriteLine();
            Context.Out.WriteLine($"{n} rows(s) returned.");
        }

        public abstract void Execute();
    }
}
