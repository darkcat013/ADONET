using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADONET
{
    static class Program
    { 
        static string connectionString = ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString;
        static DataTable authorsTable;
        static SqlDataAdapter authorsAdapter;
        static void CheckTables()
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (var sqlCommand = new SqlCommand("DROP TABLE IF EXISTS [Articles]", sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();
                }
                using (var sqlCommand = new SqlCommand("DROP TABLE IF EXISTS [Authors]", sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
        static void CreateTables()
        {
            string authorsCommand = "CREATE TABLE [Authors]([Id] INT NOT NULL, [Name] NVARCHAR(64), [Surname] NVARCHAR(64), CONSTRAINT[PK_Authors] PRIMARY KEY CLUSTERED([Id]))";
            string articleCommand = "CREATE TABLE [Articles]([Id] INT NOT NULL IDENTITY(1,1), [Title] NVARCHAR(128) NOT NULL, [AuthorId] INT, CONSTRAINT [PK_Articles] PRIMARY KEY CLUSTERED ([Id]), CONSTRAINT [FK_Author] FOREIGN KEY([AuthorId]) REFERENCES [Authors]([Id]))";
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                using(var sqlCommand = new SqlCommand(authorsCommand, sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();
                }
                using (var sqlCommand = new SqlCommand(articleCommand, sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }
        static DataTable InitializeAuthorsTable()
        {
            DataTable tempTable = new DataTable("Authors");
            tempTable.Columns.Add("Id", typeof(int));
            tempTable.Columns.Add("Name", typeof(string));
            tempTable.Columns.Add("Surname", typeof(string));
            return tempTable;
        }
        static SqlDataAdapter InitializeAuthorsAdapter()
        {
            SqlDataAdapter tempAdapter = new SqlDataAdapter("SELECT Id, Name, Surname FROM Authors", connectionString);

            tempAdapter.InsertCommand = new SqlCommand("INSERT INTO Authors(Id, Name, Surname) VALUES (@Id, @Name, @Surname)");
            tempAdapter.InsertCommand.Parameters.Add("@Id", SqlDbType.Int).SourceColumn = "Id";
            tempAdapter.InsertCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 64, "Name");
            tempAdapter.InsertCommand.Parameters.Add("@Surname", SqlDbType.NVarChar, 64, "Surname");

            tempAdapter.UpdateCommand = new SqlCommand("UPDATE Authors SET Name = @Name, Surname = @Surname WHERE Id = @Id");
            tempAdapter.UpdateCommand.Parameters.Add("@Id", SqlDbType.Int).SourceColumn = "Id";
            tempAdapter.UpdateCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 64, "Name");
            tempAdapter.UpdateCommand.Parameters.Add("@Surname", SqlDbType.NVarChar, 64, "Surname");

            tempAdapter.DeleteCommand = new SqlCommand("DELETE FROM Authors WHERE Id = @Id");
            tempAdapter.DeleteCommand.Parameters.Add("@Id", SqlDbType.Int).SourceColumn = "Id";

            return tempAdapter;
        }
        static void InsertIntoAuthors()
        {
            DataTable tempAuthorsTable = InitializeAuthorsTable();
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                List<Author> authors = new List<Author>
                {
                    new Author{Id = 1, Name = "Viorel", Surname = "Noroc" },
                    new Author{Id = 2, Name = "Evan", Surname = "Ermac" },
                    new Author{Id = 3, Name = "Mike", Surname = "Smith" },
                    new Author{Id = 4, Name = "Mario", Surname = "Luigi" },
                    new Author{Id = 5, Name = "Albert", Surname = "Albert" },
                    new Author{Id = 6, Name = "Ronald", Surname = "Meriweather" }
                };
                foreach (var item in authors)
                {
                    var row = tempAuthorsTable.NewRow();
                    row["Id"] = item.Id;
                    row["Name"] = item.Name;
                    row["Surname"] = item.Surname;
                    tempAuthorsTable.Rows.Add(row);
                }
                authorsAdapter.InsertCommand.Connection = sqlConnection;
                authorsAdapter.Update(tempAuthorsTable);
            }
        }
        static void InsertAuthorToAuthorTable(Author author)
        {
            var row = authorsTable.NewRow();
            row["Id"] = author.Id;
            row["Name"] = author.Name;
            row["Surname"] = author.Surname;
            authorsTable.Rows.Add(row);
        }
        static void UpdateAuthorInAuthorTable(Author author)
        {
            foreach (DataRow row in authorsTable.Rows)
            {
                if((int)row["Id"] == author.Id)
                {
                    row["Name"] = author.Name;
                    row["Surname"] = author.Surname;
                }
            }
        }
        static void DeleteRowInAuthorTable(int id)
        {
            foreach (DataRow row in authorsTable.Rows)
            {
                if ((int)row["Id"] == id)
                {
                    row.Delete();
                }
            }
        }
        static void CommitChanges()
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                authorsAdapter.InsertCommand.Connection = sqlConnection;
                authorsAdapter.UpdateCommand.Connection = sqlConnection;
                authorsAdapter.DeleteCommand.Connection = sqlConnection;
                authorsAdapter.Update(authorsTable);
            }
        }
        static void PrintAuthors(DataTable authorTable)
        {
            Console.WriteLine("Id \t Name   \t Surname");
            foreach (DataRow row in authorsTable.Rows)
            {
                Console.WriteLine($"{row["Id"]} \t {row["Name"]}   \t {row["Surname"]}");
            }
        }
        static void Main(string[] args)
        {
            CheckTables();
            CreateTables();
            authorsAdapter = InitializeAuthorsAdapter();
            InsertIntoAuthors();
            authorsTable = InitializeAuthorsTable();
            authorsAdapter.Fill(authorsTable);
            PrintAuthors(authorsTable);
            InsertAuthorToAuthorTable(new Author { Id = 7, Name = "Oogway", Surname = "Master" });
            UpdateAuthorInAuthorTable(new Author { Id = 2, Name = "Victor", Surname = "Noroc" });
            DeleteRowInAuthorTable(3);
            Console.WriteLine("\n Commiting changes \n");
            CommitChanges();
            DataTable tempAuthorsTable = InitializeAuthorsTable();
            authorsAdapter.Fill(tempAuthorsTable);
            PrintAuthors(tempAuthorsTable);
            Console.ReadKey();
        }
    }
}
