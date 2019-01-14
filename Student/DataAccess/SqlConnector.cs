using Dapper;
using Student.Helpers;
using Student.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Student.DataAccess
{
    public class SqlConnector : IDataConnection
    {
        private const string loginAccess = "LoginAccess";
        private const string signUpAccess = "SignUpAccess";
        private const string resetAccess = "ResetAccess";
        private const string userAccess = "UserAccess";

        public ResponseModel<List<KeyValuePair<int, string>>> GetAccountTypes()
        {
            ResponseModel<List<KeyValuePair<int, string>>> responseModel = new ResponseModel<List<KeyValuePair<int, string>>>();

            List<KeyValuePair<int, string>> accountTypes = new List<KeyValuePair<int, string>>();

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(signUpAccess)))
            {
                accountTypes = connection.Query<KeyValuePair<int, string>>("dbo.spAccountTypes_Get", commandType: CommandType.StoredProcedure).ToList();

                responseModel.IsSuccess = true;
                responseModel.Model = accountTypes;
            }

            return responseModel;
        }

        public ResponseModel<List<GradeModel>> GetGrades(int studentID, int teacherID)
        {
            ResponseModel<List<GradeModel>> responseModel = new ResponseModel<List<GradeModel>>();

            List<GradeModel> grades = new List<GradeModel>();

            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(userAccess)))
                {

                    var p = new DynamicParameters();
                    p.Add("@StudentID", studentID);
                    p.Add("@TeacherID", teacherID);

                    grades = connection.Query<GradeModel>("dbo.spGrades_GetByStudentTeacher", p, commandType: CommandType.StoredProcedure).ToList();

                    responseModel.Model = grades;
                    responseModel.IsSuccess = true;
                }
            }
            catch (Exception e)
            {
                responseModel.ErrorMessage = e.Message;
            }

            return responseModel;
        }

        public ResponseModel<int> GetStudentRating(int studentID, int teacherID)
        {
            ResponseModel<int> responseModel = new ResponseModel<int>();

            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(userAccess)))
                {
                    var p = new DynamicParameters();
                    p.Add("@StudentID", studentID);
                    p.Add("@TeacherID", teacherID);

                    responseModel.Model = connection.Query<int>("dbo.spStudentRating_Get", p, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    responseModel.IsSuccess = true;
                }
            }
            catch (Exception e)
            {
                responseModel.ErrorMessage = e.Message;
            }

            return responseModel;
        }

        public ResponseModel<List<StudentModel>> GetStudents()
        {
            ResponseModel<List<StudentModel>> responseModel = new ResponseModel<List<StudentModel>>();

            List<StudentModel> students = new List<StudentModel>();

            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(userAccess)))
                {
                    students = connection.Query<StudentModel>("dbo.spStudents_GetAll", commandType: CommandType.StoredProcedure).ToList();

                    responseModel.Model = students;
                    responseModel.IsSuccess = true;
                }
            }
            catch (Exception e)
            {
                responseModel.ErrorMessage = e.Message;
            }

            return responseModel;
        }

        public ResponseModel<List<TeacherModel>> GetTeachers()
        {
            ResponseModel<List<TeacherModel>> responseModel = new ResponseModel<List<TeacherModel>>();

            List<TeacherModel> teachers = new List<TeacherModel>();

            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(userAccess)))
                {
                    teachers = connection.Query<TeacherModel>("dbo.spTeachers_GetAll", commandType: CommandType.StoredProcedure).ToList();

                    responseModel.Model = teachers;
                    responseModel.IsSuccess = true;
                }
            }
            catch (Exception e)
            {
                responseModel.ErrorMessage = e.Message;
            }

            return responseModel;
        }

        public ResponseModel<UserModel> LoginUser(string username, string password)
        {
            ResponseModel<UserModel> responseModel = new ResponseModel<UserModel>();

            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(loginAccess)))
                {
                    var p = new DynamicParameters();
                    p.Add("@Username", username);

                    KeyValuePair<int,string> userID_Pass = connection.Query<KeyValuePair<int, string>>("dbo.spUser_GetID_Pass", p, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    
                    if (userID_Pass.Key > 0 && !string.IsNullOrEmpty(userID_Pass.Value))
                    {
                        Encryptor encryption = new Encryptor();
                        responseModel.IsSuccess = encryption.IsHashValid(password, userID_Pass.Value);

                        if (responseModel.IsSuccess)
                        {
                            UserModel tempModel = new UserModel();

                            string token = encryption.CreateAccessToken(username); // maybe better on client side?

                            InsertAccessToken(Convert.ToInt32(userID_Pass.Key), token);

                            responseModel.ErrorMessage = "Login successful";

                            tempModel.Username = username;
                            tempModel.AccessToken = token;

                            responseModel.Model = tempModel;
                        }
                        else
                        {
                            responseModel.ErrorMessage = "Invalid user or password";
                        }
                    }
                    else
                    {
                        responseModel.ErrorMessage = "Invalid user or password";
                    }
                }
            }
            catch (Exception e)
            {
                responseModel.ErrorMessage = e.Message;
            }

            return responseModel;
        }

        public ResponseModel<string> RateTeacher(string accessToken, int teacherID, int rate)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>() { Model = string.Empty };

            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(userAccess)))
                {
                    var p = new DynamicParameters();
                    p.Add("@AccessToken", accessToken);
                    p.Add("@TeacherID", teacherID);
                    p.Add("@Rate", rate);

                    responseModel.Model = connection.Query<string>("dbo.spRateTeacher", p, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    responseModel.IsSuccess = true;
                }
            }
            catch (Exception e)
            {
                responseModel.ErrorMessage = e.Message;
            }

            return responseModel;
        }

        public ResponseModel<string> SignUpUser(string username, string password, int accountTypeID)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>() { Model = string.Empty };

            try
            {
                if (!CheckUsername(username))
                {
                    Encryptor encryption = new Encryptor();
                    string passwordHash = encryption.GenerateHash(password);

                    using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(signUpAccess)))
                    {
                        var p = new DynamicParameters();
                        p.Add("@Username", username);
                        p.Add("@Password", passwordHash);
                        p.Add("@AccountTypeID", accountTypeID);

                        connection.Execute("dbo.spUser_Insert", p, commandType: CommandType.StoredProcedure);

                        responseModel.IsSuccess = true;
                        responseModel.Model = "User " + username + " was added successfully";
                    }
                }
                else
                {
                    responseModel.ErrorMessage = "Username taken";
                }                
            }
            catch (Exception e)
            {
                responseModel.ErrorMessage = e.Message;
            }

            return responseModel;
        }
        
        public ResponseModel<string> AddGrade(GradeModel gradeModel, string accessToken)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>();

            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(userAccess)))
                {
                    var p = new DynamicParameters();
                    p.Add("@StudentID", gradeModel.StudentID);
                    p.Add("@TeacherID", gradeModel.TeacherID);
                    p.Add("@Grade", gradeModel.Grade);
                    p.Add("@GradeDate", DateTime.UtcNow);
                    p.Add("@GradeNotes", gradeModel.GradeNotes);
                    p.Add("@AccessToken", accessToken);

                    responseModel.Model = connection.Query<string>("dbo.spGrades_Insert", p, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    responseModel.IsSuccess = true;
                }
            }
            catch (Exception e)
            {
                responseModel.ErrorMessage = e.Message;
            }

            return responseModel;
        }

        public ResponseModel<string> EditGrade(GradeModel gradeModel)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>();

            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(userAccess)))
                {
                    var p = new DynamicParameters();
                    p.Add("@GradeID", gradeModel.GradeID);                    
                    p.Add("@Grade", gradeModel.Grade);
                    p.Add("@GradeDate", DateTime.UtcNow);
                    p.Add("@GradeNotes", gradeModel.GradeNotes);

                    responseModel.Model = connection.Query<string>("dbo.spGrades_Update", p, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    responseModel.IsSuccess = true;
                }
            }
            catch (Exception e)
            {
                responseModel.ErrorMessage = e.Message;
            }

            return responseModel;
        }

        public ResponseModel<string[]> ResetPassword_SendInstructions(string username)
        {
            ResponseModel<string[]> responseModel = new ResponseModel<string[]>() { Model = new string[2]};
            
            try
            {
                if (CheckUsername(username))
                {                   
                    using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(resetAccess)))
                    {
                        Encryptor encryptor = new Encryptor();

                        string token = encryptor.GenerateRNG(8, 16);
                        string hashToken = encryptor.GenerateHash(token);
                        DateTime expDate = DateTime.UtcNow.AddMinutes(30);
                        //DateTime expDate = DateTime.UtcNow.AddSeconds(5);

                        var p = new DynamicParameters();
                        p.Add("@Username", username);
                        p.Add("@HashToken", hashToken);
                        p.Add("@ExpDate", expDate);

                        connection.Execute("dbo.spResetTicket_Insert", p, commandType: CommandType.StoredProcedure);

                        responseModel.Model[0] = username;
                        responseModel.Model[1] = token;
                    }
                }

                responseModel.IsSuccess = true;
            }
            catch (Exception e)
            {
                responseModel.ErrorMessage = e.Message;
            }

            return responseModel;
        }

        public ResponseModel<string> ActivateTokenLink(string username, string token)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>();

            try
            {   
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(resetAccess)))
                {
                    Encryptor encryptor = new Encryptor();

                    var p = new DynamicParameters();
                    p.Add("@Username", username);

                    string hashedToken = connection.Query<string>("dbo.spResetTicket_GetToken", p, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    
                    if (!string.IsNullOrEmpty(hashedToken) && encryptor.IsHashValid(token, hashedToken))
                    {  
                        responseModel.Model = username;
                    }
                    else
                    {
                        responseModel.ErrorAction = "[LinkExpired]";
                    }

                    responseModel.IsSuccess = true;
                }               
            }
            catch (Exception e)
            {
                responseModel.ErrorMessage = e.Message;
            }

            return responseModel;
        }

        public ResponseModel<string> ConfirmPasswordReset(string userToken, string newPassword)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>();

            try
            {
                Authenticator authenticator = new Authenticator();

                string message = string.Empty;
                string hashedPassword; 

                if (authenticator.VerifyToken(userToken, ref message))
                {
                    Encryptor encryptor = new Encryptor();

                    // get hashed Password if link not exp
                    using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(resetAccess)))
                    {
                        var p = new DynamicParameters();
                        p.Add("@Username", message);

                        hashedPassword = connection.Query<string>("dbo.spUser_GetPassword_LinkNotExp", p, commandType:
                            CommandType.StoredProcedure).FirstOrDefault();                                               
                    }

                    if (!string.IsNullOrEmpty(hashedPassword))
                    {
                        if (!encryptor.IsHashValid(newPassword, hashedPassword))
                        {
                            // change password and delete ticket
                            string newPasswordHash = encryptor.GenerateHash(newPassword);
                            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(resetAccess)))
                            {
                                var p = new DynamicParameters();
                                p.Add("@Username", message);
                                p.Add("@Password", newPasswordHash);

                                connection.Execute("dbo.spUser_ChangePassword", p, commandType:
                                    CommandType.StoredProcedure);

                                responseModel.IsSuccess = true;
                                responseModel.Model = "Password changed successfully!";
                            }
                        }
                        else // new password is the same as old one
                        {
                            responseModel.ErrorMessage = "New password must be different from the old one";
                        }                        
                    }
                    else 
                    {
                        responseModel.ErrorAction = "[LinkExpired]";                        
                    }
                }
                else
                {
                    responseModel.ErrorMessage = message;
                }               
            }
            catch (Exception e)
            {
                responseModel.ErrorMessage = e.Message;
            }

            return responseModel;
        }

        public bool CheckActivity(string username, string activity, ref string errorMessage)
        {
            bool isAllowed = false;
            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(userAccess)))
                {
                    var p = new DynamicParameters();
                    p.Add("@Username", username);
                    p.Add("@Activity", activity);

                    isAllowed = connection.Query<bool>("dbo.spCheckActivity", p, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    if (!isAllowed)
                    {
                        errorMessage = "You're not allowed, bad kitty !";
                    }
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }

            return isAllowed;
        }

        private void InsertAccessToken(int userID, string accessToken)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(loginAccess)))
            {
                var p = new DynamicParameters();
                p.Add("@UserID", userID);
                p.Add("@AccessToken", accessToken);

                connection.Execute("dbo.spToken_Insert", p, commandType: CommandType.StoredProcedure);
            }
        }

        private bool CheckUsername(string username)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(signUpAccess)))
            {
                var p = new DynamicParameters();
                p.Add("@Username", username);

                return connection.Query<bool>("dbo.spUser_CheckIfExist", p, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }
        }
    }
}