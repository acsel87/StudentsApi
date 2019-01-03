using Dapper;
using Student.Helpers;
using Student.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Student.DataAccess
{
    public class SqlConnector : IDataConnection
    {
        private const string readAccess = "ReadAccess";
        private const string writeAccess = "WriteAccess";
        private const string userAccess = "UserAccess";

        public ResponseModel<List<GradeModel>> GetGrades(int studentID, int teacherID)
        {
            throw new NotImplementedException();
        }

        public ResponseModel<int> GetStudentRating(int studentID, int teacherID)
        {
            throw new NotImplementedException();
        }

        public ResponseModel<List<StudentModel>> GetStudents()
        {
            throw new NotImplementedException();
        }

        public ResponseModel<List<TeacherModel>> GetTeachers()
        {
            throw new NotImplementedException();
        }

        public string InsertAccessToken(int userID, string accessToken)
        {
            throw new NotImplementedException();
        }

        public ResponseModel<UserModel> LoginUser(string username, string password)
        {
            throw new NotImplementedException();
        }

        public ResponseModel<string> SignUpUser(string username, string password, string accountType)
        {
            ResponseModel<string> responseModel = new ResponseModel<string>() { Model = "" };

            try
            {
                if (!UsernameAlreadyExists(username))
                {
                    Encryption encryption = new Encryption();
                    string passwordHash = encryption.GenerateHash(password, null);

                    using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(writeAccess)))
                    {
                        var p = new DynamicParameters();
                        p.Add("@Username", username);
                        p.Add("@Password", passwordHash);
                        p.Add("@AccountType", accountType);

                        connection.Execute("dbo.spUser_Insert", p, commandType: CommandType.StoredProcedure);

                        responseModel.IsSuccess = true;
                        responseModel.Model = "User " + username + " was added successfully";
                    }
                }
                else
                {
                    responseModel.IsSuccess = false;
                    responseModel.ErrorMessage = "Username taken";
                }                
            }
            catch (Exception e)
            {
                responseModel.IsSuccess = false;
                responseModel.ErrorMessage = e.Message;
            }

            return responseModel;
        }
        
        private bool UsernameAlreadyExists(string username)
        {
            bool isExist = true;

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(readAccess)))
            {
                var p = new DynamicParameters();
                p.Add("@Username", username);

                isExist = connection.Query<bool>("dbo.spUser_CheckIfExist", p, commandType: CommandType.StoredProcedure).FirstOrDefault();
            }

            return isExist;
        }

        //public Dictionary<int, string> GetCustomer_All_Name()
        //{
        //    Dictionary<int, string> output = new Dictionary<int, string>();

        //    using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
        //    {
        //        // string strSql = "SELECT DISTINCT TableID AS [Key],TableName AS [Value] FROM dbo.TS_TStuctMaster";
        //        output = connection.Query<KeyValuePair<int, string>>("dbo.spCustomer_GetAll_Name").ToDictionary(pair => pair.Key, pair => pair.Value);
        //    }

        //    return output;
        //}

        //public CustomerModel GetCustomer_ById(int id)
        //{
        //    CustomerModel customer = new CustomerModel();

        //    using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
        //    {
        //        var p = new DynamicParameters();
        //        p.Add("@Id", id);
        //        customer = connection.Query<CustomerModel>("dbo.spCustomer_GetById", p, commandType: CommandType.StoredProcedure).FirstOrDefault();
        //    }

        //    return customer;
        //}

        //public Dictionary<int, string> GetProduct_All_Name()
        //{
        //    Dictionary<int, string> output = new Dictionary<int, string>();

        //    using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
        //    {
        //        output = connection.Query<KeyValuePair<int, string>>("dbo.spProduct_GetAll_Name").ToDictionary(pair => pair.Key, pair => pair.Value);
        //    }

        //    return output;
        //}

        //public ProductModel GetProduct_ById(int id)
        //{
        //    ProductModel product = new ProductModel();

        //    using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
        //    {
        //        var p = new DynamicParameters();
        //        p.Add("@Id", id);
        //        product = connection.Query<ProductModel>("dbo.spProduct_GetById", p, commandType: CommandType.StoredProcedure).FirstOrDefault();
        //    }

        //    return product;
        //}

        //public void InsertOrder(OrderModel model)
        //{
        //    using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
        //    {
        //        var p = new DynamicParameters();
        //        p.Add("@OrderDate", model.OrderDate);
        //        p.Add("@OrderNumber", model.OrderNumber);
        //        p.Add("@CustomerId", model.CustomerId);
        //        p.Add("@TotalAmount", model.TotalAmount);
        //        p.Add("@Id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

        //        connection.Execute("dbo.spOrder_Insert", p, commandType: CommandType.StoredProcedure);

        //        model.Id = p.Get<int>("@Id");
        //    }
        //}

        //public void InsertOrderItem(OrderItemModel model)
        //{
        //    using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
        //    {
        //        var p = new DynamicParameters();
        //        p.Add("@OrderId", model.OrderId);
        //        p.Add("@ProductId", model.ProductId);
        //        p.Add("@UnitPrice", model.UnitPrice);
        //        p.Add("@Quantity", model.Quantity);
        //        p.Add("@Id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

        //        connection.Execute("dbo.spOrderItem_Insert", p, commandType: CommandType.StoredProcedure);

        //        model.Id = p.Get<int>("@Id");
        //    }
        //}

        //public int GetOrder_OrderNumber_Last()
        //{
        //    using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
        //    {
        //        return connection.Query<int>("dbo.spOrder_GetOrderNumber_Last", commandType: CommandType.StoredProcedure).FirstOrDefault();
        //    }
        //}

        //public void RemoveCustomer_ById(int id)
        //{
        //    using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
        //    {
        //        var p = new DynamicParameters();
        //        p.Add("@Id", id);
        //        connection.Execute("dbo.spCustomer_Remove_ById", p, commandType: CommandType.StoredProcedure);
        //    }
        //}

        //public void UpdateProductPrice_ById(int id, decimal price)
        //{
        //    using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(db)))
        //    {
        //        var p = new DynamicParameters();
        //        p.Add("@Id", id);
        //        p.Add("@Price", price);
        //        connection.Execute("dbo.spProduct_UpdatePrice_ById", p, commandType: CommandType.StoredProcedure);
        //    }
        //}
    }
}