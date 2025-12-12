using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Artikolly
{
    class DatabaseHelper
    {
        // Строка подключения для MySQL
        private static readonly string connectionString = @"server=localhost;user=root;database=artikolly;password=;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
    
}
