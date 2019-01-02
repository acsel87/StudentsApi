using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Student.Models
{
    public class ResponseModel<T>
    {
        public T Model { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = "";
    }
}