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
            //GlobalConfig.SetConnection(new SqlConnector());
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

        public string ResetPassword(string username)
        {
            Authenticator authenticator = new Authenticator();

            ResponseModel<string[]> responseModel = GlobalConfig.Connection.ResetPassword_SendInstructions(username);

            return authenticator.ResponseSerializer(responseModel);
        }

        public string ActivateLink(string username, string token)
        {
            Authenticator authenticator = new Authenticator();

            ResponseModel<string> responseModel = GlobalConfig.Connection.ActivateTokenLink(username, token);

            return authenticator.ResponseSerializer(responseModel);
        }

        public string ConfirmReset(string userToken, string newPassword)
        {
            Authenticator authenticator = new Authenticator();

            ResponseModel<string> responseModel = GlobalConfig.Connection.ConfirmPasswordReset(userToken, newPassword);

            return authenticator.ResponseSerializer(responseModel);
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

        public string RateTeacher(int teacherID, int rate, string accessToken)
        {
            Authenticator authenticator = new Authenticator();

            ResponseModel<string> responseModel = new ResponseModel<string>();

            string message = string.Empty;

            if (authenticator.VerifyToken(accessToken, ref message))
            {
                if (GlobalConfig.Connection.CheckActivity(message, "RateTeacher", ref message))
                {
                    responseModel = GlobalConfig.Connection.RateTeacher(accessToken, teacherID, rate);
                }
                else
                {
                    responseModel.ErrorMessage = message;
                }
            }
            else
            {
                responseModel.ErrorMessage = message;
                responseModel.ErrorAction = "[LogOut]";
            }

            return authenticator.ResponseSerializer(responseModel);
        }

        public string ModifyGrades(bool isNewGrade, GradeModel gradeModel, string accessToken)
        {
            Authenticator authenticator = new Authenticator();

            ResponseModel<string> responseModel = new ResponseModel<string>();

            string message = string.Empty;
            string activity = isNewGrade ? "AddGrade":"EditGrade";

            if (authenticator.VerifyToken(accessToken, ref message) )
            {
                if (GlobalConfig.Connection.CheckActivity(message, activity, ref message))
                {
                    if (isNewGrade)
                    {
                        responseModel = GlobalConfig.Connection.AddGrade(gradeModel, accessToken);
                    }
                    else
                    {
                        responseModel = GlobalConfig.Connection.EditGrade(gradeModel);
                    }
                }
                else
                {
                    responseModel.ErrorMessage = message;
                }
            }
            else
            {
                responseModel.ErrorMessage = message;
                responseModel.ErrorAction = "[LogOut]";
            }

            return authenticator.ResponseSerializer(responseModel);
        }
    }
}