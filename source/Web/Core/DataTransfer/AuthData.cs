using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AspNetSkeleton.Core.DataTransfer
{
    public class AuthData
    {
        public static string GenerateToken(AuthData authData, byte[] encryptionKey)
        {
            if (authData == null)
                throw new ArgumentNullException(nameof(authData));
            if (encryptionKey == null)
                throw new ArgumentNullException(nameof(encryptionKey));

            byte[] data;
            using (var ms = new MemoryStream())
            using (var cryptoProvider = new AesCryptoServiceProvider())
            {
                cryptoProvider.Mode = CipherMode.CBC;
                cryptoProvider.Padding = PaddingMode.PKCS7;
                cryptoProvider.Key = encryptionKey;
                cryptoProvider.GenerateIV();

                var iv = cryptoProvider.IV;
                ms.Write(iv, 0, iv.Length);

                using (var encryptor = cryptoProvider.CreateEncryptor())
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var writer = new BinaryWriter(cs))
                {
                    var bytes = Encoding.UTF8.GetBytes(authData.UserName);
                    writer.Write(bytes.Length);
                    writer.Write(bytes);

                    writer.Write(authData.ExpirationTime.Ticks);

                    bytes = Encoding.UTF8.GetBytes(authData.DeviceId);
                    writer.Write(bytes.Length);
                    writer.Write(bytes);

                    cs.FlushFinalBlock();
                }

                data = ms.ToArray();
            }

            return Convert.ToBase64String(data);
        }

        public static AuthData ParseToken(string token, byte[] encryptionKey)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            if (encryptionKey == null)
                throw new ArgumentNullException(nameof(encryptionKey));

            byte[] data;
            try { data = Convert.FromBase64String(token); }
            catch (FormatException) { return null; }

            using (var ms = new MemoryStream(data))
            using (var cryptoProvider = new AesCryptoServiceProvider())
            {
                cryptoProvider.Mode = CipherMode.CBC;
                cryptoProvider.Padding = PaddingMode.PKCS7;
                cryptoProvider.Key = encryptionKey;

                var iv = new byte[cryptoProvider.BlockSize >> 3];
                if (ms.Read(iv, 0, iv.Length) < iv.Length)
                    return null;

                cryptoProvider.IV = iv;

                using (var decryptor = cryptoProvider.CreateDecryptor())
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new BinaryReader(cs))
                    try
                    {
                        var length = reader.ReadInt32();
                        var bytes = reader.ReadBytes(length);
                        var userName = Encoding.UTF8.GetString(bytes);

                        var expirationTime = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);

                        length = reader.ReadInt32();
                        bytes = reader.ReadBytes(length);
                        var deviceId = Encoding.UTF8.GetString(bytes);

                        return new AuthData
                        {
                            UserName = userName,
                            ExpirationTime = expirationTime,
                            DeviceId = deviceId,
                        };
                    }
                    catch (EndOfStreamException) { return null; }
            }
        }

        public string UserName { get; set; }
        public DateTime ExpirationTime { get; set; }
        public string DeviceId { get; set; }
    }
}
