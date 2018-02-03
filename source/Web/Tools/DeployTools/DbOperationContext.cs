using System;
using System.Collections.Generic;
using System.Text;
using AspNetSkeleton.Common;
using AspNetSkeleton.Common.Cli;

namespace AspNetSkeleton.DeployTools
{
    public interface IDbOperationContext : IOperationContext
    {
        IClock Clock { get; }
    }
}
