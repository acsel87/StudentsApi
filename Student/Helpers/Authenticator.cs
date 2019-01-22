using JWT;
using JWT.Builder;
using Newtonsoft.Json;
using Student.DataAccess;
using Student.Models;
using System;
using System.Collections.Generic;

namespace Student.Helpers
{
    public class Authenticator
    {        
        public string ResponseSerializer<T>(ResponseModel<T> responseModel)
        {
            return JsonConvert.SerializeObject(responseModel);
        }

        public bool VerifyToken(string accessToken, ref string output)
        {
            try
            {
                var resultedJson = new JwtBuilder()                    
                    .WithSecret(GlobalConfig.secretKey)
                    .MustVerifySignature()
                    .Decode<IDictionary<string, string>>(accessToken);

                output = resultedJson["sub"];
                return true;
            }
            catch (TokenExpiredException)
            {
                output = "Access expired";
                return false;
            }
            catch (SignatureVerificationException)
            {
                output = "Session authentication failed";
                return false;
            }
            catch (InvalidOperationException ex)
            {
                output = ex.Message;
                return false;
            }
        }

        public string GetUserIDFromExpiredToken(string accessToken)
        {
            string output = string.Empty;

            try
            {
                // should ignore only exp date, but check singature                         
                var resultedJson = new JwtBuilder()
                   .WithSecret(GlobalConfig.secretKey)
                   .DoNotVerifySignature() // unable to validate only token lifetime
                   .Decode<IDictionary<string, string>>(accessToken);

                output = resultedJson["sub"];
            }
            catch (InvalidOperationException ex)
            {
                output = ex.Message;
            }

            return output;
        }
    }
}