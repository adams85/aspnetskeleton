using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetSkeleton.Core
{
    public class DataProtectionSettings
    {
        public string ApplicationName { get; set; }
        public TimeSpan? KeyLifetime { get; set; }
        public string KeyStorePath { get; set; }
        public bool DisableAutomaticKeyGeneration { get; set; }
    }
}
