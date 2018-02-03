using System;
using AspNetSkeleton.Common;
using Microsoft.Extensions.Internal;

namespace AspNetSkeleton.Core.Infrastructure
{
    public class SystemClock : Clock, ISystemClock
    {
        DateTimeOffset ISystemClock.UtcNow => UtcNow;
    }
}
