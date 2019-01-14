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
        private readonly string tokensFile = "Tokens.csv";
        private readonly string studentsFile = "Students.csv";
        private readonly string teachersFile = "Teachers.csv";
        private readonly string gradesFile = "Grades.csv";
        private readonly string ratingsFile = "Ratings.csv";
        private readonly string accountTypesFile = "AccountTypes.csv";
        private readonly string activitiesFile = "Activities.csv";
        private readonly string resetTicketsFile = "ResetTickets.csv";
        private readonly string csvDelimiter =  "--,--";

        public ResponseModel<UserModel> LoginUser(string username, string password)
        {
            ResponseModel<UserModel> responseModel = new ResponseModel<UserModel>();

            string checkFileResult = CheckFile(filesDirectory + usersFile, FileAccess.Read, "Users");

            if (string.IsNullOrEmpty(checkFileResult))
            {
                string[] user = GetRowByColumn(filesDirectory + usersFile, username, 1);
                if (user != null && !string.IsNullOrEmpty(user[2]))
                {
                    Encryptor encryption = new Encryptor();                                           
                    responseModel.IsSuccess = encryption.IsHashValid(password, user[2]);                    

                    if (responseModel.IsSuccess)
                    {
                        UserModel tempModel = new UserModel();

                        string token = encryption.CreateAccessToken(username);

                        string insertTokenResponse = InsertAccessToken(Convert.ToInt32(user[0]), token);

                        if (string.IsNullOrEmpty(insertTokenResponse))
                        {
                            responseModel.ErrorMessage = "Login successful";

                            tempModel.Username = username;
                            tempModel.AccessToken = token;

                            responseModel.Model = tempModel;
                        }
                        else
                        {
                            responseModel.ErrorMessage = insertTokenResponse;
                        }                        
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
            else
            {
                responseModel.ErrorMessage = checkFileResult;
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
                    responseModel.ErrorMessage = "Username taken";
                }               
            }
            else
            {
                responseModel.ErrorMessage = checkFileResult;
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
                responseModel.ErrorMessage = checkFileResult;
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
                responseModel.ErrorMessage = checkFileResult;
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
                    
                    if ( gradeLine[1].Equals(studentID.ToString()) && gradeLine[2].Equals(teacherID.ToString()))
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
                responseModel.ErrorMessage = checkFileResult;
            }

            return responseModel;
        }

        public ResponseModel<int> GetStudentRating(int studentID, int teacherID)
        {
            ResponseModel<int> responseModel = new ResponseModel<int>();

            string checkFileResult = CheckFile(filesDirectory + ratingsFile, FileAccess.Read, "Ratings");

            if (string.IsNullOrEmpty(checkFileResult))
            {
                string[] lines = File.ReadAllLines(filesDirectory + ratingsFile);

                foreach (string line in lines)
                {
                    string[] ratingLine = line.Split(new[] { csvDelimiter }, StringSplitOptions.None);

                    if (ratingLine[1].Equals(studentID.ToString()) && ratingLine[2].Equals(teacherID.ToString()))
                    {
                        responseModel.Model = Convert.ToInt32(ratingLine[3]);
                        responseModel.IsSuccess = true;
                        break;
                    }
                }
            }
            else
            {
                responseModel.ErrorMessage = checkFileResult;
            }

            return responseModel;
        }
               
        public ResponseModel<string> RateTeacher(string accessToken, int teacherID, int rate)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>() { Model = string.Empty };

            string filePath = filesDirectory + ratingsFile;

            string ratingsFileResult = CheckFile(filePath, FileAccess.Write, "Ratings");
            string teachersFileResult = CheckFile(filesDirectory + teachersFile, FileAccess.Write, "Teachers");
            string studentsFileResult = CheckFile(filesDirectory + studentsFile, FileAccess.Read, "Students");
            string tokensFileResult = CheckFile(filesDirectory + tokensFile, FileAccess.Read, "Tokens");

            if (string.IsNullOrEmpty(ratingsFileResult) && string.IsNullOrEmpty(teachersFileResult) 
                && string.IsNullOrEmpty(tokensFileResult) && string.IsNullOrEmpty(studentsFileResult))
            {
                string studentID = GetStudentID(accessToken);

                string[] lines = File.ReadAllLines(filePath);

                for (int i = 0; i < lines.Length; i++)
                {
                    string[] ratingsLine = lines[i].Split(new[] { csvDelimiter }, StringSplitOptions.None);

                    if (ratingsLine[1].Equals(studentID) && ratingsLine[2].Equals(teacherID.ToString()))
                    {
                        if (ratingsLine[3].Equals(rate.ToString()))
                        {
                            responseModel.IsSuccess = true;
                            responseModel.Model = "Teacher rated";
                        }
                        else if (rate > 0 && rate <= 7)
                        {
                            lines[i] = ratingsLine[0] + csvDelimiter +
                                studentID + csvDelimiter +
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
                            responseModel.ErrorMessage = "Rate invalid";
                        }

                        return responseModel;
                    }                    
                }

                string rating =
                        GetNewID(filePath) + csvDelimiter +
                        studentID + csvDelimiter +
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
                responseModel.ErrorMessage = 
                    ratingsFileResult + "\n" +
                    teachersFileResult + "\n" +
                    studentsFileResult + "\n" +
                    tokensFileResult;
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
                responseModel.ErrorMessage = checkFileResult;
            }

            return responseModel;
        }

        public ResponseModel<string> AddGrade(GradeModel gradeModel, string accessToken)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>();

            // CheckDiscrepancies();            

            string filePath = filesDirectory + gradesFile;
            string gradesFileResult = CheckFile(filePath, FileAccess.Write, "Grades");
            string teachersFileResult = CheckFile(filesDirectory + teachersFile, FileAccess.Read, "Teachers");
            string tokensFileResult = CheckFile(filesDirectory + tokensFile, FileAccess.Read, "Tokens");

            if (string.IsNullOrEmpty(gradesFileResult) && string.IsNullOrEmpty(teachersFileResult)
                && string.IsNullOrEmpty(tokensFileResult))
            {
                if (GetTeacherID(accessToken).Equals(gradeModel.TeacherID.ToString()))
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
                    responseModel.ErrorMessage = "Teacher mismatch";
                }
            }
            else
            {
                responseModel.ErrorMessage =
                    gradesFileResult + "\n" +
                    teachersFileResult + "\n" +
                    tokensFileResult;
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
                responseModel.ErrorMessage = gradesFileResult;
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
                    DateTime expDate = DateTime.UtcNow.AddMinutes(30);
                    //DateTime expDate = DateTime.UtcNow.AddSeconds(5);

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
                responseModel.ErrorMessage =
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
                string[] userLine = GetRowByColumn(filesDirectory + usersFile, username, 1);

                if (userLine != null)
                {
                    Encryptor encryptor = new Encryptor();
                    string hashedToken = string.Empty;

                    string[] lines = File.ReadAllLines(filePath);
                    
                    foreach (string line in lines)
                    {
                        string[] ticketLine = line.Split(new[] { csvDelimiter }, StringSplitOptions.None);

                        if (ticketLine[1].Equals(userLine[0]) && Convert.ToDateTime(ticketLine[3]) > DateTime.UtcNow)
                        {
                            hashedToken = ticketLine[2];                            
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(hashedToken) && encryptor.IsHashValid(token, hashedToken))
                    {
                        responseModel.Model = username;
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
                responseModel.ErrorMessage =
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
                    string[] userLine = GetRowByColumn(usersFilePath, message, 1);

                    if (userLine != null)
                    {
                        Encryptor encryptor = new Encryptor();

                        string[] lines = File.ReadAllLines(filePath);

                        foreach (string line in lines)
                        {
                            string[] ticketLine = line.Split(new[] { csvDelimiter }, StringSplitOptions.None);

                            if (ticketLine[1].Equals(userLine[0]) && Convert.ToDateTime(ticketLine[3]) > DateTime.UtcNow)
                            {
                                hashedPassword = userLine[2];
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

                                    if (user[0].Equals(userLine[0]))
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

                                    if (ticketLine[1].Equals(userLine[0]))
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
                        responseModel.ErrorAction = "[LinkExpired]";
                    }
                }
                else
                {
                    responseModel.ErrorMessage = message;
                }                
            }
            else
            {
                responseModel.ErrorMessage =
                    resetTicketsFileResult + "\n" +
                    usersFileResult;
            }

            return responseModel;
        }

        public bool CheckActivity(string username, string activity, ref string errorMessage)
        {
            string usersFileResult = CheckFile(filesDirectory + usersFile, FileAccess.Read, "Users");
            string activitiesFileResult = CheckFile(filesDirectory + activitiesFile, FileAccess.Read, "Activities");

            if (string.IsNullOrEmpty(usersFileResult) && string.IsNullOrEmpty(activitiesFileResult))
            {
                int accountTypeID = Convert.ToInt32(GetRowByColumn(filesDirectory + usersFile, username, 1)[3]);

                string[] lines = File.ReadAllLines(filesDirectory + activitiesFile);

                foreach (string line in lines)
                {
                    string[] activityLine = line.Split(new[] { csvDelimiter }, StringSplitOptions.None);

                    if (activityLine[1].Equals(activity) && activityLine[2].Equals(accountTypeID.ToString()))
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

        private string InsertAccessToken(int userID, string accessToken)
        {
            // CheckDiscrepancies();            

            string filePath = filesDirectory + tokensFile;
            string checkFileResult = CheckFile(filePath, FileAccess.Write, "Session");

            if (string.IsNullOrEmpty(checkFileResult))
            {
                string newToken =
                        userID.ToString() + csvDelimiter +
                        accessToken;

                if (GetRowByColumn(filesDirectory + tokensFile, userID.ToString(), 0) == null)
                {
                    using (StreamWriter w = File.AppendText(filePath))
                    {
                        w.WriteLine(newToken);
                    }
                }
                else
                {
                    string[] lines = File.ReadAllLines(filePath);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        string[] userLine = lines[i].Split(new[] { csvDelimiter }, StringSplitOptions.None);

                        if (userLine[0].Equals(userID.ToString()))
                        {
                            lines[i] = userLine[0] + csvDelimiter + accessToken;
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

        private string GetStudentID(string accessToken)
        {
            string userID = GetRowByColumn(filesDirectory + tokensFile, accessToken, 1)[0];

            return GetRowByColumn(filesDirectory + studentsFile, userID, 1)[0];
        }

        private string GetTeacherID(string accessToken)
        {
            string userID = GetRowByColumn(filesDirectory + tokensFile, accessToken, 1)[0];

            return GetRowByColumn(filesDirectory + teachersFile, userID, 1)[0];
        }
    }
}