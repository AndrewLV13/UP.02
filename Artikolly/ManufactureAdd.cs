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
    public partial class ManufactureAdd : Form
    {
        public ManufactureAdd()
        {
            InitializeComponent();
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем управляющие символы (Backspace, Delete и т.д.)
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Проверяем символ с помощью регулярного выражения
            // Разрешаем: английские буквы, цифры, пробел, дефис и амперсанд
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), @"^[a-zA-Z0-9\s\-&]$"))
            {
                e.Handled = true;
            }
        }

        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем управляющие символы (Backspace, Delete и т.д.)
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

           

            // Разрешаем только русские буквы
            if (!IsRussianLetter(e.KeyChar))
            {
                e.Handled = true;
            }
            // Если текстовое поле пустое или курсор находится в начале, делаем букву заглавной
            if (textBox6.Text.Length == 0 || textBox6.SelectionStart == 0)
            {
                e.KeyChar = char.ToUpper(e.KeyChar);
            }
        }
        private bool IsRussianLetter(char c)
        {
            // Русские буквы в диапазоне от 'А' до 'я' (включая Ё и ё)
            return (c >= 'А' && c <= 'я') || c == 'Ё' || c == 'ё';
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string manufacturerName = textBox5.Text.Trim();
            string countryName = textBox6.Text.Trim();

            if (string.IsNullOrEmpty(manufacturerName) || string.IsNullOrEmpty(countryName))
            {
                MessageBox.Show("Необходимо заполнить все обязательные поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка на дублирование
            if (IsDuplicateManufacturer(manufacturerName, countryName))
            {
                MessageBox.Show("Такой производитель уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Добавление производителя
            if (AddManufacturer(manufacturerName, countryName))
            {
                MessageBox.Show("Производитель успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Очистка полей после успешного добавления
                textBox5.Text = "";
                textBox6.Text = "";
            }
            else
            {
                MessageBox.Show("Ошибка при добавлении производителя.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsDuplicateManufacturer(string name, string country)
        {
            bool isDuplicate = false;
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM manufacturer WHERE ManufacturerName = @name AND CountryName = @country";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@country", country);
                try
                {
                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count > 0)
                        isDuplicate = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при проверке на дублирование: " + ex.Message);
                }
            }
            return isDuplicate;
        }

        private bool AddManufacturer(string name, string country)
        {
            bool success = false;
            using (var conn = DatabaseHelper.GetConnection())
            {
                string insertQuery = "INSERT INTO manufacturer (ManufacturerName, CountryName) VALUES (@name, @country)";
                MySqlCommand cmd = new MySqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@country", country);
                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    success = rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при добавлении: " + ex.Message);
                }
            }
            return success;
        }
    }
}

