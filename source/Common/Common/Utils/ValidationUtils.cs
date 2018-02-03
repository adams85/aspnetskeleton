using System;
using System.Net.Mail;

namespace AspNetSkeleton.Common.Utils
{
    public static class ValidationUtils
    {
        public static bool IsValidEmailAddress(string value)
        {
            try { return new MailAddress(value).Address == value; }
            catch (FormatException) { return false; }
        }
    }
}
