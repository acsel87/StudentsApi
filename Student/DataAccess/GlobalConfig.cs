using System;
using System.Configuration;
using System.Text;

namespace Student.DataAccess
{
    public static class GlobalConfig
    {
        public static readonly string secretKey = "NdR4Ce6gS7fGrFkPagzFj5gn7qfDRWt25GDspxxCEuTEFtKvW3yJg2xZZkDtyyYDkEPEdFLqRDf4wUkb7tB2cpxyjWD9EVXQhm6ecUxHaAsaWjyJMGKmbJSsBR7EvwrY";
         
        public static IDataConnection Connection { get; private set; }

        public static string CnnString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        public static void SetConnection(IDataConnection dataConnection)
        {
            Connection = dataConnection;
        }

        public static long GetAccessTokenExpDate()
        {
            //return DateTimeOffset.UtcNow.AddSeconds(10).ToUnixTimeSeconds();
            return DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
        }

        public static DateTime GetRefreshTokenExpDate()
        {
            //return DateTime.UtcNow.AddMinutes(1);
            return DateTime.UtcNow.AddMonths(1);
        }

        public static DateTime GetResetTokenExpDate()
        {            
            //return DateTime.UtcNow.AddSeconds(5);
            return DateTime.UtcNow.AddMinutes(30);
        }
    }
}