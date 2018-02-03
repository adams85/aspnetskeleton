using System.Web.Configuration;
using Karambolo.Common;
using AspNetSkeleton.Common.Utils;
using AspNetSkeleton.Service.Host.Core;
using System;
using System.Globalization;

namespace AspNetSkeleton.UI
{
    public interface IUIServiceHostSettings : IUISettings, IServiceHostCoreSettings { }

    public class UIServiceHostSettings : UISettings, IUIServiceHostSettings
    {
        public TimeSpan WorkerIdleWaitTime { get; } = TimeSpan.Parse(WebConfigurationManager.AppSettings["WorkerIdleWaitTime"], CultureInfo.InvariantCulture);

        public int MailSenderBatchSize { get; } = int.Parse(WebConfigurationManager.AppSettings["MailSenderBatchSize"]);

        public string MailFrom { get; } = WebConfigurationManager.AppSettings["MailFrom"];

        public string[] SupportMailTo { get; } = SerializationUtils.DeserializeArray(WebConfigurationManager.AppSettings["SupportMailTo"], Identity<string>.Func);

        public string[] SupportMailCc { get; } = SerializationUtils.DeserializeArray(WebConfigurationManager.AppSettings["SupportMailCc"], Identity<string>.Func);
    }
}
