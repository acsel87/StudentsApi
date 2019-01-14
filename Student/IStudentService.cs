using Student.Models;
using System.ServiceModel;

namespace Student
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IStudentService" in both code and config file together.
    [ServiceContract]
    public interface IStudentService
    {
        [OperationContract]
        string CheckConnection();

        [OperationContract]
        string GetAccountTypes();

        [OperationContract]
        string SignUp(string username, string password, int accountTypeID);

        [OperationContract]
        string Login(string username, string password);

        [OperationContract]
        string ResetPassword(string username);

        [OperationContract]
        string ActivateLink(string username, string token);

        [OperationContract]
        string ConfirmReset(string userToken, string newPassword);

        [OperationContract]
        string GetStudents(string accessToken);

        [OperationContract]
        string GetTeachers(string accessToken);

        [OperationContract]
        string GetGrades(int studentID, int teacherID, string accessToken);

        [OperationContract]
        string GetStudentRating(int studentID, int teacherID, string accessToken);

        [OperationContract]
        string RateTeacher(int teacherID, int rate, string accessToken);

        [OperationContract]
        string ModifyGrades(bool isNewGrade, GradeModel gradeModel, string accessToken);
        
    }
}
