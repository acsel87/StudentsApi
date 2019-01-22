using Student.Models;
using System.Collections.Generic;

namespace Student.DataAccess
{
    public interface IDataConnection
    {
        ResponseModel<UserModel> LoginUser(string username, string password);
        ResponseModel<string> SignUpUser(string username, string password, int accountTypeID);
        void SignOut(string accessToken);

        ResponseModel<long> GetNewAccessToken(string refreshToken, string accessToken);

        ResponseModel<List<StudentModel>> GetStudents();
        ResponseModel<List<TeacherModel>> GetTeachers();
        ResponseModel<List<GradeModel>> GetGrades(int studentID, int teacherID);
        ResponseModel<int> GetStudentRating(string userID, int teacherID);
        ResponseModel<string> RateTeacher(string userID, int teacherID, int rate);
        ResponseModel<List<KeyValuePair<int, string>>> GetAccountTypes();
        ResponseModel<string> AddGrade(GradeModel gradeModel, string userID);
        ResponseModel<string> EditGrade(GradeModel gradeModel);

        bool CheckActivity(string userID, string activity, ref string errorMessage);

        ResponseModel<string[]> ResetPassword_SendInstructions(string username);
        ResponseModel<string> ActivateTokenLink(string username, string token);
        ResponseModel<string> ConfirmPasswordReset(string userToken, string newPassword);
    }
}