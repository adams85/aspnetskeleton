using System;
using System.Collections.Generic;

namespace AspNetSkeleton.Common.Cli
{
    public class UsageException : ApplicationException
    {
        public UsageException(IEnumerable<string> usage) : this(null, usage) { }

        public UsageException(string errorMessage, IEnumerable<string> usage) : base(errorMessage)
        {
            Usage = usage;
            IsError = errorMessage != null;
        }

        public bool IsError { get; }
        public IEnumerable<string> Usage { get; }
    }
}
