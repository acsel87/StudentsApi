using Student.Models;
using System.Collections.Generic;

namespace Student.DataAccess
{
    public interface IDataConnection
    {
        ResponseModel<UserModel> LoginUser(string username, string password);
        ResponseModel<string> SignUpUser(string username, string password, int accountTypeID);

        ResponseModel<List<StudentModel>> GetStudents();
        ResponseModel<List<TeacherModel>> GetTeachers();
        ResponseModel<List<GradeModel>> GetGrades(int studentID, int teacherID);
        ResponseModel<int> GetStudentRating(int studentID, int teacherID);
        ResponseModel<string> RateTeacher(int studentID, int teacherID, int rate);
        ResponseModel<List<KeyValuePair<int, string>>> GetAccountTypes();

        string InsertAccessToken(int userID, string accessToken);
    }
}