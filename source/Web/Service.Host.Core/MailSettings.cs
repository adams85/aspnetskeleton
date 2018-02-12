using MailKit.Security;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace AspNetSkeleton.Service.Host.Core
{
    public class MailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public SecureSocketOptions Security { get; set; } = SecureSocketOptions.Auto;
        public bool Authenticate { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(100_000);

        public bool UsePickupDir { get; set; }
        public string PickupDirPath { get; set; }
    }
}
