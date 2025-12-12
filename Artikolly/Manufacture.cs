using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;
namespace Artikolly
{
    public partial class Manufacture : Form
    {
     
        public Manufacture()
        {
            InitializeComponent();
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.DarkBlue;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Manufacture_Load(object sender, EventArgs e)
        {
            // Загрузка данных о производителях
            string queryManufacturers = @"SELECT 
                                    ManufacturerID AS ID,
                                    ManufacturerName AS Название,
                                    CountryName AS Страна
                                FROM manufacturer";

            FillManufacturersDataGrid(queryManufacturers);
        }


        private void FillManufacturersDataGrid(string query)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader RDR = cmd.ExecuteReader())
                    {
                        dataGridView1.Rows.Clear();
                        if (dataGridView1.Columns.Count == 0)
                        {
                            dataGridView1.Columns.Add("ID", "ID");
                            dataGridView1.Columns.Add("Название", "Название");
                            dataGridView1.Columns.Add("Страна", "Страна");
                        }
                        dataGridView1.Columns["ID"].Visible = false;
                        while (RDR.Read())
                        {
                            dataGridView1.Rows.Add(
                                RDR["ID"].ToString(),
                                RDR["Название"].ToString(),
                                RDR["Страна"].ToString()
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBox1.Text.Trim();

            // Формируем запрос с условием поиска по стране
            string queryManufacturers = @"SELECT 
                                    ManufacturerID AS ID,
                                    ManufacturerName AS Название,
                                    CountryName AS Страна
                                FROM manufacturer";

            if (!string.IsNullOrEmpty(searchText))
            {
                // Экранирование или подготовка для предотвращения SQL-инъекций желательно
                // Но для простоты здесь просто вставляем строку
                queryManufacturers += " WHERE CountryName LIKE @country";
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(queryManufacturers, conn);

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        cmd.Parameters.AddWithValue("@country", "%" + searchText + "%");
                    }

                    using (MySqlDataReader RDR = cmd.ExecuteReader())
                    {
                        dataGridView1.Rows.Clear();

                        if (dataGridView1.Columns.Count == 0)
                        {
                            dataGridView1.Columns.Add("ID", "ID");
                            dataGridView1.Columns.Add("Название", "Название");
                            dataGridView1.Columns.Add("Страна", "Страна");
                        }

                        while (RDR.Read())
                        {
                            dataGridView1.Rows.Add(
                                RDR["ID"].ToString(),
                                RDR["Название"].ToString(),
                                RDR["Страна"].ToString()
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedManufacturerId == -1)
            {
                MessageBox.Show("Выберите производителя для редактирования!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            RedactManufacture formEdit = new RedactManufacture(selectedManufacturerId);
            formEdit.FormClosed += (s, args) => FillManufacturersDataGrid(@"SELECT 
                                    ManufacturerID AS ID,
                                    ManufacturerName AS Название,
                                    CountryName AS Страна
                                FROM manufacturer");
            formEdit.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ManufactureAdd formAdd = new ManufactureAdd();
            formAdd.FormClosed += (s, args) => FillManufacturersDataGrid(@"SELECT 
                                    ManufacturerID AS ID,
                                    ManufacturerName AS Название,
                                    CountryName AS Страна
                                FROM manufacturer");
            formAdd.ShowDialog();
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
        private int selectedManufacturerId = -1; // Для хранения ID выбранного производителя
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Получаем ID выбранной строки
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // Предполагается, что ID находится в первой колонке
                if (row.Cells["ID"].Value != null)
                {
                    selectedManufacturerId = Convert.ToInt32(row.Cells["ID"].Value);
                    button2.Enabled = true;
                    button3.Enabled = true;
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        //удаление
        
        private void button3_Click(object sender, EventArgs e)
        {

            if (selectedManufacturerId == -1)
            {
                MessageBox.Show("Пожалуйста, выберите производителя для удаления.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var conn = DatabaseHelper.GetConnection())
            {
                // Проверка наличия связанной записи в таблице товаров
                string checkQuery = "SELECT COUNT(*) FROM product WHERE ProductManufacture = @id";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@id", selectedManufacturerId);
                conn.Open();
                int linkedRecordsCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (linkedRecordsCount > 0)
                {
                    MessageBox.Show("Невозможно удалить производителя, поскольку он используется в таблице товаров.", "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // Прерываем операцию удаления
                }
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить выбранного производителя?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    using (var conn = DatabaseHelper.GetConnection())
                    {
                        string deleteQuery = "DELETE FROM manufacturer WHERE ManufacturerID = @id";
                        MySqlCommand cmd = new MySqlCommand(deleteQuery, conn);
                        cmd.Parameters.AddWithValue("@id", selectedManufacturerId);
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Производитель успешно удален.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Производитель не найден или уже удален.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    // Обновляем таблицу после удаления
                    FillManufacturersDataGrid(@"SELECT 
                                    ManufacturerID AS ID,
                                    ManufacturerName AS Название,
                                    CountryName AS Страна
                                FROM manufacturer");
                    // Сброс выбранного ID и деактивация кнопки
                    selectedManufacturerId = -1;
                    button2.Enabled = false;
                    button3.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
