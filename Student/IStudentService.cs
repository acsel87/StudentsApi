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
        string ResetPassword();

        [OperationContract]
        string GetStudents(string accessToken);

        [OperationContract]
        string GetTeachers(string accessToken);

        [OperationContract]
        string GetGrades(int studentID, int teacherID, string accessToken);

        [OperationContract]
        string GetStudentRating(int studentID, int teacherID, string accessToken);

        [OperationContract]
        string RateTeacher(int studentID, int teacherID, int rate, string accessToken);
    }
}
