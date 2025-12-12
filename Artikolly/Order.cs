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
    public partial class Order : Form
    {
        private int selectedOrderId = -1;
        public Order()
        {
            InitializeComponent();
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.DarkBlue;

            dataGridView1.RowTemplate.Height = 70;

            // Делаем кнопки неактивными при запуске
            
            button3.Enabled = false; // Удалить
            button2.Enabled = false; // Просмотр

            //Фильтрация
            comboBox1.Items.Add("Все диапазоны");
            comboBox1.Items.Add("В ожидании");
            comboBox1.Items.Add("Отгружен");
            comboBox1.Items.Add("Доставлен");
            comboBox1.Items.Add("В обработке");
            comboBox1.SelectedItem = "Все диапазоны";
            // Обработка изменения фильтра
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //просмотр состава заказа
        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedOrderId == -1)
            {
                MessageBox.Show("Пожалуйста, выберите заказ для просмотра.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Открываем форму OrderProduct и передаем ID выбранного заказа
                OrderProduct orderProductForm = new OrderProduct(selectedOrderId);
                orderProductForm.ShowDialog();

                // После закрытия формы обновляем данные в таблице
                // Это автоматически обновит отображение статуса
                UpdateDataGrid();

                // Сбрасываем выбранную строку
                dataGridView1.ClearSelection();
                selectedOrderId = -1;
                button2.Enabled = false;
                button3.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии состава заказа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Order_Load(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }

        private void UpdateDataGrid()
        {
            string statusFilter = "";
            string selectedStatus = comboBox1.SelectedItem?.ToString();

            // Обрабатываем выбранный статус - теперь используем ID статусов
            if (selectedStatus == "В ожидании")
                statusFilter = "o.OrderStatus = 1"; // Предполагая, что 1 = В ожидании
            else if (selectedStatus == "Отгружен")
                statusFilter = "o.OrderStatus = 2"; // Предполагая, что 2 = Отгружен
            else if (selectedStatus == "Доставлен")
                statusFilter = "o.OrderStatus = 3"; // Предполагая, что 3 = Доставлен
            else if (selectedStatus == "В обработке")
                statusFilter = "o.OrderStatus = 4"; // Предполагая, что 4 = В обработке
            // "Все диапазоны" или null - без фильтра по статусу

            // Текущий ввод поиска
            string input = textBox1.Text.Trim();
            bool isNumber = int.TryParse(input, out int orderNumber);
            string filterBySurname = "";
            string filterByOrderID = "";

            if (!string.IsNullOrEmpty(input))
            {
                if (isNumber)
                {
                    // Поиск по номеру заказа
                    filterByOrderID = $"o.OrderID = {orderNumber}";
                }
                else
                {
                    // Поиск по фамилии
                    string sellerSurname = input.ToLower();
                    filterBySurname = $"LOWER(u.UserSurname) LIKE '%{sellerSurname}%'";
                }
            }

            // Собираем список условий
            List<string> conditions = new List<string>();

            if (!string.IsNullOrEmpty(statusFilter))
                conditions.Add(statusFilter);
            if (!string.IsNullOrEmpty(filterBySurname))
                conditions.Add(filterBySurname);
            else if (!string.IsNullOrEmpty(filterByOrderID))
                conditions.Add(filterByOrderID);

            string whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

            // Обновленный запрос с JOIN таблицы Status для получения названий статусов
            string query = $"SELECT " +
                "o.OrderID AS НомерЗаказа, " +
                "o.OrderDate AS ДатаЗаказа, " +
                "s.StatusName AS Статус, " + // Берем название статуса из таблицы Status
                "CONCAT(u.UserSurname, ' ', u.UserName, ' ', u.UserPatronomic) AS Продавец, " +
                "o.OrderTotalPrice AS ОбщаяСтоимость, " +
                "o.OrderTotalDiscount AS ОбщаяСкидка " +
                "FROM `order` o " +
                "INNER JOIN `user` u ON o.OrderUser = u.UserID " +
                "INNER JOIN `status` s ON o.OrderStatus = s.StatusID " + // JOIN с таблицей статусов
                whereClause;

            FillDataGrid(query);
        }


        void FillDataGrid(string CMD)
        {
            try
            {
                dataGridView1.Columns.Clear();
                dataGridView1.Rows.Clear();

                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand(CMD, conn);
                    using (MySqlDataReader RDR = cmd.ExecuteReader())
                    {
                        // Создаем колонки для отображения заказов
                        dataGridView1.Columns.Add("OrderID", "Номер Заказа");
                        dataGridView1.Columns.Add("OrderDate", "Дата Заказа");
                        dataGridView1.Columns.Add("OrderStatus", "Статус");
                        dataGridView1.Columns.Add("Customer", "Продавец");
                        dataGridView1.Columns.Add("TotalPrice", "Общая стоимость");
                        dataGridView1.Columns.Add("TotalDiscount", "Общая скидка");

                        // Заполняем данными
                        while (RDR.Read())
                        {
                            string orderId = RDR["НомерЗаказа"].ToString();
                            string orderDate = RDR["ДатаЗаказа"].ToString();
                            string status = RDR["Статус"].ToString();
                            string customer = RDR["Продавец"].ToString();
                            string totalPrice = RDR["ОбщаяСтоимость"].ToString();
                            string totalDiscount = RDR["ОбщаяСкидка"].ToString();

                            dataGridView1.Rows.Add(orderId, orderDate, status, customer, totalPrice, totalDiscount);
                        }
                    }
                }
                selectedOrderId = -1;
                
                button3.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }




        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDataGrid();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Разрешаем только цифры (ASCII коды от 48 до 57)
            if (e.KeyChar < '0' || e.KeyChar > '9')
            {
                e.Handled = true;
            }
        }
        // Обработчик выбора строки в DataGridView
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Получаем ID выбранного заказа
                var orderIdCell = dataGridView1.SelectedRows[0].Cells["OrderID"];
                if (orderIdCell != null && orderIdCell.Value != null)
                {
                    if (int.TryParse(orderIdCell.Value.ToString(), out int orderId))
                    {
                        selectedOrderId = orderId;
                        button2.Enabled = true; // Просмотр
                        button3.Enabled = true; // Удалить
                    }
                    else
                    {
                        selectedOrderId = -1;
                        button2.Enabled = false;
                        button3.Enabled = false;
                    }
                }
            }
            else
            {
                selectedOrderId = -1;
                button2.Enabled = false;
                button3.Enabled = false;
            }
        }



        //Удаление
        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedOrderId == -1)
            {
                MessageBox.Show("Пожалуйста, выберите заказ для удаления.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Получаем информацию о выбранном заказе
                string orderInfo = "";
                string orderDate = "";
                string orderStatus = "";

                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string query = @"SELECT 
                o.OrderID,
                o.OrderDate,
                s.StatusName,
                CONCAT(u.UserSurname, ' ', u.UserName) as SellerName,
                o.OrderTotalPrice
            FROM `order` o
            INNER JOIN `user` u ON o.OrderUser = u.UserID
            INNER JOIN `status` s ON o.OrderStatus = s.StatusID
            WHERE o.OrderID = @orderId";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@orderId", selectedOrderId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            orderDate = reader["OrderDate"].ToString();
                            orderStatus = reader["StatusName"].ToString();
                            string sellerName = reader["SellerName"].ToString();
                            string totalPrice = reader["OrderTotalPrice"].ToString();

                            orderInfo = $"Заказ №{selectedOrderId}\n" +
                                       $"Дата: {orderDate}\n" +
                                       $"Статус: {orderStatus}\n" +
                                       $"Продавец: {sellerName}\n" +
                                       $"Сумма: {totalPrice} руб.";
                        }
                    }
                }

                if (string.IsNullOrEmpty(orderInfo))
                {
                    MessageBox.Show("Не удалось получить информацию о заказе.", "Ошибка");
                    return;
                }

                // Проверяем, можно ли удалить заказ (статус должен позволять удаление)
                if (!CanDeleteOrder(selectedOrderId))
                {
                    MessageBox.Show("Невозможно удалить заказ с текущим статусом. " +
                                  "Удаление возможно только для заказов в статусе 'В ожидании' или 'В обработке'.",
                        "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Подтверждение удаления
                DialogResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить заказ?\n\n{orderInfo}",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Удаляем заказ
                    if (DeleteOrder(selectedOrderId))
                    {
                        MessageBox.Show("Заказ успешно удален!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Обновляем список и сбрасываем выбор
                        UpdateDataGrid();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить заказ.", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении заказа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для проверки возможности удаления заказа
        private bool CanDeleteOrder(int orderId)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    // Проверяем статус заказа - разрешаем удаление только для определенных статусов
                    string query = @"SELECT s.StatusName 
                           FROM `order` o 
                           INNER JOIN `status` s ON o.OrderStatus = s.StatusID 
                           WHERE o.OrderID = @orderId";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);
                        string status = cmd.ExecuteScalar()?.ToString();

                        // Разрешаем удаление только для заказов в статусе "В ожидании" или "В обработке"
                        return status == "В ожидании" || status == "В обработке";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке статуса заказа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Метод для удаления заказа
        private bool DeleteOrder(int orderId)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    // 1. Сначала удаляем связанные товары из заказа
                    try
                    {
                        string deleteOrderProductsQuery = "DELETE FROM orderproduct WHERE OrderID = @orderId";
                        using (var cmd = new MySqlCommand(deleteOrderProductsQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@orderId", orderId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Игнорируем ошибки при удалении из orderproduct (может быть пустая таблица)
                        Console.WriteLine($"Ошибка при удалении из orderproduct: {ex.Message}");
                    }

                    // 2. Затем удаляем сам заказ
                    string deleteOrderQuery = "DELETE FROM `order` WHERE OrderID = @orderId";
                    using (var cmd = new MySqlCommand(deleteOrderQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);
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
                    MessageBox.Show("Невозможно удалить заказ, так как с ним связаны другие данные.",
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
                MessageBox.Show($"Ошибка при удалении заказа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

        }
    }
}
