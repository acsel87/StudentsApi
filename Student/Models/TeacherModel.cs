using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Student.Models
{
    public class TeacherModel
    {
        public int TeacherID { get; set; }
        public int UserID { get; set; }
        public string TeacherName { get; set; }
        public string Class { get; set; }
        public int TeacherRating { get; set; }
    }
}