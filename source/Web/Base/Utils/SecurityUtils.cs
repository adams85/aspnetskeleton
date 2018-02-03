using AspNetSkeleton.Common.Utils;
using Karambolo.Common;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.IO;
using System.Security.Cryptography;

namespace AspNetSkeleton.Base.Utils
{
    public static class SecurityUtils
    {
        public static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

        // CRC8-ITU
        // http://www.sunshine2k.de/coding/javascript/crc/crc_js.html
        static class Crc8
        {
            static readonly byte[] table = new byte[256];

            const byte init = 0x00;
            const byte poly = 0x07;
            const byte finalXor = 0x55;

            static Crc8()
            {
                for (var i = 0; i < 256; ++i)
                {
                    var temp = i;
                    for (var j = 0; j < 8; ++j)
                        if ((temp & 0x80) != 0)
                            temp = (temp << 1) ^ poly;
                        else
                            temp <<= 1;
                    table[i] = (byte)temp;
                }
            }

            public static byte ComputeChecksum(byte[] bytes, int index, int count)
            {
                var crc = init;
                var length = index + count;
                for (var i = index; i < length; i++)
                    crc = table[crc ^ bytes[i]];
                return (byte)(crc ^ finalXor);
            }
        }

        #region Passwords

        const int pwdIterCount = 10000;
        const int pwdSubkeyLength = 256 / 8;
        const int pwdSaltSize = 128 / 8;

        // based on: https://github.com/aspnet/Identity/blob/release/2.0.0/src/Microsoft.Extensions.Identity.Core/PasswordHasher.cs
        public static string HashPassword(string password)
        {
            var salt = new byte[pwdSaltSize];
            Rng.GetBytes(salt);

            var subkey = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, pwdIterCount, pwdSubkeyLength);

            var outputBytes = new byte[salt.Length + subkey.Length];

            Buffer.BlockCopy(salt, 0, outputBytes, 0, salt.Length);
            Buffer.BlockCopy(subkey, 0, outputBytes, pwdSaltSize, subkey.Length);

            return Convert.ToBase64String(outputBytes);
        }

        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            if (hashedPassword == null)
                throw new ArgumentNullException(nameof(hashedPassword));

            if (password == null)
                throw new ArgumentNullException(nameof(password));

            var hashedPasswordBytes = Convert.FromBase64String(hashedPassword);

            if (hashedPasswordBytes.Length != pwdSaltSize + pwdSubkeyLength)
                throw new FormatException("Hashed password size is invalid.");

            var salt = new byte[pwdSaltSize];
            Buffer.BlockCopy(hashedPasswordBytes, 0, salt, 0, pwdSaltSize);

            var storedSubkey = new byte[pwdSubkeyLength];
            Buffer.BlockCopy(hashedPasswordBytes, pwdSaltSize, storedSubkey, 0, pwdSubkeyLength);

            var generatedSubkey = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, pwdIterCount, pwdSubkeyLength);

            return ArrayUtils.ContentEquals(storedSubkey, generatedSubkey);
        }
        #endregion

        #region Tokens

        public static string GenerateToken(int tokenSizeInBytes)
        {
            var tokenBytes = new byte[tokenSizeInBytes];
            Rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes);
        }
        #endregion

        #region Reference Numbers
        const byte refNoMagic = 0x7;
        const int refNoLength = sizeof(int) + 1;
        // http://stackoverflow.com/questions/2745074/fast-ceiling-of-an-integer-division-in-c-c
        const int refNoZBase32Length = (8 * refNoLength + 5 - 1) / 5;
        const string refNoPostfixChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string RefNoFromId(int id, int randomPostfixLength = 0)
        {
            var bytes = new byte[refNoLength];
            bytes[4] = (byte)(id >> 24 & 0xFF);
            bytes[3] = (byte)(id >> 16 & 0xFF);
            bytes[2] = (byte)(id >> 8 & 0xFF);
            bytes[1] = (byte)(id & 0xFF);
            bytes[0] = Crc8.ComputeChecksum(bytes, 1, 4);

            var b = refNoMagic;
            for (var i = 0; i < refNoLength; i++)
            {
                b ^= bytes[i];
                bytes[i] = b;
            }
            var result = ZBase32Encoder.Encode(bytes);

            if (randomPostfixLength != 0)
            {
                var postfix = new char[randomPostfixLength];
                var randomBytes = new byte[4];
                for (var i = 0; i < randomPostfixLength; i++)
                {                    
                    Rng.GetBytes(randomBytes);

                    var randomValue = 
                        randomBytes[0] | 
                        randomBytes[1] << 8 | 
                        randomBytes[2] << 16 | 
                        (randomBytes[3] & 0x7F) << 24;

                    randomValue %= refNoPostfixChars.Length;

                    postfix[i] = refNoPostfixChars[randomValue];
                }
                result += new string(postfix);
            }

            return result;
        }

        public static int IdFromRefNo(string refNo)
        {
            var bytes = ZBase32Encoder.Decode(refNo.Substring(0, refNoZBase32Length));

            var b = refNoMagic;
            for (var i = 0; i < refNoLength; i++)
            {
                bytes[i] = (byte)(b ^ bytes[i]);
                b ^= bytes[i];
            }

            if (bytes[0] != Crc8.ComputeChecksum(bytes, 1, 4))
                throw new InvalidDataException();

            return
                bytes[1] |
                bytes[2] << 8 |
                bytes[3] << 16 |
                bytes[4] << 24;
        }
        #endregion
    }
}
