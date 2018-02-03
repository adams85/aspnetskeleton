using System;

namespace AspNetSkeleton.Service.Host.Core
{
    public class ServiceHostCoreSettings
    {
        public TimeSpan DefaultCacheSlidingExpiration { get; set; } = TimeSpan.FromMinutes(10);
        public TimeSpan WorkerIdleWaitTime { get; set; } = TimeSpan.FromSeconds(5);

        public int MailSenderBatchSize { get; set; } = 8;
        public string MailFrom { get; set; }
        public string[] SupportMailTo { get; set; }
        public string[] SupportMailCc { get; set; }
    }
}
