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
        ResponseModel<string> RateTeacher(string accessToken, int teacherID, int rate);
        ResponseModel<List<KeyValuePair<int, string>>> GetAccountTypes();
        ResponseModel<string> AddGrade(GradeModel gradeModel, string accessToken);
        ResponseModel<string> EditGrade(GradeModel gradeModel);

        bool CheckActivity(string username, string activity, ref string errorMessage);

        ResponseModel<string[]> ResetPassword_SendInstructions(string username);
        ResponseModel<string> ActivateTokenLink(string username, string token);
        ResponseModel<string> ConfirmPasswordReset(string userToken, string newPassword);
    }
}