using Student.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Student
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IStudentService" in both code and config file together.
    [ServiceContract]
    public interface IStudentService
    {
        [OperationContract]
        string CheckConnection();

        [OperationContract]
        ResponseModel<string> SignUp(string username, string password, string accountType);

        [OperationContract]
        ResponseModel<UserModel> Login(string username, string password);

        [OperationContract]
        string ResetPassword();

        [OperationContract]
        string GetStudents();

        [OperationContract]
        string GetTeachers();

        [OperationContract]
        string GetGrades(int studentID, int teacherID);

        [OperationContract]
        string GetStudentRating(int studentID, int teacherID);
    }
}
