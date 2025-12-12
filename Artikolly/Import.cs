using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Artikolly
{
    public partial class Import : Form
    {
        public Import()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Восстановление структуры БД из дампа
            try
            {
                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Получаем скрипт из дампа (только структура, без данных)
                    string sqlScript = GetDatabaseSchemaScript();

                    // Разделяем скрипт на отдельные команды
                    string[] commands = sqlScript.Split(
                        new[] { ";" },
                        StringSplitOptions.RemoveEmptyEntries
                    );

                    int tablesCreated = 0;

                    foreach (string command in commands)
                    {
                        if (!string.IsNullOrWhiteSpace(command))
                        {
                            try
                            {
                                using (MySqlCommand cmd = new MySqlCommand(command, conn))
                                {
                                    cmd.ExecuteNonQuery();

                                    // Считаем созданные таблицы
                                    if (command.Trim().StartsWith("CREATE TABLE", StringComparison.OrdinalIgnoreCase))
                                    {
                                        tablesCreated++;
                                    }
                                }
                            }
                            catch (MySqlException ex)
                            {
                                // Игнорируем ошибки "таблица уже существует"
                                if (!ex.Message.Contains("already exists"))
                                {
                                    throw;
                                }
                            }
                        }
                    }

                    MessageBox.Show($"Структура БД восстановлена успешно!\nСоздано таблиц: 10",
                                  "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при восстановлении структуры БД:\n" + ex.Message,
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetDatabaseSchemaScript()
        {
            
            return @"
-- Создание таблицы category
CREATE TABLE IF NOT EXISTS `category` (
  `CategoryID` int NOT NULL AUTO_INCREMENT,
  `CategoryName` varchar(100) NOT NULL,
  PRIMARY KEY (`CategoryID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Создание таблицы manufacturer
CREATE TABLE IF NOT EXISTS `manufacturer` (
  `ManufacturerID` int NOT NULL AUTO_INCREMENT,
  `ManufacturerName` varchar(100) NOT NULL,
  `CountryName` varchar(100) NOT NULL,
  PRIMARY KEY (`ManufacturerID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Создание таблицы role
CREATE TABLE IF NOT EXISTS `role` (
  `RoleId` int NOT NULL AUTO_INCREMENT,
  `RoleName` varchar(45) NOT NULL,
  PRIMARY KEY (`RoleId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Создание таблицы Status
CREATE TABLE IF NOT EXISTS `Status` (
  `StatusID` int NOT NULL AUTO_INCREMENT,
  `StatusName` varchar(100) NOT NULL,
  PRIMARY KEY (`StatusID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Создание таблицы suppliers
CREATE TABLE IF NOT EXISTS `suppliers` (
  `SuppliersID` int NOT NULL AUTO_INCREMENT,
  `SuppliersName` varchar(100) NOT NULL,
  `ContactInformation` varchar(150) NOT NULL,
  PRIMARY KEY (`SuppliersID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Создание таблицы user
CREATE TABLE IF NOT EXISTS `user` (
  `UserID` int NOT NULL AUTO_INCREMENT,
  `UserSurname` varchar(100) NOT NULL,
  `UserName` varchar(100) NOT NULL,
  `UserPatronomic` varchar(100) DEFAULT NULL,
  `DateOfBirth` date NOT NULL,
  `Phone` varchar(20) NOT NULL,
  `Login` tinytext NOT NULL,
  `Password` tinytext NOT NULL,
  `Role` int NOT NULL,
  PRIMARY KEY (`UserID`),
  KEY `Role` (`Role`),
  CONSTRAINT `Role` FOREIGN KEY (`Role`) REFERENCES `role` (`RoleId`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Создание таблицы product
CREATE TABLE IF NOT EXISTS `product` (
  `ProductArticleNumber` varchar(6) NOT NULL,
  `ProductName` varchar(100) NOT NULL,
  `ProductCost` float NOT NULL,
  `ProductDiscount` tinyint NOT NULL,
  `ProductManufacture` int NOT NULL,
  `ProductCategory` int NOT NULL,
  `ProductQuantilyStock` int NOT NULL,
  `ProductDescription` varchar(100) NOT NULL,
  `Volume` varchar(6) NOT NULL,
  `ProductPhoto` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`ProductArticleNumber`),
  KEY `Quantity` (`ProductCategory`),
  KEY `ProductManufacture_idx` (`ProductManufacture`),
  CONSTRAINT `ProductManufacture` FOREIGN KEY (`ProductManufacture`) REFERENCES `manufacturer` (`ManufacturerID`),
  CONSTRAINT `Quantity` FOREIGN KEY (`ProductCategory`) REFERENCES `category` (`CategoryID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Создание таблицы `order`
CREATE TABLE IF NOT EXISTS `order` (
  `OrderID` int NOT NULL AUTO_INCREMENT,
  `OrderDate` date NOT NULL,
  `OrderUser` int NOT NULL,
  `OrderStatus` int NOT NULL,
  `OrderTotalPrice` decimal(19,2) NOT NULL,
  `OrderTotalDiscount` decimal(19,2) NOT NULL,
  PRIMARY KEY (`OrderID`),
  KEY `order_ibfk_2_idx` (`OrderStatus`),
  KEY `order_ibfk_1_idx` (`OrderUser`),
  CONSTRAINT `order_ibfk_1` FOREIGN KEY (`OrderUser`) REFERENCES `user` (`UserID`),
  CONSTRAINT `order_ibfk_2` FOREIGN KEY (`OrderStatus`) REFERENCES `Status` (`StatusID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Создание таблицы orderproduct
CREATE TABLE IF NOT EXISTS `orderproduct` (
  `OrderID` int NOT NULL,
  `ProductArticleNumber` varchar(100) NOT NULL,
  `Count` int NOT NULL,
  PRIMARY KEY (`OrderID`,`ProductArticleNumber`),
  KEY `ProductArticleNumber` (`ProductArticleNumber`),
  CONSTRAINT `orderproduct_ibfk_1` FOREIGN KEY (`OrderID`) REFERENCES `order` (`OrderID`),
  CONSTRAINT `orderproduct_ibfk_2` FOREIGN KEY (`ProductArticleNumber`) REFERENCES `product` (`ProductArticleNumber`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Создание таблицы supplies
CREATE TABLE IF NOT EXISTS `supplies` (
  `SuppliesID` int NOT NULL AUTO_INCREMENT,
  `DateOfDelivery` date NOT NULL,
  `SupplierID` int NOT NULL,
  `ProductArticle` varchar(45) NOT NULL,
  `Quantity` int NOT NULL,
  PRIMARY KEY (`SuppliesID`),
  KEY `SupplierID_idx` (`SupplierID`),
  KEY `ProductArticle_idx` (`ProductArticle`),
  CONSTRAINT `ProductArticle` FOREIGN KEY (`ProductArticle`) REFERENCES `product` (`ProductArticleNumber`),
  CONSTRAINT `SupplierID` FOREIGN KEY (`SupplierID`) REFERENCES `suppliers` (`SuppliersID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Vostanovlenie vos = new Vostanovlenie();
            this.Hide();
            vos.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Authorization aut = new Authorization();
            this.Hide();
            aut.ShowDialog();
        }
    }
}
