using JWT.Algorithms;
using JWT.Builder;
using Scrypt;
using Student.DataAccess;
using System;
using System.Security.Cryptography;

namespace Student.Helpers
{
    public class Encryptor
    {
        // todo - is it always 103 for scrypt?
        private const int hashLength = 103;

        public string GenerateHash(string password)
        {
            string salt = GenerateRNG(16,32);

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
                          .WithUrlEncoder(new JWT.JwtBase64UrlEncoder())
                          .WithSecret(GlobalConfig.secretKey)
                          .AddClaim("sub", username)
                          .AddClaim("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds())
                          .Build(); ;
            }

            return null;
        }

        public string GenerateRNG(int minSize, int maxSize)
        {
            Random random = new Random();
            int size = random.Next(minSize, maxSize);

            byte[] bytes = new byte[size];

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            
            rng.GetNonZeroBytes(bytes);      
            
            return Convert.ToBase64String(bytes);            
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