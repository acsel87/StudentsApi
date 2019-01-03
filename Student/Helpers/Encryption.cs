using Scrypt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using Newtonsoft.Json;
using Student.DataAccess;
using System.Text;
using JWT.Builder;
using JWT.Algorithms;
using Student.Models;

namespace Student.Helpers
{
    public class Encryption
    {
        // todo - is it always 103 for scrypt?
        private const int hashLength = 103;

        public string GenerateHash(string password, string salt)
        {
            if (string.IsNullOrEmpty(salt))
            {
                int minSaltSize = 16;
                int maxSaltSize = 32;

                Random random = new Random();
                int saltSize = random.Next(minSaltSize, maxSaltSize);

                byte[] saltBytes = new byte[saltSize];

                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

                rng.GetNonZeroBytes(saltBytes);

                salt = Convert.ToBase64String(saltBytes);
            }

            // calculate iterationCount = 16384, blockSize = 8, threadCount = 1
            ScryptEncoder encoder = new ScryptEncoder(16384, 8, 1);
            string passwordWithSaltString = salt + password;
            string hashedPasswordWithSalt = salt + encoder.Encode(passwordWithSaltString);

            return hashedPasswordWithSalt;
        }

        public bool IsHashValid(string currentPassword, string hashedPasswordWithSalt)
        {
            string saltString = hashedPasswordWithSalt.Substring(0, hashedPasswordWithSalt.Length - hashLength);
            string hashString = hashedPasswordWithSalt.Substring(hashedPasswordWithSalt.Length - hashLength, hashLength);

            ScryptEncoder encoder = new ScryptEncoder(16384, 8, 1);
            return encoder.Compare(saltString + currentPassword, hashString);
        }

        public string CreateAccessToken(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                return new JwtBuilder()
                          .WithAlgorithm(new HMACSHA256Algorithm())
                          .WithSecret(GlobalConfig.secretKey)
                          .AddClaim("sub", username)
                          .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
                          .AddClaim("asd", "12345")
                          .Build(); ;
            }

            return null;
        }       

        // for other hash algorithms against timing attack
        private bool CompareBytes(byte[] a, byte[] b)
        {
            if (a != null && a.Length == b.Length)
            {
                int diff = a.Length ^ b.Length;
                for (int i = 0; i < a.Length && i < b.Length; i++)
                    diff |= a[i] ^ b[i];
                return diff == 0;
            }
            return false;
        }
    }
}