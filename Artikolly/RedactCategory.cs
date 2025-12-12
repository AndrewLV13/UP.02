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
    public partial class RedactCategory : Form
    {
        private string currentCategory;
        public RedactCategory(string selectedCategory)
        {
            InitializeComponent();
            currentCategory = selectedCategory;
            textBox1.Text = currentCategory;
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

        private void button1_Click(object sender, EventArgs e)
        {
            string newCategoryName = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(newCategoryName))
            {
                MessageBox.Show("Пожалуйста, введите название категории.");
                return;
            }

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                string query = "UPDATE category SET CategoryName = @name WHERE CategoryName = @oldName";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", newCategoryName);
                cmd.Parameters.AddWithValue("@oldName", currentCategory);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            MessageBox.Show("Категория отредактирована");
            this.Close();
        }
    }
    
}
