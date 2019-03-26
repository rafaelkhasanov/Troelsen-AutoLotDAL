using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoLotDAL.DataOperations;
using AutoLotDAL.Models;

namespace AutoLotClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            InventoryDAL dal = new InventoryDAL();
            var list = dal.GetAllInventory();
            Console.WriteLine("All Cars");
            Console.WriteLine("CarId\tMake\tColor\tPet Name");
            foreach (var item in list)
            {
                Console.WriteLine($"{item.CarId}\t{item.Make}\t{item.Color}\t{item.PetName}");
            }

            Console.WriteLine();
            var car = dal.GetCar(list.OrderBy(x => x.Color).Select(x => x.CarId).First());
            Console.WriteLine("First Car by Color");
            Console.WriteLine("CarId\tMake\tColor\tPet Name");
            Console.WriteLine($"{car.CarId}\t{car.Make}\t{car.Color}\t{car.PetName}");
            try
            {
                dal.DeleteCar(5);
                Console.WriteLine("Car deleted.");//запись об автомобиле удалена
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occured: {ex.Message}");
            }

            dal.InsertAuto(new Car
            {
                Color = "Blue",
                Make = "Pilot",
                PetName = "TowMonster"
            });
            list = dal.GetAllInventory();
            var newCar = list.First(x => x.PetName == "TowMonster");
            Console.WriteLine("New Car");
            Console.WriteLine("CarId\tMake\tColor\tPet Name");
            Console.WriteLine($"{newCar.CarId}\t{newCar.Make}\t{newCar.Color}\t{newCar.PetName}");
            dal.DeleteCar(newCar.CarId);
            var petName = dal.LookUpPetName(car.CarId);
            Console.WriteLine("New Car");
            Console.WriteLine($"Car pet name: {petName}");
            Console.WriteLine("Press enter to continue...");
            MoveCustomer();
            Console.ReadLine();
        }

        public static void MoveCustomer()
        {
            Console.WriteLine("Simple Transaction Example");
            //Простой способ позволить транзакции успешно завершиться или отказать
            bool throwEx = true;
            Console.Write("Do you want to throw an exception (Y or N): ");
            var userAnswer = Console.ReadLine();
            if(userAnswer?.ToLower() == "n")
            {
                throwEx = false;
            }

            var dal = new InventoryDAL();
            //Обработать клиента 1 - ввести идентификатор клиента, подлежащего перемещению
            dal.ProcessCreditRisk(throwEx, 1);
            Console.WriteLine("Check CreditRisk table for results");
            Console.ReadLine();
        }
    }
}
