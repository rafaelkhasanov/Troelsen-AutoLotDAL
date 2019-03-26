using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoLotDAL.DataOperations;
using AutoLotDAL.Models;
using System.Data;
using System.Data.SqlClient;

namespace AutoLotDAL.BulkImport
{
    public static class ProcessBulkImport
    {
        private static SqlConnection sqlConnection = null;
        private static readonly string _connectionString = @"Data Source = (localDb)\mssqllocaldb; Integrated Security = true; Initial Catalog = AutoLot";
        private static void OpenConnection()
        {
            sqlConnection = new SqlConnection { ConnectionString = _connectionString };
            sqlConnection.Open();
        }

        private static void CloseConnection()
        {
            if(sqlConnection?.State != ConnectionState.Closed)
            {
                sqlConnection?.Close();
            }
        }
    }
}
