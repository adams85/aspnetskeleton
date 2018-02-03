using Karambolo.Common;
using System;
using System.Linq;
using System.Text;

namespace AspNetSkeleton.Api.Contract.DataTransfer
{
    public class CredentialsData
    {
        const char separatorChar = '|';

        public static string GenerateToken(CredentialsData credentials)
        {
            if (credentials == null)
                throw new ArgumentNullException(nameof(credentials));

            if (credentials.UserName == null || credentials.Password == null || credentials.DeviceId == null)
                throw new ArgumentException(null, nameof(credentials));

            var items = new[]
            {
                credentials.UserName,
                credentials.Password,
                credentials.DeviceId,
            };

            var serializedValue = StringUtils.JoinEscaped(separatorChar, separatorChar, items);
            var data = Encoding.UTF8.GetBytes(serializedValue);

            return Convert.ToBase64String(data);
        }

        public static CredentialsData ParseToken(string token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            byte[] data;
            try { data = Convert.FromBase64String(token); }
            catch (FormatException) { return null; }

            var serializedValue = Encoding.UTF8.GetString(data);

            var items = serializedValue.SplitEscaped(separatorChar, separatorChar).ToArray();
            if (items.Length != 3)
                return null;

            return new CredentialsData
            {
                UserName = items[0],
                Password = items[1],
                DeviceId = items[2],
            };
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
    }
}
