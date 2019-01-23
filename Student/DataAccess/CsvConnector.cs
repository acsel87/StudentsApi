using Student.Helpers;
using Student.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Student.DataAccess
{
    public class CsvConnector : IDataConnection
    {
        private readonly string filesDirectory = @"C:\Files\";
        private readonly string usersFile = "Users.csv";
        private readonly string refreshTokensFile = "RefreshTokens.csv";
        private readonly string studentsFile = "Students.csv";
        private readonly string teachersFile = "Teachers.csv";
        private readonly string gradesFile = "Grades.csv";
        private readonly string ratingsFile = "Ratings.csv";
        private readonly string accountTypesFile = "AccountTypes.csv";
        private readonly string activitiesFile = "Activities.csv";
        private readonly string resetTicketsFile = "ResetTickets.csv";
        private readonly string csvDelimiter = "--,--";

        public ResponseModel<UserModel> LoginUser(string username, string password)
        {
            ResponseModel<UserModel> responseModel = new ResponseModel<UserModel>();

            string checkFileResult = CheckFile(filesDirectory + usersFile, FileAccess.Read, "Users");

            if (string.IsNullOrEmpty(checkFileResult))
            {
                string[] user = GetRowByColumn(filesDirectory + usersFile, username, 1);
                if (user != null && !string.IsNullOrEmpty(user[2]))
                {
                    Encryptor encryptor = new Encryptor();
                    responseModel.IsSuccess = encryptor.IsHashValid(password, user[2]);

                    if (responseModel.IsSuccess)
                    {
                        UserModel tempModel = new UserModel();

                        string refreshToken = encryptor.GenerateRNG(32, 32);
                        string refreshTokenHash = encryptor.GenerateHash(refreshToken);

                        string insertTokenResponse = InsertRefreshToken(user[0], refreshTokenHash);

                        if (string.IsNullOrEmpty(insertTokenResponse))
                        {
                            tempModel.Username = username;
                            tempModel.RefreshToken = refreshToken;
                            tempModel.AccessToken = encryptor.CreateAccessToken(user[0]);
                            tempModel.AccessToken_ExpDate = GlobalConfig.GetAccessTokenExpDate();

                            responseModel.Model = tempModel;
                            responseModel.OutputMessage = "Login successful";
                        }
                        else
                        {
                            responseModel.OutputMessage = insertTokenResponse;
                        }
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
            else
            {
                responseModel.OutputMessage = checkFileResult;
            }

            return responseModel;
        }

        public ResponseModel<string> SignUpUser(string username, string password, int accountTypeID)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>() { Model = string.Empty };

            // CheckDiscrepancies();            

            string filePath = filesDirectory + usersFile;
            string checkFileResult = CheckFile(filePath, FileAccess.Write, "Users");

            if (string.IsNullOrEmpty(checkFileResult))
            {
                if (GetRowByColumn(filePath, username, 1) == null)
                {
                    Encryptor encryption = new Encryptor();
                    string passwordHash = encryption.GenerateHash(password);

                    string newUser =
                        GetNewID(filePath) + csvDelimiter +
                        username + csvDelimiter +
                        passwordHash + csvDelimiter +
                        accountTypeID.ToString();

                    using (StreamWriter w = File.AppendText(filePath))
                    {
                        w.WriteLine(newUser);
                    }

                    responseModel.IsSuccess = true;
                    responseModel.Model = "User " + username + " was added successfully";
                }
                else
                {
                    responseModel.OutputMessage = "Username taken";
                }
            }
            else
            {
                responseModel.OutputMessage = checkFileResult;
            }

            return responseModel;
        }

        public ResponseModel<List<StudentModel>> GetStudents()
        {
            ResponseModel<List<StudentModel>> responseModel = new ResponseModel<List<StudentModel>>();

            List<StudentModel> students = new List<StudentModel>();

            string checkFileResult = CheckFile(filesDirectory + studentsFile, FileAccess.Read, "Students");

            if (string.IsNullOrEmpty(checkFileResult))
            {
                string[] lines = File.ReadAllLines(filesDirectory + studentsFile);

                foreach (string line in lines)
                {
                    string[] studentLine = line.Split(new[] { csvDelimiter }, StringSplitOptions.None);

                    StudentModel student = new StudentModel();
                    student.StudentID = Convert.ToInt32(studentLine[0]);
                    student.UserID = Convert.ToInt32(studentLine[1]);
                    student.StudentName = studentLine[2];
                    student.Year = studentLine[3];

                    students.Add(student);
                }

                responseModel.Model = students;
                responseModel.IsSuccess = true;

            }
            else
            {
                responseModel.OutputMessage = checkFileResult;
            }

            return responseModel;

        }

        public ResponseModel<List<TeacherModel>> GetTeachers()
        {
            ResponseModel<List<TeacherModel>> responseModel = new ResponseModel<List<TeacherModel>>();

            List<TeacherModel> teachers = new List<TeacherModel>();

            string checkFileResult = CheckFile(filesDirectory + teachersFile, FileAccess.Read, "Teachers");

            if (string.IsNullOrEmpty(checkFileResult))
            {
                string[] lines = File.ReadAllLines(filesDirectory + teachersFile);

                foreach (string line in lines)
                {
                    string[] teacherLine = line.Split(new[] { csvDelimiter }, StringSplitOptions.None);

                    TeacherModel teacher = new TeacherModel();
                    teacher.TeacherID = Convert.ToInt32(teacherLine[0]);
                    teacher.UserID = Convert.ToInt32(teacherLine[1]);
                    teacher.TeacherName = teacherLine[2];
                    teacher.Class = teacherLine[3];
                    teacher.TeacherRating = Convert.ToInt32(teacherLine[4]);

                    teachers.Add(teacher);
                }

                responseModel.Model = teachers;
                responseModel.IsSuccess = true;

            }
            else
            {
                responseModel.OutputMessage = checkFileResult;
            }

            return responseModel;
        }

        public ResponseModel<List<GradeModel>> GetGrades(int studentID, int teacherID)
        {
            ResponseModel<List<GradeModel>> responseModel = new ResponseModel<List<GradeModel>>();

            List<GradeModel> grades = new List<GradeModel>();

            string checkFileResult = CheckFile(filesDirectory + gradesFile, FileAccess.Read, "Grades");

            if (string.IsNullOrEmpty(checkFileResult))
            {
                string[] lines = File.ReadAllLines(filesDirectory + gradesFile);

                foreach (string line in lines)
                {
                    string[] gradeLine = line.Split(new[] { csvDelimiter }, StringSplitOptions.None);

                    if (gradeLine[1].Equals(studentID.ToString()) && gradeLine[2].Equals(teacherID.ToString()))
                    {
                        GradeModel grade = new GradeModel();

                        grade.GradeID = Convert.ToInt32(gradeLine[0]);
                        grade.StudentID = studentID;
                        grade.TeacherID = teacherID;
                        grade.Grade = gradeLine[3];
                        grade.GradeDate = Convert.ToDateTime(gradeLine[4]);
                        grade.GradeNotes = gradeLine[5];

                        grades.Add(grade);
                    }
                }

                responseModel.Model = grades;
                responseModel.IsSuccess = true;

            }
            else
            {
                responseModel.OutputMessage = checkFileResult;
            }

            return responseModel;
        }

        public ResponseModel<int> GetStudentRating(string userID, int teacherID)
        {
            ResponseModel<int> responseModel = new ResponseModel<int>();

            string ratingsFileResult = CheckFile(filesDirectory + ratingsFile, FileAccess.Read, "Ratings");
            string studentsFileResult = CheckFile(filesDirectory + studentsFile, FileAccess.Read, "Students");

            if (string.IsNullOrEmpty(ratingsFileResult) && string.IsNullOrEmpty(ratingsFileResult))
            {
                string[] studentRow = GetRowByColumn(filesDirectory + studentsFile, userID, 1);

                string[] lines = File.ReadAllLines(filesDirectory + ratingsFile);

                responseModel.Model = 0;
                responseModel.IsSuccess = true;

                foreach (string line in lines)
                {
                    string[] ratingLine = line.Split(new[] { csvDelimiter }, StringSplitOptions.None);

                    if (studentRow != null && ratingLine[1].Equals(studentRow[0])
                        && ratingLine[2].Equals(teacherID.ToString()))
                    {
                        responseModel.Model = Convert.ToInt32(ratingLine[3]);
                        break;
                    }
                }
            }
            else
            {
                responseModel.OutputMessage =
                    ratingsFileResult + "\n" +
                    studentsFileResult;
            }

            return responseModel;
        }

        public ResponseModel<string> RateTeacher(string userID, int teacherID, int rate)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>() { Model = string.Empty };

            string filePath = filesDirectory + ratingsFile;

            string ratingsFileResult = CheckFile(filePath, FileAccess.Write, "Ratings");
            string teachersFileResult = CheckFile(filesDirectory + teachersFile, FileAccess.Write, "Teachers");
            string studentsFileResult = CheckFile(filesDirectory + studentsFile, FileAccess.Read, "Students");

            if (string.IsNullOrEmpty(ratingsFileResult) && string.IsNullOrEmpty(teachersFileResult)
               && string.IsNullOrEmpty(studentsFileResult))
            {
                string[] studentRow = GetRowByColumn(filesDirectory + studentsFile, userID, 1);

                string[] lines = File.ReadAllLines(filePath);

                for (int i = 0; i < lines.Length; i++)
                {
                    string[] ratingsLine = lines[i].Split(new[] { csvDelimiter }, StringSplitOptions.None);

                    if (studentRow != null && ratingsLine[1].Equals(studentRow[0])
                        && ratingsLine[2].Equals(teacherID.ToString()))
                    {
                        if (ratingsLine[3].Equals(rate.ToString()))
                        {
                            responseModel.IsSuccess = true;
                            responseModel.Model = "Teacher rated";
                        }
                        else if (rate > 0 && rate <= 7)
                        {
                            lines[i] = ratingsLine[0] + csvDelimiter +
                                studentRow[0] + csvDelimiter +
                                teacherID.ToString() + csvDelimiter +
                                rate;

                            File.WriteAllLines(filePath, lines);

                            UpdateTeacherRating(teacherID);

                            responseModel.IsSuccess = true;
                            responseModel.Model = "Teacher rated";
                        }
                        else if (rate == 0)
                        {
                            lines = lines.RemoveAtIndex(i);
                            File.WriteAllLines(filePath, lines);
                            UpdateTeacherRating(teacherID);

                            responseModel.IsSuccess = true;
                            responseModel.Model = "Teacher unrated";
                        }
                        else
                        {
                            responseModel.OutputMessage = "Rate invalid";
                        }

                        return responseModel;
                    }
                }

                if (rate == 0)
                {
                    responseModel.IsSuccess = true;
                    responseModel.Model = "Teacher unrated";
                }
                else if (rate > 0 && rate <= 7)
                {
                    string rating =
                        GetNewID(filePath) + csvDelimiter +
                        studentRow[0] + csvDelimiter +
                        teacherID.ToString() + csvDelimiter +
                        rate;

                    using (StreamWriter w = File.AppendText(filePath))
                    {
                        w.WriteLine(rating);
                    }

                    UpdateTeacherRating(teacherID);

                    responseModel.IsSuccess = true;
                    responseModel.Model = "Teacher rated";
                }
                else
                {
                    responseModel.OutputMessage = "Rate invalid";
                }                
            }
            else
            {
                responseModel.OutputMessage =
                    ratingsFileResult + "\n" +
                    teachersFileResult + "\n" +
                    studentsFileResult;
            }

            return responseModel;
        }

        public ResponseModel<List<KeyValuePair<int, string>>> GetAccountTypes()
        {
            ResponseModel<List<KeyValuePair<int, string>>> responseModel = new ResponseModel<List<KeyValuePair<int, string>>>();

            List<KeyValuePair<int, string>> accountTypes = new List<KeyValuePair<int, string>>();

            string checkFileResult = CheckFile(filesDirectory + accountTypesFile, FileAccess.Read, "Accounts");

            if (string.IsNullOrEmpty(checkFileResult))
            {
                string[] lines = File.ReadAllLines(filesDirectory + accountTypesFile);

                foreach (string line in lines)
                {
                    string[] accountTypeLine = line.Split(new[] { csvDelimiter }, StringSplitOptions.None);

                    KeyValuePair<int, string> tempPair = new KeyValuePair<int, string>(Convert.ToInt32(accountTypeLine[0]), accountTypeLine[1]);

                    accountTypes.Add(tempPair);
                }

                responseModel.Model = accountTypes;
                responseModel.IsSuccess = true;

            }
            else
            {
                responseModel.OutputMessage = checkFileResult;
            }

            return responseModel;
        }

        public ResponseModel<string> AddGrade(GradeModel gradeModel, string userID)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>();

            // CheckDiscrepancies();            

            string filePath = filesDirectory + gradesFile;
            string gradesFileResult = CheckFile(filePath, FileAccess.Write, "Grades");
            string teachersFileResult = CheckFile(filesDirectory + teachersFile, FileAccess.Read, "Teachers");

            if (string.IsNullOrEmpty(gradesFileResult) && string.IsNullOrEmpty(teachersFileResult))
            {
                string[] teacherRow = GetRowByColumn(filesDirectory + teachersFile, userID, 1);

                if (teacherRow != null && teacherRow[0].Equals(gradeModel.TeacherID.ToString()))
                {
                    string newGrade =
                                        GetNewID(filePath) + csvDelimiter +
                                        gradeModel.StudentID.ToString() + csvDelimiter +
                                        gradeModel.TeacherID.ToString() + csvDelimiter +
                                        gradeModel.Grade + csvDelimiter +
                                        DateTime.UtcNow.ToString() + csvDelimiter +
                                        gradeModel.GradeNotes;

                    using (StreamWriter w = File.AppendText(filePath))
                    {
                        w.WriteLine(newGrade);
                    }

                    responseModel.IsSuccess = true;
                }
                else
                {
                    responseModel.OutputMessage = "Teacher mismatch";
                }
            }
            else
            {
                responseModel.OutputMessage =
                    gradesFileResult + "\n" +
                    teachersFileResult;
            }

            return responseModel;
        }

        public ResponseModel<string> EditGrade(GradeModel gradeModel)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>();

            // CheckDiscrepancies();            

            string filePath = filesDirectory + gradesFile;
            string gradesFileResult = CheckFile(filePath, FileAccess.Write, "Grades");

            if (string.IsNullOrEmpty(gradesFileResult))
            {
                string[] lines = File.ReadAllLines(filePath);

                for (int i = 0; i < lines.Length; i++)
                {
                    string[] gradeLine = lines[i].Split(new[] { csvDelimiter }, StringSplitOptions.None);

                    if (gradeLine[0].Equals(gradeModel.GradeID.ToString()))
                    {
                        lines[i] = gradeLine[0] + csvDelimiter +
                                    gradeLine[1] + csvDelimiter +
                                    gradeLine[2] + csvDelimiter +
                                    gradeModel.Grade + csvDelimiter +
                                    DateTime.UtcNow.ToString() + csvDelimiter +
                                    gradeModel.GradeNotes;
                        break;
                    }
                }

                File.WriteAllLines(filePath, lines);

                responseModel.IsSuccess = true;
            }
            else
            {
                responseModel.OutputMessage = gradesFileResult;
            }

            return responseModel;
        }

        public ResponseModel<string[]> ResetPassword_SendInstructions(string username)
        {
            ResponseModel<string[]> responseModel = new ResponseModel<string[]>() { Model = new string[2] };

            string filePath = filesDirectory + resetTicketsFile;
            string resetTicketsFileResult = CheckFile(filePath, FileAccess.Write, "Reset");
            string usersFileResult = CheckFile(filesDirectory + usersFile, FileAccess.Read, "Users");

            if (string.IsNullOrEmpty(resetTicketsFileResult) && string.IsNullOrEmpty(usersFileResult))
            {
                string[] userLine = GetRowByColumn(filesDirectory + usersFile, username, 1);

                if (userLine != null)
                {
                    Encryptor encryptor = new Encryptor();

                    string token = encryptor.GenerateRNG(8, 16);
                    string hashToken = encryptor.GenerateHash(token);
                    DateTime expDate = GlobalConfig.GetResetTokenExpDate();

                    if (GetRowByColumn(filePath, userLine[0], 1) != null)
                    {
                        string[] lines = File.ReadAllLines(filePath);

                        for (int i = 0; i < lines.Length; i++)
                        {
                            string[] ticketLine = lines[i].Split(new[] { csvDelimiter }, StringSplitOptions.None);

                            if (ticketLine[1].Equals(userLine[0]))
                            {
                                lines[i] = ticketLine[0] + csvDelimiter +
                                            ticketLine[1] + csvDelimiter +
                                            hashToken + csvDelimiter +
                                            expDate.ToString();
                                break;
                            }
                        }

                        File.WriteAllLines(filePath, lines);
                    }
                    else
                    {

                        string ticketLine =
                                GetNewID(filePath) + csvDelimiter +
                                userLine[0] + csvDelimiter +
                                hashToken + csvDelimiter +
                                expDate.ToString();

                        using (StreamWriter w = File.AppendText(filePath))
                        {
                            w.WriteLine(ticketLine);
                        }
                    }

                    responseModel.Model[0] = username;
                    responseModel.Model[1] = token;
                }

                responseModel.IsSuccess = true;
            }
            else
            {
                responseModel.OutputMessage =
                    resetTicketsFileResult + "\n" +
                    usersFileResult;
            }

            return responseModel;
        }

        public ResponseModel<string> ActivateTokenLink(string username, string token)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>();

            string filePath = filesDirectory + resetTicketsFile;
            string resetTicketsFileResult = CheckFile(filePath, FileAccess.Write, "Reset");
            string usersFileResult = CheckFile(filesDirectory + usersFile, FileAccess.Read, "Users");

            if (string.IsNullOrEmpty(resetTicketsFileResult) && string.IsNullOrEmpty(usersFileResult))
            {
                string[] userRow = GetRowByColumn(filesDirectory + usersFile, username, 1);

                if (userRow != null)
                {
                    Encryptor encryptor = new Encryptor();
                    string hashedToken = string.Empty;

                    string[] lines = File.ReadAllLines(filePath);

                    foreach (string line in lines)
                    {
                        string[] ticketLine = line.Split(new[] { csvDelimiter }, StringSplitOptions.None);

                        if (ticketLine[1].Equals(userRow[0]) && Convert.ToDateTime(ticketLine[3]) > DateTime.UtcNow)
                        {
                            hashedToken = ticketLine[2];
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(hashedToken) && encryptor.IsHashValid(token, hashedToken))
                    {
                        responseModel.Model = userRow[0];
                    }
                    else
                    {
                        responseModel.ErrorAction = "[LinkExpired]";
                    }
                }
                else
                {
                    responseModel.ErrorAction = "[LinkExpired]";
                }

                responseModel.IsSuccess = true;
            }
            else
            {
                responseModel.OutputMessage =
                    resetTicketsFileResult + "\n" +
                    usersFileResult;
            }

            return responseModel;
        }

        public ResponseModel<string> ConfirmPasswordReset(string userToken, string newPassword)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>();

            string filePath = filesDirectory + resetTicketsFile;
            string usersFilePath = filesDirectory + usersFile;
            string resetTicketsFileResult = CheckFile(filePath, FileAccess.Write, "Reset");
            string usersFileResult = CheckFile(usersFilePath, FileAccess.Write, "Users");

            if (string.IsNullOrEmpty(resetTicketsFileResult) && string.IsNullOrEmpty(usersFileResult))
            {
                Authenticator authenticator = new Authenticator();

                string message = string.Empty;
                string hashedPassword = string.Empty;

                if (authenticator.VerifyToken(userToken, ref message))
                {
                    string[] userRow = GetRowByColumn(usersFilePath, message, 0);

                    if (userRow != null)
                    {
                        Encryptor encryptor = new Encryptor();

                        string[] lines = File.ReadAllLines(filePath);

                        foreach (string line in lines)
                        {
                            string[] ticketLine = line.Split(new[] { csvDelimiter }, StringSplitOptions.None);

                            if (ticketLine[1].Equals(userRow[0]) && Convert.ToDateTime(ticketLine[3]) > DateTime.UtcNow)
                            {
                                hashedPassword = userRow[2];
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(hashedPassword))
                        {
                            if (!encryptor.IsHashValid(newPassword, hashedPassword))
                            {
                                // change password 
                                string newPasswordHash = encryptor.GenerateHash(newPassword);

                                string[] userLines = File.ReadAllLines(usersFilePath);

                                for (int i = 0; i < userLines.Length; i++)
                                {
                                    string[] user = userLines[i].Split(new[] { csvDelimiter }, StringSplitOptions.None);

                                    if (user[0].Equals(userRow[0]))
                                    {
                                        userLines[i] =
                                                user[0] + csvDelimiter +
                                                user[1] + csvDelimiter +
                                                newPasswordHash + csvDelimiter +
                                                user[3];
                                        break;
                                    }
                                }

                                File.WriteAllLines(usersFilePath, userLines);

                                // delete ticket
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    string[] ticketLine = lines[i].Split(new[] { csvDelimiter }, StringSplitOptions.None);

                                    if (ticketLine[1].Equals(userRow[0]))
                                    {
                                        lines = lines.RemoveAtIndex(i);
                                        break;
                                    }
                                }

                                File.WriteAllLines(filePath, lines);

                                responseModel.IsSuccess = true;
                                responseModel.Model = "Password changed successfully!";
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
                        responseModel.ErrorAction = "[LinkExpired]";
                    }
                }
                else
                {
                    responseModel.OutputMessage = message;
                }
            }
            else
            {
                responseModel.OutputMessage =
                    resetTicketsFileResult + "\n" +
                    usersFileResult;
            }

            return responseModel;
        }

        public bool CheckActivity(string userID, string activity, ref string errorMessage)
        {
            string usersFileResult = CheckFile(filesDirectory + usersFile, FileAccess.Read, "Users");
            string activitiesFileResult = CheckFile(filesDirectory + activitiesFile, FileAccess.Read, "Activities");

            string[] userRow = GetRowByColumn(filesDirectory + usersFile, userID, 0);

            if (string.IsNullOrEmpty(usersFileResult) && string.IsNullOrEmpty(activitiesFileResult)
                && userRow != null)
            {
                string[] lines = File.ReadAllLines(filesDirectory + activitiesFile);

                foreach (string line in lines)
                {
                    string[] activityLine = line.Split(new[] { csvDelimiter }, StringSplitOptions.None);

                    if (activityLine[1].Equals(activity) && activityLine[2].Equals(userRow[3]))
                    {
                        return true;
                    }
                }

                errorMessage = "You're not allowed, bad kitty !";
            }
            else
            {
                errorMessage = usersFileResult + "\n" + activitiesFileResult;
            }

            return false;
        }

        private string InsertRefreshToken(string userID, string refreshTokenHash)
        {
            // CheckDiscrepancies();            

            string filePath = filesDirectory + refreshTokensFile;
            string checkFileResult = CheckFile(filePath, FileAccess.Write, "Session");

            if (string.IsNullOrEmpty(checkFileResult))
            {
                string newRefreshToken =
                        GetNewID(filePath) + csvDelimiter +
                        userID.ToString() + csvDelimiter +
                        refreshTokenHash + csvDelimiter +
                        GlobalConfig.GetRefreshTokenExpDate();

                if (GetRowByColumn(filePath, userID, 1) == null)
                {
                    using (StreamWriter w = File.AppendText(filePath))
                    {
                        w.WriteLine(newRefreshToken);
                    }
                }
                else
                {
                    string[] lines = File.ReadAllLines(filePath);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        string[] tokenLine = lines[i].Split(new[] { csvDelimiter }, StringSplitOptions.None);

                        if (tokenLine[1].Equals(userID))
                        {
                            lines[i] = newRefreshToken;
                            break;
                        }
                    }

                    File.WriteAllLines(filePath, lines);
                }

                return null;
            }
            else
            {
                return checkFileResult;
            }
        }

        private string[] GetRowByColumn(string filePath, string columnValue, int columnNumber)
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] userLine = line.Split(new[] { csvDelimiter }, StringSplitOptions.None);

                if (userLine[columnNumber].Equals(columnValue))
                {
                    return userLine;
                }
            }
            return null;
        }

        private string CheckFile(string filePath, FileAccess fileAccess, string fileTag)
        {
            string error = string.Empty;

            try
            {
                File.Open(filePath, FileMode.Open, fileAccess).Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                try
                {
                    Directory.CreateDirectory(filePath);
                    File.Create(filePath).Dispose();
                }
                catch (UnauthorizedAccessException)
                {
                    error = fileTag + " access denied";
                }
            }
            catch (FileNotFoundException)
            {
                try
                {
                    File.Create(filePath).Dispose();
                }
                catch (UnauthorizedAccessException)
                {
                    error = fileTag + " access denied";
                }
            }
            catch (UnauthorizedAccessException)
            {
                error = fileTag + " access denied";
            }
            catch (IOException)
            {
                error = fileTag + " file is in use by another program";
            }

            return error;
        }

        private string CheckDiscrepancies(params string[] filepaths)
        {
            // todo - check if all files have the same userIDs
            return "ignore";
        }

        private void UpdateTeacherRating(int teacherID)
        {
            string[] lines = File.ReadAllLines(filesDirectory + ratingsFile);

            int totalRating = 0;
            int ratingsCounter = 0;

            foreach (string line in lines)
            {
                string[] ratingsLine = line.Split(new[] { csvDelimiter }, StringSplitOptions.None);

                if (ratingsLine[2].Equals(teacherID.ToString()))
                {
                    totalRating += Convert.ToInt32(ratingsLine[3]);
                    ratingsCounter++;
                }
            }

            int newRating;

            try
            {
                newRating = totalRating / ratingsCounter;
            }
            catch (DivideByZeroException)
            {
                newRating = 0;
            }

            lines = File.ReadAllLines(filesDirectory + teachersFile);

            for (int i = 0; i < lines.Length; i++)
            {
                string[] teachersLine = lines[i].Split(new[] { csvDelimiter }, StringSplitOptions.None);

                if (teachersLine[0].Equals(teacherID.ToString()))
                {
                    lines[i] = teachersLine[0] + csvDelimiter +
                            teachersLine[1] + csvDelimiter +
                            teachersLine[2] + csvDelimiter +
                            teachersLine[3] + csvDelimiter +
                            newRating.ToString();

                    File.WriteAllLines(filesDirectory + teachersFile, lines);
                    break;
                }
            }
        }

        private string GetNewID(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length > 0)
            {
                int[] arrayOfID = new int[lines.Length];

                for (int i = 0; i < arrayOfID.Length; i++)
                {
                    string[] line = lines[i].Split(new[] { csvDelimiter }, StringSplitOptions.None);

                    if (int.TryParse(line[0], out int tempId))
                    {
                        arrayOfID[i] = tempId;
                    }
                    else
                    {
                        arrayOfID[i] = -1;
                    }
                }

                Array.Sort(arrayOfID);

                return (arrayOfID[arrayOfID.Length - 1] + 1).ToString();
            }

            return "1";
        }

        [Obsolete("Use GetRowByColumn instead")]
        private string GetRowIDByUserID(string accessToken, string filePath)
        {
            string[] userRow = GetRowByColumn(filesDirectory + refreshTokensFile, accessToken, 1);

            if (userRow.Length > 0 && !string.IsNullOrEmpty(userRow[0]))
            {
                string[] row = GetRowByColumn(filePath, userRow[0], 1);

                if (row != null)
                {
                    return row[0];
                }
            }

            return null;
        }

        public void SignOut(string accessToken)
        {
            string filePath = filesDirectory + refreshTokensFile;

            try
            {
                Authenticator authenticator = new Authenticator();

                string userID = string.Empty;

                authenticator.VerifyToken(accessToken, ref userID);

                string[] lines = File.ReadAllLines(filePath);

                for (int i = 0; i < lines.Length; i++)
                {
                    string[] tokenLine = lines[i].Split(new[] { csvDelimiter }, StringSplitOptions.None);

                    if (tokenLine[1].Equals(userID))
                    {
                        lines = lines.RemoveAtIndex(i);
                        break;
                    }
                }

                File.WriteAllLines(filePath, lines);

            }
            catch (Exception)
            {
                // do nothing
            }                      
        }

        public ResponseModel<long> GetNewAccessToken(string refreshToken, string accessToken)
        {
            ResponseModel<long> responseModel = new ResponseModel<long>();

            string filePath = filesDirectory + refreshTokensFile;
            string checkFileResult = CheckFile(filePath, FileAccess.Write, "Session");

            if (string.IsNullOrEmpty(checkFileResult))
            {                
                Authenticator authenticator = new Authenticator();
                Encryptor encryptor = new Encryptor();

                string hashedRefreshToken = string.Empty;

                string accessTokenUserID = authenticator.GetUserIDFromExpiredToken(accessToken);

                if (int.TryParse(accessTokenUserID, out int userID))
                {
                    string[] lines = File.ReadAllLines(filePath);

                    foreach (string line in lines)
                    {
                        string[] tokenLine = line.Split(new[] { csvDelimiter }, StringSplitOptions.None);

                        if (tokenLine[1].Equals(accessTokenUserID) && Convert.ToDateTime(tokenLine[3]) > DateTime.UtcNow)
                        {
                            hashedRefreshToken = tokenLine[2];
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(hashedRefreshToken) && encryptor.IsHashValid(refreshToken, hashedRefreshToken))
                {
                    responseModel.OutputMessage = encryptor.CreateAccessToken(userID.ToString());
                    responseModel.Model = GlobalConfig.GetAccessTokenExpDate();
                    responseModel.IsSuccess = true;
                }
                else
                {
                    responseModel.OutputMessage = "Session has expired";
                    responseModel.ErrorAction = "[LogOut]";
                }
            }
            else
            {
                responseModel.OutputMessage = checkFileResult;
            }


            return responseModel;
        }
    }
}