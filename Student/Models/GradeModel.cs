using System;

namespace Student.Models
{
    public class GradeModel
    {
        public int GradeID { get; set; }
        public int StudentID { get; set; }
        public int TeacherID { get; set; }
        public string Grade { get; set; }
        public DateTime GradeDate { get; set; }
        public string GradeNotes { get; set; }        
    }
}