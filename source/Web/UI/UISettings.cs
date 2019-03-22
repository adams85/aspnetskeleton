using System.Configuration;
using System.Web.Configuration;
using Karambolo.Common;
using AspNetSkeleton.Common.Utils;
using System;
using System.Globalization;
using AspNetSkeleton.Core;

namespace AspNetSkeleton.UI
{
    public interface IUISettings : ICoreSettings
    {
        string[] ModelMetadataUrls { get; }

        int CurrentMajorVersion { get; }

        byte[] EncryptionKey { get; }

        int DefaultPageSize { get; }

        bool IsRegistrationEnabled { get; }
        TimeSpan PasswordTokenExpiration { get; }
        int DefaultDeviceLimit { get; }
    }

    public class UISettings : IUISettings
    {
        public TimeSpan ServiceTimeOut { get; } = TimeSpan.Parse(WebConfigurationManager.AppSettings["ServiceTimeOut"], CultureInfo.InvariantCulture);

        public string ServiceBaseUrl { get; } = WebConfigurationManager.AppSettings["ServiceBaseUrl"];

        public string UIBaseUrl { get; } = WebConfigurationManager.AppSettings["UIBaseUrl"];

        public string[] AdminMailTo { get; } = SerializationUtils.DeserializeArray(WebConfigurationManager.AppSettings["AdminMailTo"], Identity<string>.Func);

        public string[] ModelMetadataUrls { get; } = SerializationUtils.DeserializeArray(WebConfigurationManager.AppSettings["ModelMetadataUrls"], Identity<string>.Func);

        public int CurrentMajorVersion { get; } = int.Parse(WebConfigurationManager.AppSettings["CurrentMajorVersion"]);

        public byte[] EncryptionKey { get; } = StringUtils.BytesFromHexString(((MachineKeySection) ConfigurationManager.GetSection("system.web/machineKey")).DecryptionKey);

        public int DefaultPageSize { get; } = int.Parse(WebConfigurationManager.AppSettings["DefaultPageSize"]);
       
        public bool IsRegistrationEnabled { get; } = bool.Parse(WebConfigurationManager.AppSettings["IsRegistrationEnabled"]);

        public TimeSpan PasswordTokenExpiration { get; } = TimeSpan.FromDays(double.Parse(WebConfigurationManager.AppSettings["PasswordTokenExpirationDays"], CultureInfo.InvariantCulture));

        public int DefaultDeviceLimit { get; } = int.Parse(WebConfigurationManager.AppSettings["DefaultDeviceLimit"]);
    }
}
