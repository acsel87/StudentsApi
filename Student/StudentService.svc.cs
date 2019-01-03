using Student.DataAccess;
using Student.Helpers;
using Student.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Student
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "StudentService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select StudentService.svc or StudentService.svc.cs at the Solution Explorer and start debugging.
    public class StudentService : IStudentService
    {
        public StudentService()
        {
            GlobalConfig.SetConnection(new CsvConnector());
           // GlobalConfig.SetConnection(new SqlConnector());
        }

        public string CheckConnection()
        {            
            return "Connected to service";
        }

        public ResponseModel<UserModel> Login(string username, string password)
        {
            return GlobalConfig.Connection.LoginUser(username, password);
        }

        public string ResetPassword()
        {
            throw new NotImplementedException();
        }

        public ResponseModel<string> SignUp(string username, string password, string accountType)
        {   
            return GlobalConfig.Connection.SignUpUser(username, password, accountType);
        }

        public string GetStudents()
        {
            return GlobalConfig.ResponseSerializer(GlobalConfig.Connection.GetStudents());
        }

        public string GetTeachers()
        {
            return GlobalConfig.ResponseSerializer(GlobalConfig.Connection.GetTeachers());
        }

        public string GetGrades(int studentID, int teacherID)
        {
            return GlobalConfig.ResponseSerializer(GlobalConfig.Connection.GetGrades(studentID, teacherID));
        }

        public string GetStudentRating(int studentID, int teacherID)
        {
            return GlobalConfig.ResponseSerializer(GlobalConfig.Connection.GetStudentRating(studentID, teacherID));
        }
    }
}