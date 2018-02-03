using System;

namespace AspNetSkeleton.Common.Cli
{
    public class OperationErrorException : ApplicationException
    {
        public OperationErrorException(string message) : base(message) { }

        public OperationErrorException(string message, Exception innerException) : base(message, innerException) { }
    }
}
