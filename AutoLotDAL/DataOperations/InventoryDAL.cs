using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using AutoLotDAL.Models;

namespace AutoLotDAL.DataOperations
{
    public class InventoryDAL
    {
        private readonly string _connectionString;
        private SqlConnection _sqlConnection = null;
        public InventoryDAL() : this(@"Data Source = (localDb)\mssqllocaldb; Integrated Security = true; Initial Catalog = AutoLot")
        {

        }

        public InventoryDAL(string connectionString) => _connectionString = connectionString;
        private void OpenConnection()
        {
            _sqlConnection = new SqlConnection { ConnectionString = _connectionString };
            _sqlConnection.Open();
        }

        private void CloseConnection()
        {
            if (_sqlConnection?.State != ConnectionState.Closed)
            {
                _sqlConnection?.Close();
            }
        }

        public List<Car> GetAllInventory()
        {
            OpenConnection();
            //Здесь будут храниться записи
            List<Car> inventory = new List<Car>();
            //Подготовить объекты команды
            string sql = "Select * From Inventory";
            using (SqlCommand sqlCommand = new SqlCommand(sql, _sqlConnection))
            {
                sqlCommand.CommandType = CommandType.Text;
                SqlDataReader dataReader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
                while (dataReader.Read())
                {
                    inventory.Add(new Car
                    {
                        CarId = (int)dataReader["CarId"],
                        Color = (string)dataReader["Color"],
                        Make = (string)dataReader["Make"],
                        PetName = (string)dataReader["PetName"]
                    });
                }
                dataReader.Close();
            }
            return inventory;
        }

        public Car GetCar(int id)
        {
            OpenConnection();
            Car car = null;
            string sql = $"Select * From Inventory where CarId = '{id}'";
            using (SqlCommand sqlCommand = new SqlCommand(sql, _sqlConnection))
            {
                sqlCommand.CommandType = CommandType.Text;
                SqlDataReader dataReader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
                while (dataReader.Read())
                {
                    car = new Car
                    {
                        CarId = (int)dataReader["CarId"],
                        Color = (string)dataReader["Color"],
                        Make = (string)dataReader["Make"],
                        PetName = (string)dataReader["PetName"]
                    };
                }
                dataReader.Close();
            }
            return car;
        }

        public void InsertAuto(string color, string petName, string Make)
        {
            OpenConnection();
            string sqlInsert = $"Insert into Inventory (Make, Color, PetName) Values('{Make}', '{color}', '{petName}')";
            //выполнить используя наше подключение
            using (SqlCommand sqlCommand = new SqlCommand(sqlInsert, _sqlConnection))
            {
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.ExecuteNonQuery();
            }
            CloseConnection();
        }

        public void InsertAuto(Car car)
        {
            OpenConnection();
            string sqlInsert = "Insert into Inventory"
                + "(Make, Color, PetName) Values" +
                "(@Make, @Color, @PetName)";
            //Эта команда будет иметь внутренние параметры
            using (SqlCommand sqlCommand = new SqlCommand(sqlInsert, _sqlConnection))
            {
                //заполнить коллекцию параметров
                SqlParameter sqlParametr = new SqlParameter()
                {
                    ParameterName = "@Make",
                    Value = car.Make,
                    SqlDbType = SqlDbType.Char,
                    Size = 10
                };
                sqlCommand.Parameters.Add(sqlParametr);

                sqlParametr = new SqlParameter()
                {
                    ParameterName = @"Color",
                    Value = car.Color,
                    SqlDbType = SqlDbType.Char,
                    Size = 20
                };

                sqlCommand.Parameters.Add(sqlParametr);
                sqlParametr = new SqlParameter()
                {
                    ParameterName = @"PetName",
                    Value = car.PetName,
                    SqlDbType = SqlDbType.Char,
                    Size = 10
                };

                sqlCommand.Parameters.Add(sqlParametr);
                sqlCommand.ExecuteNonQuery();
            }
            CloseConnection();
        }

        public void DeleteCar(int id)
        {
            OpenConnection();
            string sqlDelete = $"Delete from Inventory Where CarId = '{id}'";
            using (SqlCommand sql = new SqlCommand(sqlDelete, _sqlConnection))
            {
                try
                {
                    sql.CommandType = CommandType.Text;
                    sql.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Exception error = new Exception("Sorry! This car is on order!", ex);
                    throw error;
                }
            }
            CloseConnection();
        }

        public void UpdateCarPetName(int id, string newPetName)
        {
            OpenConnection();
            string sqlUpdate = $"Update Inventory set PetName = '{newPetName}' where CarId = '{id}'";
            using (SqlCommand sql = new SqlCommand(sqlUpdate, _sqlConnection))
            {
                sql.CommandType = CommandType.Text;
                sql.ExecuteNonQuery();
            }
            CloseConnection();
        }

        public string LookUpPetName(int carId)
        {
            OpenConnection();
            string carPetName;
            //Установить имя хранимой процедуры
            using (SqlCommand sqlCommand = new SqlCommand())
            {
                sqlCommand.Connection = _sqlConnection;
                sqlCommand.CommandText = "GetPetName";
                sqlCommand.CommandType = CommandType.StoredProcedure;
                //Входной параметр
                SqlParameter sqlParametr = new SqlParameter()
                {
                    ParameterName = @"carId",
                    SqlDbType = SqlDbType.Int,
                    Value = carId,
                    Direction = ParameterDirection.Input
                };

                sqlCommand.Parameters.Add(sqlParametr);
                //Выходной параметр
                sqlParametr = new SqlParameter()
                {
                    ParameterName = @"petName",
                    SqlDbType = SqlDbType.Char,
                    Size = 10,
                    Direction = ParameterDirection.Output
                };

                sqlCommand.Parameters.Add(sqlParametr);

                //выполнить хранимую процедуру
                sqlCommand.ExecuteNonQuery();
                //возвратить выходной параметр
                carPetName = (string)sqlCommand.Parameters[@"petName"].Value;
                CloseConnection();
            }
            return carPetName;
        }

        public void ProcessCreditRisk(bool throwEx, int custId)
        {
            OpenConnection();
            //Первым делом найти текущее имя по идентификатору клиента
            string fName;
            string lName;
            var cmdSelect = new SqlCommand($"Select * from Customers where CustId = {custId}", _sqlConnection);
            using (var dataReader = cmdSelect.ExecuteReader())
            {
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    fName = (string)dataReader["FirstName"];
                    lName = (string)dataReader["LastName"];
                }
                else
                {
                    CloseConnection();
                    return;
                }
            }

            //Создать объекты команд, которые представляют каждый шаг операции
            var cmdRemove = new SqlCommand($"Delete from Customers where CustId = {custId}", _sqlConnection);
            var cmdInsert = new SqlCommand("Insert into CreditRisks"
                + $"(FirstName, LastName) Values ('{fName}', '{lName}')", _sqlConnection);
            //это будет получено из объекта подключения
            SqlTransaction tx = null;
            try
            {
                tx = _sqlConnection.BeginTransaction();
                //Включить команды в транзакцию
                cmdInsert.Transaction = tx;
                cmdRemove.Transaction = tx;

                //Выполнить команды
                cmdInsert.ExecuteNonQuery();
                cmdRemove.ExecuteNonQuery();

                //Эмулировать ошибку
                if (throwEx)
                {
                    //Возникла ошибка, связанная с базой данных! Отказ транзакции
                    throw new Exception("Sorry! Database error! Tx failed...");
                }
                //Зафиксировать транзакцию!
                tx.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //любая ошибка приведет к откату транзакции
                //Использовать условную операцию для проверки на предмет null
                tx?.Rollback();
            }
            finally
            {
                CloseConnection();
            }
        }
    }
}
