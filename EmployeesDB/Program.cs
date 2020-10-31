using EmployeesDB.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;

namespace EmployeesDB
{
    class Program
    {

        static private EmployeesContext _context = new EmployeesContext();
        static void Main(string[] args)
        {
            //GetEmloyeesHighPaid();
            //AddAddress();
            //GetEmployeesWithProject();
            //GetEmployeesById();
            //GetSmallDepartament();
            //SalaryIncrease();
            DeleteDepartament();
            //DeleteTown();
           
        }

        static void GetEmloyeesHighPaid()
        {
            var employees = _context.Employees
            .Where(e => e.Salary > 48000 )
            .OrderBy(e => e.LastName)
            .Include(e => e.Department)
            .Include(e => e.Address)
            .Select(e => new
            { 
                e.FirstName,
                e.LastName,
                e.MiddleName,
                e.JobTitle,
                e.HireDate,
                e.Salary,
                Department = e.Department.Name,
                Address = e.Address.AddressText,
                Manager = e.Department.Manager.FirstName + " " + e.Department.Manager.LastName
            })
            .ToList();
            
            foreach (var e in employees)
            {
                Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", e.FirstName, e.LastName, e.MiddleName, e.JobTitle, e.HireDate, e.Salary, e.Department, e.Address, e.Manager);
            }
            
        }

        static void AddAddress()
        {
            Addresses address = new Addresses
            {
                AddressText = " 221b Baker street",
                TownId = 23
            };
            _context.Addresses.Add(address);
            _context.SaveChanges();

            var resEmployees = _context.Employees
                .Where(e => e.LastName.Contains("Brown"));

            var newAddress = _context.Addresses
                .Where(ad => ad.AddressText.Contains(address.AddressText))
                .FirstOrDefault();
            foreach (var e in resEmployees)
            {
                e.AddressId = newAddress.AddressId;
            }
            _context.SaveChanges();

            var employees = _context.Employees
               .Where(e => e.LastName == "Brown")
               .Select(e => new
               {
                   e.FirstName,
                   e.LastName,
                   e.MiddleName,
                   e.JobTitle,
                   e.HireDate,
                   e.Salary,
                   Department = e.Department.Name,
                   Address = e.Address.AddressText,
                   Manager = e.Department.Manager.FirstName + " " + e.Department.Manager.LastName
               })
               .ToList();

            foreach (var e in employees)
            {
                Console.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", e.FirstName, e.LastName, e.MiddleName, e.JobTitle, e.HireDate, e.Salary, e.Department, e.Address, e.Manager);
            }
        }

        static void GetEmployeesWithProject()
        {
            var dateStart = new DateTime(2002, 1, 1);
            var dateEnd = new DateTime(2005, 12, 31);
            var employees = _context.Employees
                .Include(e => e.EmployeesProjects)
                    .ThenInclude(e => e.Project)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.MiddleName,
                    Manager = e.Department.Manager.FirstName + " " + e.Department.Manager.LastName,
                    Project = e.EmployeesProjects.Select(p => new { p.Project.Name, p.Project.StartDate, p.Project.EndDate}).FirstOrDefault()
                })
                .Where(e => e.Project.StartDate >= dateStart && e.Project.StartDate <= dateEnd)
                .Take(5)
                .ToList();
            
            foreach(var e in employees)
            {
                Console.WriteLine("Employess: {0} {1} {2} Manager: {3}", e.FirstName, e.LastName, e.MiddleName, e.Manager);
                Console.Write("Project: {0} {1} {2}", e.Project.Name, e.Project.StartDate, e.Project.EndDate);
                if (e.Project.EndDate == null)
                {
                    Console.WriteLine("Проект не завершен");
                }
            }
        }

        static void GetEmployeesById()
        {
            int id;
            Console.Write("Id: ");
            id = Convert.ToInt32(Console.ReadLine());
            var employees = _context.Employees
                .Where(e => e.EmployeeId == id)
                .Include(e => e.EmployeesProjects)
                    .ThenInclude(e => e.Project)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.MiddleName,
                    e.JobTitle,
                    Project = e.EmployeesProjects.Select(p => p.Project.Name).ToArray()
                })
                .ToList();
            
            foreach(var e in employees)
            {
                Console.WriteLine("{0} {1} {2} {3}", e.FirstName, e.LastName, e.MiddleName, e.JobTitle);
                for (int i = 0; i < e.Project.Length; i++)
                {
                    Console.WriteLine(e.Project[i]);
                }
            }
        }
       
        static void GetSmallDepartament()
        {
            var employees = _context.Employees
                .Include(e => e.Department)
                .GroupBy(e => new
                {
                    e.DepartmentId,
                    e.Department.Name
                })
                .Select(e => new
                {
                    Count = e.Count(),
                    DepName = e.Key.Name
                })
                .Where(e => e.Count <= 5)
                .ToList();

            foreach(var e in employees)
            {
                Console.WriteLine(e.DepName);
            }
        }

        static void SalaryIncrease()
        {
            Console.Write("Departament: ");
            string departament = Console.ReadLine();
            Console.Write("Percent: ");
            int percent = Convert.ToInt32(Console.ReadLine());
            var employees = _context.Employees
                .Include(e => e.Department)
                .Where(e => e.Department.Name.Contains(departament));
            foreach(var e in employees)
            {
                e.Salary += e.Salary * percent / 100;
            }
            _context.SaveChanges();
        }

        static void DeleteDepartament()
        {
            Console.Write("Departament id: ");
            int id = Convert.ToInt32(Console.ReadLine());

            var departament = _context.Departments
                .Where(d => d.DepartmentId == id)
                .FirstOrDefault();
            
            _context.Departments.Remove(departament);
            _context.SaveChanges();
        }

        static void DeleteTown()
        {
            Console.Write("Town: ");
            string town = Console.ReadLine();
            var towns = _context.Towns
                .Where(t => t.Name.Contains(town))
                .FirstOrDefault();
            _context.Entry(towns).Collection(t => t.Addresses).Load();
            _context.Towns.Remove(towns);
            _context.SaveChanges();
        }
    }
}
