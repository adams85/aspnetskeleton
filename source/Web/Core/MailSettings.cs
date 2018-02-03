using System.IO;
using System.Net;
using System.Net.Mail;

namespace AspNetSkeleton.Core
{
    public class MailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public bool UsePickupDir { get; set; }
        public string PickupDirPath { get; set; }

        public void Configure(SmtpClient smtpClient, string pickupBasePath)
        {
            if (!UsePickupDir)
            {
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Host = Host;
                smtpClient.Port = Port;
                smtpClient.EnableSsl = UseSsl;
                smtpClient.UseDefaultCredentials = UseDefaultCredentials;
                // TODO: store credentials encrypted?
                if (!UseDefaultCredentials)
                    smtpClient.Credentials = new NetworkCredential(UserName, Password);
            }
            else
            {
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;

                var pickupDirPath = PickupDirPath ?? string.Empty;
                if (!Path.IsPathRooted(pickupDirPath))
                    pickupDirPath = Path.Combine(pickupBasePath, pickupDirPath);

                smtpClient.PickupDirectoryLocation = pickupDirPath;
            }
        }
    }
}
