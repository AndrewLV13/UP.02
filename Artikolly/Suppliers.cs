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
    public partial class Suppliers : Form
    {
        public Suppliers()
        {
            InitializeComponent();
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.DarkBlue;
            // Настройка DataGridView
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ReadOnly = true;

            // Делаем кнопки неактивными при запуске
            button1.Enabled = false; // Редактировать
            button3.Enabled = false; // Удалить
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SuppliersAdd FormA = new SuppliersAdd();
            FormA.SupplierChanged += (s, args) => UpdateDataGrid(); // подписка на событие
            FormA.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int supplierId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["SuppliersID"].Value);
                RedactSuppliers FormA = new RedactSuppliers(supplierId); // передача ID
                FormA.SupplierChanged += (s, args) => UpdateDataGrid(); // подписка на событие
                FormA.ShowDialog();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите поставщика для редактирования.", "Информация",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

       

        private void UpdateDataGrid()
        {
            try
            {
                dataGridView1.Columns.Clear();
                dataGridView1.Rows.Clear();

                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string query = "SELECT SuppliersID, SuppliersName, ContactInformation FROM suppliers";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Создаем колонки (SuppliersID будет скрытой)
                        dataGridView1.Columns.Add("SuppliersID", "ID"); // Эта колонка будет скрыта
                        dataGridView1.Columns.Add("SuppliersName", "Название поставщика");
                        dataGridView1.Columns.Add("ContactInformation", "Контактная информация");

                        // Скрываем колонку с ID
                        dataGridView1.Columns["SuppliersID"].Visible = false;

                        // Заполняем данными
                        while (reader.Read())
                        {
                            string supplierId = reader["SuppliersID"].ToString();
                            string supplierName = reader["SuppliersName"].ToString();
                            string contactInfo = reader["ContactInformation"].ToString();

                            dataGridView1.Rows.Add(supplierId, supplierName, contactInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик выбора строки в DataGridView
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            // Активируем кнопки только если выбрана строка
            bool hasSelection = dataGridView1.SelectedRows.Count > 0;
            button1.Enabled = hasSelection;
            button3.Enabled = hasSelection;
        }

        // Дополнительно можно добавить обработчик двойного клика для быстрого редактирования
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Проверяем, что кликнули не по заголовку
            {
                button1_Click(sender, e);
            }
        }

        private void Suppliers_Load_1(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }


        //Удаление
        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите поставщика для удаления.", "Информация",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int supplierId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["SuppliersID"].Value);
                string supplierName = dataGridView1.SelectedRows[0].Cells["SuppliersName"].Value?.ToString();

                if (string.IsNullOrEmpty(supplierName))
                {
                    MessageBox.Show("Не удалось получить название поставщика.", "Ошибка");
                    return;
                }

                // Проверяем наличие связанных товаров
                if (HasSupplierReferences(supplierId))
                {
                    MessageBox.Show("Невозможно удалить поставщика, так как с ним связаны товары.",
                        "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Подтверждение удаления
                DialogResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить поставщика:\n\"{supplierName}\"?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Удаляем поставщика
                    if (DeleteSupplier(supplierId))
                    {
                        MessageBox.Show("Поставщик успешно удален!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Обновляем список и сбрасываем выбор
                        UpdateDataGrid();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить поставщика.", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении поставщика: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для проверки связей поставщика с товарами
        private bool HasSupplierReferences(int supplierId)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    // Проверяем несколько возможных таблиц, которые могут ссылаться на поставщика
                    List<string> queries = new List<string>
                    {
                        // Если у вас есть прямая связь товаров с поставщиками
                        "SELECT COUNT(*) FROM product WHERE ProductSupplier = @supplierId",
                        "SELECT COUNT(*) FROM product WHERE SuppliersID = @supplierId",
                        "SELECT COUNT(*) FROM product WHERE SupplierID = @supplierId",
                        // Если у вас есть таблица поставок
                        "SELECT COUNT(*) FROM supply WHERE SupplierID = @supplierId",
                        "SELECT COUNT(*) FROM supplies WHERE SupplierID = @supplierId",
                        "SELECT COUNT(*) FROM delivery WHERE SupplierID = @supplierId"
                    };

                    foreach (string query in queries)
                    {
                        try
                        {
                            using (var cmd = new MySqlCommand(query, connection))
                            {
                                cmd.Parameters.AddWithValue("@supplierId", supplierId);
                                object result = cmd.ExecuteScalar();
                                if (result != null && Convert.ToInt32(result) > 0)
                                {
                                    return true; // Найдены связи
                                }
                            }
                        }
                        catch (MySqlException ex)
                        {
                            // Игнорируем ошибки "таблица не существует" или "столбец не существует"
                            if (ex.Number != 1146 && ex.Number != 1054) // Table doesn't exist / Unknown column
                            {
                                // Для других ошибок логируем и продолжаем
                                Console.WriteLine($"Ошибка при выполнении запроса {query}: {ex.Message}");
                            }
                        }
                    }

                    return false; // Связей не найдено
                }
            }
            catch (Exception ex)
            {
                // В случае непредвиденной ошибки считаем что связи есть (безопасный подход)
                MessageBox.Show($"Ошибка при проверке связей поставщика: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
        }

        // Метод для удаления поставщика
        private bool DeleteSupplier(int supplierId)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    string query = "DELETE FROM suppliers WHERE SuppliersID = @supplierId";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@supplierId", supplierId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Если ошибка связана с внешними ключами
                if (ex.Number == 1451)
                {
                    MessageBox.Show("Невозможно удалить поставщика, так как с ним связаны товары или поставки.",
                        "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show($"Ошибка базы данных при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении поставщика: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}