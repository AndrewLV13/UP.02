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
    public partial class CategoryAdd : Form
    {
        public CategoryAdd()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {

            // Разрешаем управляющие символы (Backspace, Delete и т.д.)
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Проверяем длину текста
            if (textBox1.Text.Length >= 20)
            {
                e.Handled = true;
                return;
            }

            // Разрешаем только русские буквы
            if (!IsRussianLetter(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        private bool IsRussianLetter(char c)
        {
            // Русские буквы в диапазоне от 'А' до 'я' (включая Ё и ё)
            return (c >= 'А' && c <= 'я') || c == 'Ё' || c == 'ё';
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string categoryName = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(categoryName))
            {
                MessageBox.Show("Необходимо заполнить все обязательные поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка на дублирование
            if (IsDuplicateCategory(categoryName))
            {
                MessageBox.Show("Такая категория уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Добавление новой категории
            if (AddCategory(categoryName))
            {
                MessageBox.Show("Категория успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Очистка поля после успешного добавления
                textBox1.Text = string.Empty;
            }
            else
            {
                MessageBox.Show("Ошибка при добавлении категории.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsDuplicateCategory(string categoryName)
        {
            bool isDuplicate = false;
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM category WHERE CategoryName = @name";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", categoryName);
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

        private bool AddCategory(string categoryName)
        {
            bool success = false;
            using (var conn = DatabaseHelper.GetConnection())
            {
                string insertQuery = "INSERT INTO category (CategoryName) VALUES (@name)";
                MySqlCommand cmd = new MySqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@name", categoryName);
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

        private void CategoryAdd_Load(object sender, EventArgs e)
        {

        }
    }
}
