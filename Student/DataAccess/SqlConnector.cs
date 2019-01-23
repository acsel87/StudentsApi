using Dapper;
using JWT.Builder;
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

            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(signUpAccess)))
                {
                    accountTypes = connection.Query<KeyValuePair<int, string>>("dbo.spAccountTypes_Get", commandType: CommandType.StoredProcedure).ToList();

                    responseModel.IsSuccess = true;
                    responseModel.Model = accountTypes;
                }
            }
            catch (System.Data.SqlClient.SqlException)
            {
                responseModel.OutputMessage = "Server is down";
            }
            catch (Exception e)
            {
                responseModel.OutputMessage = e.Message;
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
                responseModel.OutputMessage = e.Message;
            }

            return responseModel;
        }

        public ResponseModel<int> GetStudentRating(string userID, int teacherID)
        {
            ResponseModel<int> responseModel = new ResponseModel<int>();

            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(userAccess)))
                {
                    var p = new DynamicParameters();
                    p.Add("@UserID", Convert.ToInt32(userID));
                    p.Add("@TeacherID", teacherID);

                    responseModel.Model = connection.Query<int>("dbo.spStudentRating_Get", p, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    responseModel.IsSuccess = true;
                }

                if (responseModel.Model < 1)
                {
                    responseModel.Model = 0;
                }
            }
            catch (Exception e)
            {
                responseModel.OutputMessage = e.Message;
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
                responseModel.OutputMessage = e.Message;
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
                responseModel.OutputMessage = e.Message;
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
                        Encryptor encryptor = new Encryptor();
                        responseModel.IsSuccess = encryptor.IsHashValid(password, userID_Pass.Value);

                        if (responseModel.IsSuccess)
                        {
                            UserModel tempModel = new UserModel();

                            string refreshToken = encryptor.GenerateRNG(32, 32);
                            string refreshTokenHash = encryptor.GenerateHash(refreshToken);

                            InsertRefreshToken(userID_Pass.Key, refreshTokenHash);

                            tempModel.Username = username;
                            tempModel.RefreshToken = refreshToken;
                            tempModel.AccessToken = encryptor.CreateAccessToken(userID_Pass.Key.ToString());
                            tempModel.AccessToken_ExpDate = GlobalConfig.GetAccessTokenExpDate();

                            responseModel.Model = tempModel;
                            responseModel.OutputMessage = "Login successful";
                        }
                        else
                        {
                            responseModel.OutputMessage = "Invalid user or password";
                        }
                    }
                    else
                    {
                        responseModel.OutputMessage = "Invalid user or password";
                    }
                }
            }
            catch (System.Data.SqlClient.SqlException)
            {
                responseModel.OutputMessage = "Server is down";
            }
            catch (Exception e)
            {
                responseModel.OutputMessage = e.Message;
            }

            return responseModel;
        }

        public ResponseModel<string> RateTeacher(string userID, int teacherID, int rate)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>() { Model = string.Empty };

            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(userAccess)))
                {
                    var p = new DynamicParameters();
                    p.Add("@UserID", Convert.ToInt32(userID));
                    p.Add("@TeacherID", teacherID);
                    p.Add("@Rate", rate);

                    responseModel.Model = connection.Query<string>("dbo.spRateTeacher", p, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    responseModel.IsSuccess = true;
                }
            }
            catch (Exception e)
            {
                responseModel.OutputMessage = e.Message;
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
                    responseModel.OutputMessage = "Username taken";
                }                
            }
            catch (Exception e)
            {
                responseModel.OutputMessage = e.Message;
            }

            return responseModel;
        }
        
        public ResponseModel<string> AddGrade(GradeModel gradeModel, string userID)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>();
           
            try
            {
                string gradeInsertResponse = string.Empty;

                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(userAccess)))
                {
                    var p = new DynamicParameters();
                    p.Add("@UserID", Convert.ToInt32(userID));
                    p.Add("@StudentID", gradeModel.StudentID);
                    p.Add("@TeacherID", gradeModel.TeacherID);
                    p.Add("@Grade", gradeModel.Grade);
                    p.Add("@GradeDate", DateTime.UtcNow);
                    p.Add("@GradeNotes", gradeModel.GradeNotes);                    

                    gradeInsertResponse = connection.Query<string>("dbo.spGrades_Insert", p, commandType: CommandType.StoredProcedure).FirstOrDefault();
                }

                if (string.IsNullOrEmpty(gradeInsertResponse))
                {
                    responseModel.IsSuccess = true;
                }
                else
                {
                    responseModel.OutputMessage = gradeInsertResponse;
                }
            }
            catch (Exception e)
            {
                responseModel.OutputMessage = e.Message;
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
                responseModel.OutputMessage = e.Message;
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

                        var p = new DynamicParameters();
                        p.Add("@Username", username);
                        p.Add("@HashToken", hashToken);
                        p.Add("@ExpDate", GlobalConfig.GetResetTokenExpDate());

                        connection.Execute("dbo.spResetTicket_Insert", p, commandType: CommandType.StoredProcedure);

                        responseModel.Model[0] = username;
                        responseModel.Model[1] = token;
                    }
                }

                responseModel.IsSuccess = true;
            }
            catch (System.Data.SqlClient.SqlException)
            {
                responseModel.OutputMessage = "Server is down";
            }
            catch (Exception e)
            {
                responseModel.OutputMessage = e.Message;
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

                   Tuple<int, string> userID_HashedToken = connection.Query<Tuple<int, string>>("dbo.spResetTicket_GetToken", p, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    
                    if (userID_HashedToken != null && encryptor.IsHashValid(token, userID_HashedToken.Item2))
                    {  
                        responseModel.Model = userID_HashedToken.Item1.ToString();
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
                responseModel.OutputMessage = e.Message;
            }

            return responseModel;
        }

        public ResponseModel<string> ConfirmPasswordReset(string userToken, string newPassword)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>();

            try
            {
                Authenticator authenticator = new Authenticator();

                string tokenOutput = string.Empty;
                string hashedPassword = string.Empty; 

                if (authenticator.VerifyToken(userToken, ref tokenOutput))
                {
                    Encryptor encryptor = new Encryptor();

                    if (int.TryParse(tokenOutput, out int userID))
                    {
                        // get hashed Password if link not exp
                        using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(resetAccess)))
                        {
                            var p = new DynamicParameters();
                            p.Add("@UserID", userID);

                            hashedPassword = connection.Query<string>("dbo.spUser_GetPassword_LinkNotExp", p, commandType:
                                CommandType.StoredProcedure).FirstOrDefault();
                        }
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
                                p.Add("@UserID", tokenOutput);
                                p.Add("@Password", newPasswordHash);

                                connection.Execute("dbo.spUser_ChangePassword", p, commandType:
                                    CommandType.StoredProcedure);

                                responseModel.IsSuccess = true;
                                responseModel.Model = "Password changed successfully!";
                            }
                        }
                        else // new password is the same as old one
                        {
                            responseModel.OutputMessage = "New password must be different from the old one";
                        }                        
                    }
                    else 
                    {
                        responseModel.ErrorAction = "[LinkExpired]";                        
                    }
                }
                else
                {
                    responseModel.OutputMessage = tokenOutput;
                }               
            }
            catch (Exception e)
            {
                responseModel.OutputMessage = e.Message;
            }

            return responseModel;
        }

        public bool CheckActivity(string userID, string activity, ref string errorMessage)
        {
            bool isAllowed = false;
            try
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(userAccess)))
                {
                    var p = new DynamicParameters();
                    p.Add("@UserID", Convert.ToInt32(userID));
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

        private void InsertRefreshToken(int userID, string refreshTokenHash)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(loginAccess)))
            {
                var p = new DynamicParameters();
                p.Add("@UserID", userID);
                p.Add("@RefreshToken", refreshTokenHash);
                p.Add("@ExpDate", GlobalConfig.GetRefreshTokenExpDate());

                connection.Execute("dbo.spRefreshToken_Insert", p, commandType: CommandType.StoredProcedure);
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

        public void SignOut(string accessToken)
        {
            try
            {
                Authenticator authenticator = new Authenticator();

                string userID = string.Empty;

                authenticator.VerifyToken(accessToken, ref userID);

                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(userAccess)))
                {
                    var p = new DynamicParameters();
                    p.Add("@UserID", Convert.ToInt32(userID));

                    connection.Execute("dbo.spUser_SignOut", p, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception)
            {
               // do nothing
            }
        }

        public ResponseModel<long> GetNewAccessToken(string refreshToken, string accessToken)
        {
            ResponseModel<long> responseModel = new ResponseModel<long>();

            Authenticator authenticator = new Authenticator();
            Encryptor encryptor = new Encryptor();

            string hashedRefreshToken = string.Empty;

            string accessTokenUserID = authenticator.GetUserIDFromExpiredToken(accessToken);

            if (int.TryParse(accessTokenUserID, out int userID))
            {
                using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(userAccess)))
                {
                    var p = new DynamicParameters();
                    p.Add("@UserID", userID);

                    hashedRefreshToken = connection.Query<string>("dbo.spRefreshToken_Get", p, commandType:
                        CommandType.StoredProcedure).FirstOrDefault();
                }
            }

            if (!string.IsNullOrEmpty(hashedRefreshToken) && encryptor.IsHashValid(refreshToken, hashedRefreshToken))
            {                
                responseModel.OutputMessage = encryptor.CreateAccessToken(userID.ToString());
                responseModel.Model = GlobalConfig.GetAccessTokenExpDate();
                responseModel.IsSuccess = true;
            }
            else // refresh token invalid -> logout
            {
                responseModel.OutputMessage = "Session has expired";
                responseModel.ErrorAction = "[LogOut]";
            }

            return responseModel;
        }
    }
}