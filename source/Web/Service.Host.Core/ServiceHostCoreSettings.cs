using AspNetSkeleton.Core;
using System;

namespace AspNetSkeleton.Service.Host.Core
{
    public interface IServiceHostCoreSettings : ICoreSettings
    {
        TimeSpan WorkerIdleWaitTime { get; }
        int MailSenderBatchSize { get; }
        string MailFrom { get; }
        string[] SupportMailTo { get; }
        string[] SupportMailCc { get; }
    }
}
