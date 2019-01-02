using Student.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student.DataAccess
{
    public interface IDataConnection
    {
        ResponseModel<UserModel> LoginUser(string username, string password);
        ResponseModel<string> SignUpUser(string username, string password, string accountType);

        string InsertAccessToken(int userID, string accessToken);
    }
}
