using System;
using System.IO;

namespace AspNetSkeleton.Common.Cli
{
    public interface IOperationHostIO
    {
        TextReader In { get; }
        TextWriter Out { get; }
        TextWriter Error { get; }

        bool InteractiveMode { get; }

        string ReadPassword();
    }

    public class ConsoleHostIO : IOperationHostIO
    {
        public static readonly ConsoleHostIO Instance = new ConsoleHostIO();

        ConsoleHostIO() { }

        public TextReader In => Console.In;
        public TextWriter Out => Console.Out;
        public TextWriter Error => Console.Error;

        public bool InteractiveMode => !Console.IsInputRedirected;

        public string ReadPassword()
        {
            return
                InteractiveMode ?
                ConsoleUtils.ReadLineMasked() :
                In.ReadLine();
        }
    }
}
