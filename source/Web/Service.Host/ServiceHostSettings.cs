using AspNetSkeleton.Common.Utils;
using AspNetSkeleton.Service.Host.Core;
using AspNetSkeleton.Service.Host.Properties;
using Karambolo.Common;
using System;

namespace AspNetSkeleton.Service.Host
{
    public interface IServiceHostSettings : IServiceHostCoreSettings
    {
        TimeSpan ShutDownTimeout { get; }
    }

    public class ServiceHostSettings : IServiceHostSettings
    {
        public TimeSpan ShutDownTimeout { get; } = Settings.Default.ShutDownTimeout;

        public string ServiceBaseUrl { get; } = Settings.Default.ServiceBaseUrl;

        public string UIBaseUrl { get; } = Settings.Default.UIBaseUrl;

        public string[] AdminMailTo { get; } = SerializationUtils.DeserializeArray(Settings.Default.AdminMailTo, Identity<string>.Func);

        public TimeSpan WorkerIdleWaitTime { get; } = Settings.Default.WorkerIdleWaitTime;

        public int MailSenderBatchSize { get; } = Settings.Default.MailSenderBatchSize;

        public string MailFrom { get; } = Settings.Default.MailFrom;

        public string[] SupportMailTo { get; } = SerializationUtils.DeserializeArray(Settings.Default.SupportMailTo, Identity<string>.Func);

        public string[] SupportMailCc { get; } = SerializationUtils.DeserializeArray(Settings.Default.SupportMailCc, Identity<string>.Func);

        public TimeSpan ServiceTimeOut => throw new NotSupportedException();
    }
}
