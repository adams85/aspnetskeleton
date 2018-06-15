using System;

namespace AspNetSkeleton.Api
{
    public class ApiSettings
    {
        public TimeSpan AuthTokenExpirationTimeSpan { get; set; } = TimeSpan.FromDays(7);
        public string ListenUrl { get; set; }
        public string BranchPrefix { get; set; }
    }
}
