using System.Configuration;
using System.Web.Configuration;
using Karambolo.Common;
using System;
using AspNetSkeleton.Core;
using System.Globalization;
using AspNetSkeleton.Common.Utils;

namespace AspNetSkeleton.Api
{
    public interface IApiSettings : ICoreSettings
    {
        byte[] EncryptionKey { get; }

        TimeSpan AuthTokenExpirationTimeSpan { get; }

        string ApiBasePath { get; }
    }

    public class ApiSettings : IApiSettings
    {
        public TimeSpan ServiceTimeOut { get; } = TimeSpan.Parse(WebConfigurationManager.AppSettings["ServiceTimeOut"], CultureInfo.InvariantCulture);

        public string ServiceBaseUrl { get; } = WebConfigurationManager.AppSettings["ServiceBaseUrl"];

        public string UIBaseUrl { get; } = WebConfigurationManager.AppSettings["UIBaseUrl"];

        public string[] AdminMailTo { get; } = SerializationUtils.DeserializeArray(WebConfigurationManager.AppSettings["AdminMailTo"], Identity<string>.Func);

        public byte[] EncryptionKey { get; } = StringUtils.HexStringToByteArray(((MachineKeySection)ConfigurationManager.GetSection("system.web/machineKey")).DecryptionKey);

        public TimeSpan AuthTokenExpirationTimeSpan { get; } = TimeSpan.FromDays(int.Parse(WebConfigurationManager.AppSettings["AuthTokenExpirationDays"]));

        public string ApiBasePath { get; } = WebConfigurationManager.AppSettings["ApiBasePath"];
    }
}
