using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AspNetSkeleton.Base;
using AspNetSkeleton.Common;
using AspNetSkeleton.Service.Contract.DataObjects;

namespace AspNetSkeleton.Core.DataTransfer
{
    public class AuthData
    {
        public static string GenerateToken(AuthData authData, Func<byte[], byte[]> encryptor)
        {
            if (authData == null)
                throw new ArgumentNullException(nameof(authData));
            if (encryptor == null)
                throw new ArgumentNullException(nameof(encryptor));

            byte[] data;
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                var bytes = Encoding.UTF8.GetBytes(authData.UserName);
                writer.Write(bytes.Length);
                writer.Write(bytes);

                bytes = Encoding.UTF8.GetBytes(authData.DeviceId);
                writer.Write(bytes.Length);
                writer.Write(bytes);

                writer.Flush();

                data = ms.ToArray();
            }

            data = encryptor(data);

            return Convert.ToBase64String(data);
        }

        public static AuthData ParseToken(string token, Func<byte[], byte[]> decryptor)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            if (decryptor == null)
                throw new ArgumentNullException(nameof(decryptor));

            byte[] data;
            try { data = Convert.FromBase64String(token); }
            catch (FormatException) { return null; }

            try { data = decryptor(data); }
            catch (CryptographicException) { return null; }

            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
                try
                {
                    var length = reader.ReadInt32();
                    var bytes = reader.ReadBytes(length);
                    var userName = Encoding.UTF8.GetString(bytes);

                    length = reader.ReadInt32();
                    bytes = reader.ReadBytes(length);
                    var deviceId = Encoding.UTF8.GetString(bytes);

                    return new AuthData
                    {
                        UserName = userName,
                        DeviceId = deviceId,
                    };
                }
                catch (EndOfStreamException) { return null; }
        }

        public string UserName { get; set; }
        public string DeviceId { get; set; }
    }
}
