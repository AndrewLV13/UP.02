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
    public partial class RedactRoly : Form
    {
        private string currentRole;
        public RedactRoly(string selectedRole)
        {
            InitializeComponent();
            currentRole = selectedRole;
            textBox1.Text = currentRole;
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
            string newRoleName = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(newRoleName))
            {
                MessageBox.Show("Пожалуйста, введите название роли.");
                return;
            }

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                string query = "UPDATE role SET RoleName = @name WHERE RoleName = @oldName";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", newRoleName);
                cmd.Parameters.AddWithValue("@oldName", currentRole);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Роль отредактирована");
            this.Close();

        }
    }
}
