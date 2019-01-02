using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Student.Models
{
    public class UserModel
    {
        public string Username { get; set; }
        public string AccessToken { get; set; }
    }
}