using System.Data.SqlClient;
using Dapper;
using Employees.Application;
using Microsoft.Extensions.Configuration;

namespace Employees.Persistence
{
    public sealed class InitDbRepository : IInitDb
    {
        private readonly IConfiguration _configuration;

        public InitDbRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task Init()
        {
            //https://stackoverflow.com/questions/29190081/create-database-and-table-in-a-single-sql-script#comment46594682_29190121
            await CreateDatebase();
            await CreateTables();
        }

        private async Task CreateDatebase()
        {
            var query = """
                IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'EmployeeDatabase')
                BEGIN
                    CREATE DATABASE [EmployeeDatabase];
                END
                """;

            await using (var db = new SqlConnection(_configuration.GetConnectionString("InitDatabase")))
            {
                await db.ExecuteAsync(query);
            }
        }

        private async Task CreateTables()
        {
            var query = """
                IF OBJECT_ID(N'Companies', N'U') IS NULL
                BEGIN
                    CREATE TABLE Companies(
                	    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                	    Name VARCHAR(50) NULL,
                	);
                
                    INSERT INTO Companies(Name)
                    	VALUES 
                    		('Microsoft');
                    INSERT INTO Companies(Name)
                    	VALUES 
                    		('Apple');
                    INSERT INTO Companies(Name)
                    	VALUES 
                            ('Amazon');
                
                    INSERT INTO Companies(Name)
                    	VALUES('Google');
                END
                
                IF OBJECT_ID(N'Passports', N'U') IS NULL
                BEGIN
                    CREATE TABLE Passports (
                        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        Type VARCHAR(50) NOT NULL,
                        Number VARCHAR(20) NOT NULL,
                    );
                    INSERT INTO Passports
                           (Type
                           ,Number)
                     VALUES
                           ('type 1', '12121');
                    INSERT INTO Passports
                               (Type
                               ,Number)
                         VALUES
                               ('type 1', '14321');
                    INSERT INTO Passports
                               (Type
                               ,Number)
                         VALUES
                               ('type 2', '6543');
                    INSERT INTO Passports
                               (Type
                               ,Number)
                         VALUES
                               ('type 1', '7654');
                END
                
                
                IF OBJECT_ID(N'Departments', N'U') IS NULL
                BEGIN
                    CREATE TABLE Departments (
                      Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                      Name VARCHAR(50) NOT NULL,
                      Phone VARCHAR(30) NOT NULL,
                      CompanyId INT REFERENCES Companies(Id)
                    );
                
                    INSERT INTO Departments(Name, Phone, CompanyId)
                         VALUES('Ms It Department', '453-999', 1);
                    INSERT INTO Departments(Name, Phone, CompanyId)
                         VALUES('Ms Sales Department', '123-256', 1);
                    INSERT INTO Departments(Name, Phone, CompanyId)
                         VALUES('Apple IT Department', '123-456', 2);			
                    INSERT INTO Departments(Name, Phone, CompanyId)
                         VALUES('Apple Sales Department', '234-890', 2);
                    INSERT INTO Departments(Name, Phone, CompanyId)
                         VALUES('Amazon Marketing Department', '345-678', 3);			
                END
                
                IF OBJECT_ID(N'Employees', N'U') IS NULL
                BEGIN
                    CREATE TABLE Employees (
                      Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                      Name VARCHAR(200) NOT NULL,
                      Surname VARCHAR(200) NOT NULL,
                      Phone VARCHAR(30) NOT NULL,
                      CompanyId INT REFERENCES Companies(Id),
                      PassportId INT  REFERENCES Passports(Id),
                      DepartmentId INT REFERENCES Departments(Id)
                    );
                
                    INSERT INTO Employees
                               (Name
                               ,Surname
                               ,Phone
                               ,CompanyId
                               ,PassportId
                               ,DepartmentId)
                         VALUES 
                    			('Ron','Swanson','+7999111', 1, 1, 1);
                    INSERT INTO Employees
                               (Name
                               ,Surname
                               ,Phone
                               ,CompanyId
                               ,PassportId
                               ,DepartmentId)
                         VALUES
                    			('Leslie', 'Knope','7999222', 1, 2, 2);
                
                    INSERT INTO Employees
                               (Name
                               ,Surname
                               ,Phone
                               ,CompanyId
                               ,PassportId
                               ,DepartmentId)
                         VALUES
                    			('Tom', 'Haverford','796253', 2, 3, 3);
                    INSERT INTO Employees
                               (Name
                               ,Surname
                               ,Phone
                               ,CompanyId
                               ,PassportId
                               ,DepartmentId)
                         VALUES
                    			('Ann', 'Perkins','79992', 1, 4, 2);
                END
                """;

            await using (var db = new SqlConnection(_configuration.GetConnectionString("Database")))
            {
                await db.ExecuteAsync(query);
            }
        }
    }
}
