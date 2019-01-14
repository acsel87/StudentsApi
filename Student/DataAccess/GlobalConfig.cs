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
    }
}