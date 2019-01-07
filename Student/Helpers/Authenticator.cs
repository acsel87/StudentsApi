using JWT;
using JWT.Builder;
using Newtonsoft.Json;
using Student.DataAccess;
using Student.Models;
using System.Collections.Generic;

namespace Student.Helpers
{
    public class Authenticator
    {        
        public string ResponseSerializer<T>(ResponseModel<T> responseModel)
        {
            return JsonConvert.SerializeObject(responseModel);
        }

        public bool VerifyToken(string accessToken, ref string message)
        {
            try
            {
                var resultedJson = new JwtBuilder()
                    .WithSecret(GlobalConfig.secretKey)
                    .MustVerifySignature()
                    .Decode<IDictionary<string, string>>(accessToken);

                message = resultedJson["sub"];
                return true;
            }
            catch (TokenExpiredException)
            {
                message = "Session has expired";
                return false;
            }
            catch (SignatureVerificationException)
            {
                message = "Session authentication failed";
                return false;
            }
        }

        public bool CheckActivity(string username, string activity)
        {
            // check if user accounty type is mapped to activity
            return true;
        }
    }
}