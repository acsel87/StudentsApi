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

        ResponseModel<List<StudentModel>> GetStudents();
        ResponseModel<List<TeacherModel>> GetTeachers();
        ResponseModel<List<GradeModel>> GetGrades(int studentID, int teacherID);
        ResponseModel<int> GetStudentRating(int studentID, int teacherID);

        string InsertAccessToken(int userID, string accessToken);
    }
}