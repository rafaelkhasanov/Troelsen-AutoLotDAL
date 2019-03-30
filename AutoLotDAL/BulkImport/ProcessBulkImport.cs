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

        public static void ExecuteBulkImport<T>(IEnumerable<T> records, string tableName)
        {
            OpenConnection();
            using (SqlConnection conn = sqlConnection)
            {
                SqlBulkCopy bc = new SqlBulkCopy(conn) { DestinationTableName = tableName };
                var dataReader = new MyDataReader<T>(records.ToList());
                try
                {
                    bc.WriteToServer(dataReader);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //Здесь должно что-то делается
                }
                finally
                {
                    CloseConnection();
                }
            }
        }

        public static void DoBulkCopy()
        {
            Console.WriteLine("Do Bulk Copy");
            var cars = new List<Car>
            {
                new Car() {Color = "Blue", Make = "Honda", PetName = "MyCar1"},
                new Car() {Color = "Red", Make = "Volvo", PetName = "MyCar2" },
                new Car() {Color = "White", Make = "VW", PetName = "MyCar3"},
                new Car() {Color = "Yellow", Make = "Toyota", PetName = "MyCar4"}
            };

            ProcessBulkImport.ExecuteBulkImport(cars, "Inventory");
            InventoryDAL dal = new InventoryDAL();
            var list = dal.GetAllInventory();
            Console.WriteLine("All Cars");
            Console.WriteLine("CarId\tMake\tColor\tPet Name");
            foreach (var item in list)
            {
                Console.WriteLine($"{item.CarId}\t{item.Make}\t{item.Color}\t{item.PetName}");
            }
            Console.WriteLine();
        }
    }
}
