using System;

namespace AspNetSkeleton.Api
{
    public class ApiSettings
    {
        public TimeSpan AuthTokenExpirationTimeSpan { get; set; } = TimeSpan.FromDays(7);
        public string ApiBaseUrl { get; set; }
        public string ApiBasePath { get; set; }
    }
}
