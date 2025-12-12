using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Artikolly
{
    public partial class RedactManufacture : Form
    {
        private int manufacturerId;
        private string originalName;
        private string originalCountry;
        public RedactManufacture(int selectedManufacturerId)
        {
            InitializeComponent();
            manufacturerId = selectedManufacturerId;
            LoadManufacturerData();
        }
        private void LoadManufacturerData()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT ManufacturerName, CountryName FROM manufacturer WHERE ManufacturerID = @id";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", manufacturerId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            originalName = reader["ManufacturerName"].ToString();
                            originalCountry = reader["CountryName"].ToString();

                            textBox5.Text = originalName;
                            textBox6.Text = originalCountry;
                        }
                        else
                        {
                            MessageBox.Show("Производитель не найден!", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox5.Text) || string.IsNullOrEmpty(textBox6.Text))
            {
                MessageBox.Show("Необходимо заполнить все обязательные поля!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверяем, были ли изменения
            if (textBox5.Text == originalName && textBox6.Text == originalCountry)
            {
                MessageBox.Show("Не было изменений для сохранения.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"UPDATE manufacturer 
                                   SET ManufacturerName = @name, CountryName = @country 
                                   WHERE ManufacturerID = @id";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", textBox5.Text.Trim());
                    cmd.Parameters.AddWithValue("@country", textBox6.Text.Trim());
                    cmd.Parameters.AddWithValue("@id", manufacturerId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Данные успешно обновлены!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить данные.", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void RedactManufacture_Load(object sender, EventArgs e)
        {

        }
    }
}
