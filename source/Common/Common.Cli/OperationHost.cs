using AspNetSkeleton.Common.Utils;
using Karambolo.Common;
using Karambolo.Common.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AspNetSkeleton.Common.Cli
{
    public interface IOperationHost
    {
        int Execute(string[] args);
    }

    public abstract class OperationHost : IOperationHost, IOperationContext
    {
        static readonly IReadOnlySet<string> helpArgs = new ReadOnlyEnabledHashSet<string>(StringComparer.OrdinalIgnoreCase) { "/?", "-?", "/h", "-h", "help" };

        readonly IReadOnlyDictionary<string, OperationDescriptor> _operationDescriptors;
        readonly IOperationHostIO _io;
        readonly IOperationContext _context;

        public ILogger Logger { get; set; }

        protected OperationHost(IEnumerable<OperationDescriptor> operationDescriptors, IOperationHostIO io)
        {
            if (operationDescriptors == null)
                throw new ArgumentNullException(nameof(operationDescriptors));

            if (io == null)
                throw new ArgumentNullException(nameof(io));

            Logger = NullLogger.Instance;

            _context = this;
            _operationDescriptors = operationDescriptors.ToDictionary(od => od.Name, Identity<OperationDescriptor>.Func);
            _io = io;
        }

        protected OperationHost(OperationHost prototype)
            : this(prototype._operationDescriptors.Values, prototype._io)
        {
            Logger = prototype.Logger;

            _context = prototype;
        }

        public abstract string AppName { get; }

        public virtual IReadOnlySet<string> HelpArgs => helpArgs;

        protected virtual IEnumerable<string> GetInstructions()
        {
            return Enumerable.Empty<string>();
        }

        IEnumerable<string> GetUsage()
        {
            yield return $"{AppName} {{command}}";

            yield return string.Empty;
            yield return $"You can specify /{Operation.AutoConfirmOption} option to automatically answer confirmation prompts.";

            foreach (var line in GetInstructions())
                yield return line;

            yield return string.Empty;
            yield return "Commands:";

            var defaultOperationFound = false;
            foreach (var descriptorKvp in _operationDescriptors.OrderBy(d => d.Key))
            {
                var text = "  " + descriptorKvp.Key;
                if (descriptorKvp.Key == DefaultOperationName)
                {
                    text += "[*]";
                    defaultOperationFound = true;
                }

                if (!string.IsNullOrEmpty(descriptorKvp.Value.Hint))
                    text = string.Concat(text, " - ", descriptorKvp.Value.Hint);

                yield return text;
            };

            if (defaultOperationFound)
                yield return "[*] Default command. Executed when no command specified.";

            yield return string.Empty;
            yield return $"Use {{command}} -? or {{command}} help to get usage of a specific command.";
        }

        protected virtual string DefaultOperationName => null;

        public int Execute(string[] args)
        {
            try
            {
                var argCount = args.Length;
                if (argCount < 1)
                {
                    var defaultOperationName = DefaultOperationName;
                    if (defaultOperationName != null)
                    {
                        var modifiedArgs = new string[argCount + 1];
                        Array.Copy(args, 0, modifiedArgs, 1, argCount);
                        modifiedArgs[0] = defaultOperationName;
                        args = modifiedArgs;
                    }
                    else
                        throw new UsageException("No command specified.", GetUsage());
                }

                var operationName = args[0];
                if (HelpArgs.Contains(operationName))
                    throw new UsageException(GetUsage());
                else if (_operationDescriptors.TryGetValue(operationName, out OperationDescriptor operationDescriptor))
                    operationDescriptor.Factory(args, _context).Execute();
                else
                    throw new UsageException($"Unknown command specified: {operationName}", GetUsage());

                return 0;
            }
            catch (UsageException ex)
            {
                if (ex.IsError)
                {
                    Error.WriteLine(ex.Message);
                    Error.WriteLine();
                }

                Out.WriteLine("Usage:");
                foreach (var line in ex.Usage)
                    Out.WriteLine(line);

                return ex.IsError ? 1 : 0;
            }
            catch (Exception ex)
            {
                int resultCode;
                if (!(ex is OperationErrorException))
                {
                    Logger.LogError(ex, "Unexpected error.");
                    resultCode = -2;
                }
                else
                    resultCode = -1;

                Error.WriteLine(ex.Message);
                Error.WriteLine();

                return resultCode;
            }
        }

        public TextReader In => _io.In;
        public TextWriter Out => _io.Out;
        public TextWriter Error => _io.Error;

        public bool InteractiveMode => _io.InteractiveMode;

        public string ReadPassword()
        {
            return _io.ReadPassword();
        }

        public TContext As<TContext>() where TContext : IOperationContext
        {
            return (TContext)(IOperationContext)this;
        }
    }
}
