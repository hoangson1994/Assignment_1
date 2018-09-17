using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment1.Model
{
    class DataAccess
    {

        public static void InitializeDatabase()
        {
            using (SqliteConnection db =
                new SqliteConnection("Filename=users_manager.db"))
            {
                db.Open();

                String tableCommand = "CREATE TABLE IF NOT " +
                    "EXISTS users (id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "name NVARCHAR(50) NOT NULL, " +
                    "email NVARCHAR(255), " +
                    "phone NVARCHAR(50), " +
                    "address NVARCHAR(50), " +
                    "avatar NVARCHAR(255)" +                    
                    ") ";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();
            }
        }
    }
}
