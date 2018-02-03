using AspNetSkeleton.Common.Utils;
using Karambolo.Common;
using System;
using System.IO;
using System.Security.Cryptography;

namespace AspNetSkeleton.Base.Utils
{
    public static class SecurityUtils
    {
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
        const int pbkdf2Count = 1000;
        const int pbkdf2SubkeyLength = 256 / 8;
        const int saltSize = 128 / 8;

        /* =======================
         * HASHED PASSWORD FORMATS
         * =======================
         * 
         * Version 0:
         * PBKDF2 with HMAC-SHA1, 128-bit salt, 256-bit subkey, 1000 iterations.
         * (See also: SDL crypto guidelines v5.1, Part III)
         * Format: { 0x00, salt, subkey }
         */

        public static string HashPassword(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            byte[] salt, subkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, saltSize, pbkdf2Count))
            {
                salt = deriveBytes.Salt;
                subkey = deriveBytes.GetBytes(pbkdf2SubkeyLength);
            }

            var outputBytes = new byte[1 + saltSize + pbkdf2SubkeyLength];
            Buffer.BlockCopy(salt, 0, outputBytes, 1, saltSize);
            Buffer.BlockCopy(subkey, 0, outputBytes, 1 + saltSize, pbkdf2SubkeyLength);

            return Convert.ToBase64String(outputBytes);
        }

        // hashedPassword must be of the format of HashWithPassword (salt + Hash(salt+input)
        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            if (hashedPassword == null)
                throw new ArgumentNullException(nameof(hashedPassword));

            if (password == null)
                throw new ArgumentNullException(nameof(password));

            var hashedPasswordBytes = Convert.FromBase64String(hashedPassword);

            // Verify a version 0 (see comment above) password hash.
            if (hashedPasswordBytes.Length != (1 + saltSize + pbkdf2SubkeyLength) || hashedPasswordBytes[0] != (byte)0x00)
            {
                // Wrong length or version header.
                return false;
            }

            var salt = new byte[saltSize];
            Buffer.BlockCopy(hashedPasswordBytes, 1, salt, 0, saltSize);

            var storedSubkey = new byte[pbkdf2SubkeyLength];
            Buffer.BlockCopy(hashedPasswordBytes, 1 + saltSize, storedSubkey, 0, pbkdf2SubkeyLength);

            byte[] generatedSubkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, pbkdf2Count))
                generatedSubkey = deriveBytes.GetBytes(pbkdf2SubkeyLength);

            return ArrayUtils.ContentEquals(storedSubkey, generatedSubkey);
        }
        #endregion

        #region Tokens
        public static string GenerateToken(int tokenSizeInBytes)
        {
            using (var rng = new RNGCryptoServiceProvider())
                return GenerateToken(rng, tokenSizeInBytes);
        }

        public static string GenerateToken(RandomNumberGenerator generator, int tokenSizeInBytes)
        {
            var tokenBytes = new byte[tokenSizeInBytes];
            generator.GetBytes(tokenBytes);
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
                var random = new Random();
                var postfix = new char[randomPostfixLength];
                for (var i = 0; i < randomPostfixLength; i++)
                    postfix[i] = refNoPostfixChars[random.Next(refNoPostfixChars.Length)];
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
