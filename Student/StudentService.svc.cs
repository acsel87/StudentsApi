using Student.DataAccess;
using Student.Helpers;
using Student.Models;
using System;
using System.Collections.Generic;

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

        public string Login(string username, string password)
        {
            Authenticator authenticator = new Authenticator();

            ResponseModel<UserModel> responseModel = GlobalConfig.Connection.LoginUser(username, password);

            return authenticator.ResponseSerializer(responseModel);
        }

        public string ResetPassword()
        {
            throw new NotImplementedException();
        }

        public string SignUp(string username, string password, int accountTypeID)
        {
            Authenticator authenticator = new Authenticator();

            ResponseModel<string> responseModel = GlobalConfig.Connection.SignUpUser(username, password, accountTypeID);

            return authenticator.ResponseSerializer(responseModel);
        }

        public string GetAccountTypes()
        {
            Authenticator authenticator = new Authenticator();

            ResponseModel<List<KeyValuePair<int, string>>> responseModel = GlobalConfig.Connection.GetAccountTypes();

            return authenticator.ResponseSerializer(responseModel);
        }

        public string GetStudents(string accessToken)
        {
            Authenticator authenticator = new Authenticator();

            ResponseModel<List<StudentModel>> responseModel = new ResponseModel<List<StudentModel>>();

            string message = string.Empty;

            if (authenticator.VerifyToken(accessToken, ref message))
            {
                responseModel = GlobalConfig.Connection.GetStudents();                
            }
            else
            {
                responseModel.ErrorMessage = message;
            }

            return authenticator.ResponseSerializer(responseModel);
        }

        public string GetTeachers(string accessToken)
        {
            Authenticator authenticator = new Authenticator();

            ResponseModel<List<TeacherModel>> responseModel = new ResponseModel<List<TeacherModel>>();

            string message = string.Empty;

            if (authenticator.VerifyToken(accessToken, ref message))
            {
                responseModel = GlobalConfig.Connection.GetTeachers();
            }
            else
            {
                responseModel.ErrorMessage = message;
            }

            return authenticator.ResponseSerializer(responseModel);
        }

        public string GetGrades(int studentID, int teacherID, string accessToken)
        {
            Authenticator authenticator = new Authenticator();

            ResponseModel<List<GradeModel>> responseModel = new ResponseModel<List<GradeModel>>();

            string message = string.Empty;

            if (authenticator.VerifyToken(accessToken, ref message))
            {
                responseModel = GlobalConfig.Connection.GetGrades(studentID, teacherID);                
            }
            else
            {
                responseModel.ErrorMessage = message;
                responseModel.ErrorAction = "[LogOut]";
            }

            return authenticator.ResponseSerializer(responseModel);
        }

        public string GetStudentRating(int studentID, int teacherID, string accessToken)
        {
            Authenticator authenticator = new Authenticator();

            ResponseModel<int> responseModel = new ResponseModel<int>();

            string message = string.Empty;

            if (authenticator.VerifyToken(accessToken, ref message))
            {
                responseModel = GlobalConfig.Connection.GetStudentRating(studentID, teacherID);                
            }
            else
            {
                responseModel.ErrorMessage = message;
                responseModel.ErrorAction = "[LogOut]";
            }

            return authenticator.ResponseSerializer(responseModel);
        }

        public string RateTeacher(int studentID, int teacherID, int rate, string accessToken)
        {
            Authenticator authenticator = new Authenticator();

            ResponseModel<string> responseModel = new ResponseModel<string>();

            string message = string.Empty;

            if (authenticator.VerifyToken(accessToken, ref message))
            {
                if (authenticator.CheckActivity(message, "RateTeacher")) // check activity
                {
                    responseModel = GlobalConfig.Connection.RateTeacher(studentID, teacherID, rate);
                }
                else // account type not mapped to activity
                {
                    responseModel.ErrorMessage = "You're not allowed, bad kitty !";
                }
            }
            else
            {
                responseModel.ErrorMessage = message;                
            }

            return authenticator.ResponseSerializer(responseModel);
        }
    }
}