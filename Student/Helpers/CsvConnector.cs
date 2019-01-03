using Student.DataAccess;
using Student.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Student.Helpers
{
    public class CsvConnector : IDataConnection
    {
        private readonly static string filesDirectory = @"C:\Files\";        
        private readonly static string usersFile = "Users.csv";
        private readonly static string tokensFile = "Tokens.csv";
        private readonly static string studentsFile = "Students.csv";
        private readonly static string teachersFile = "Teachers.csv";
        private readonly static string gradesFile = "Grades.csv";
        private readonly static string ratingsFile = "Ratings.csv";
        private readonly static string csvDelimiter =  "--,--";

        public ResponseModel<UserModel> LoginUser(string username, string password)
        {
            ResponseModel<UserModel> responseModel = new ResponseModel<UserModel>();

            string checkFileResult = CheckFile(filesDirectory + usersFile, FileAccess.Read, "Users");

            if (string.IsNullOrEmpty(checkFileResult))
            {
                responseModel.ErrorMessage = "Invalid user or password";

                string[] user = RowAlreadyExists(filesDirectory + usersFile, username, 1);
                if (user != null)
                {
                    Encryption encryption = new Encryption();

                    if (!string.IsNullOrEmpty(user[2]))
                    {                        
                        responseModel.IsSuccess = encryption.IsHashValid(password, user[2]);
                    }

                    if (responseModel.IsSuccess)
                    {
                        UserModel tempModel = new UserModel();

                        string token = encryption.CreateAccessToken(username);

                        string insertTokenResponse = InsertAccessToken(Convert.ToInt32(user[0]), token);

                        if (string.IsNullOrEmpty(insertTokenResponse))
                        {
                            responseModel.ErrorMessage = "Login successful";

                            tempModel.Username = user[1];
                            tempModel.AccessToken = token;

                            responseModel.Model = tempModel;
                        }
                        else
                        {
                            responseModel.IsSuccess = false;
                            responseModel.ErrorMessage = insertTokenResponse;
                        }                        
                    }
                }                
            }
            else
            {
                responseModel.ErrorMessage = checkFileResult;
            }

            return responseModel;
        }

        public ResponseModel<string> SignUpUser(string username, string password, string accountType)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>() { Model = string.Empty };

            // CheckDiscrepancies();            

            string checkFileResult = CheckFile(filesDirectory + usersFile, FileAccess.Write, "Users");

            if (string.IsNullOrEmpty(checkFileResult))
            {
                if (RowAlreadyExists(filesDirectory + usersFile, username, 1) == null)
                {
                    Encryption encryption = new Encryption();
                    string passwordHash = encryption.GenerateHash(password, null);

                    // todo - check and insert id
                    string newUser =
                        "1" + csvDelimiter +
                        username + csvDelimiter +
                        passwordHash + csvDelimiter +
                        accountType;

                    using (StreamWriter w = File.AppendText(filesDirectory + usersFile))
                    {
                        w.WriteLine(newUser);
                    }

                    responseModel.IsSuccess = true;
                    responseModel.Model = "User " + username + " was added successfully";
                }
                else
                {
                    responseModel.IsSuccess = false;
                    responseModel.ErrorMessage = "Username taken";
                }               
            }
            else
            {
                responseModel.IsSuccess = false;
                responseModel.ErrorMessage = checkFileResult;
            }

            return responseModel;
        }

        public string InsertAccessToken(int userID, string accessToken)
        {
            // CheckDiscrepancies();            

            string filePath = filesDirectory + tokensFile;
            string checkFileResult = CheckFile(filePath, FileAccess.Write, "Session");

            if (string.IsNullOrEmpty(checkFileResult))
            {
                string newToken =
                        userID.ToString() + csvDelimiter +
                        accessToken;

                if (RowAlreadyExists(filesDirectory + tokensFile, userID.ToString(), 0) == null)
                {  
                    using (StreamWriter w = File.AppendText(filePath))
                    {
                        w.WriteLine(newToken);
                    }

                    return null;
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

                    return null;
                }
            }
            else
            {
                return checkFileResult;
            }
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
            ResponseModel<int> responseModel = new ResponseModel<int>() {};

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

        private string[] RowAlreadyExists(string filePath, string columnValue, int columntNumber)
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] userLine = line.Split(new[] { csvDelimiter }, StringSplitOptions.None);

                if (userLine[columntNumber].Equals(columnValue))
                {
                    return userLine;
                }
            }

            return null;
        }

        private string CheckFile(string filePath, FileAccess fileAccess, string fileTag)
        {
            string error = "";

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
    }
}