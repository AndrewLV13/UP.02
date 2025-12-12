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
    public partial class OrderProduct : Form
    {
        private string orderId;
        private string saveStatus;
        private bool changesSaved = false;
        public OrderProduct(int orderId)
        {
            InitializeComponent();
            this.orderId = orderId.ToString();
            this.Text = $"Состав заказа №{orderId}";

            LoadOrderData();
            FillDataGrid();
        }

        // Загрузка данных о заказе
        private void LoadOrderData()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            o.OrderID,
                            o.OrderDate,
                            o.OrderTotalPrice,
                            o.OrderTotalDiscount,
                            s.StatusName,
                            CONCAT(u.UserSurname, ' ', u.UserName, ' ', u.UserPatronomic) AS SellerFIO
                        FROM `order` o
                        INNER JOIN `status` s ON o.OrderStatus = s.StatusID
                        INNER JOIN `user` u ON o.OrderUser = u.UserID
                        WHERE o.OrderID = @OrderID";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Сохраняем исходные значения
                            saveStatus = reader["StatusName"].ToString();

                            // Заполняем поля формы
                            label2.Text = $"Номер заказа: {reader["OrderID"]}";
                            label3.Text = $"Дата заказа: {Convert.ToDateTime(reader["OrderDate"]):dd.MM.yyyy}";
                            label4.Text = $"Сумма заказа: {Convert.ToDecimal(reader["OrderTotalPrice"]):C2}";
                            label5.Text = $"Скидка заказа: {Convert.ToDecimal(reader["OrderTotalDiscount"]):C2}";

                            // Выводим ФИО продавца в TextBox
                            textBox1.Text = reader["SellerFIO"].ToString();

                            // Инициализируем ComboBox
                            LoadStatusComboBox();
                            comboBox1.SelectedItem = saveStatus;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных заказа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Загрузка статусов в ComboBox
        private void LoadStatusComboBox()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    string query = "SELECT StatusName FROM status ORDER BY StatusID";
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    using (var reader = cmd.ExecuteReader())
                    {
                        comboBox1.Items.Clear();
                        while (reader.Read())
                        {
                            comboBox1.Items.Add(reader["StatusName"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке статусов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Заполнение таблицы товаров
        void FillDataGrid()
        {
            try
            {
                string query = @"
                    SELECT 
                        p.ProductArticleNumber,
                        p.ProductName,
                        op.Count,
                        p.ProductCost,
                        p.ProductDiscount,
                        (p.ProductCost * op.Count) AS TotalWithoutDiscount,
                        (p.ProductCost * op.Count * (1 - p.ProductDiscount / 100)) AS TotalWithDiscount
                    FROM orderproduct op
                    INNER JOIN product p ON op.ProductArticleNumber = p.ProductArticleNumber
                    WHERE op.OrderID = @OrderID";

                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        dataGridView1.Rows.Clear();
                        dataGridView1.Columns.Clear();

                        // Создаем колонки
                        dataGridView1.Columns.Add("Article", "Артикул");
                        dataGridView1.Columns.Add("Name", "Наименование");
                        dataGridView1.Columns.Add("Count", "Количество");
                        dataGridView1.Columns.Add("Price", "Цена за шт.");
                        dataGridView1.Columns.Add("Discount", "Скидка");
                        dataGridView1.Columns.Add("Total", "Сумма");

                        // Настраиваем ширину колонок
                        dataGridView1.Columns["Article"].Width = 80;
                        dataGridView1.Columns["Name"].Width = 200;
                        dataGridView1.Columns["Count"].Width = 70;
                        dataGridView1.Columns["Price"].Width = 90;
                        dataGridView1.Columns["Discount"].Width = 70;
                        dataGridView1.Columns["Total"].Width = 90;

                        decimal totalOrderAmount = 0;

                        // Заполняем данными
                        while (reader.Read())
                        {
                            string article = reader["ProductArticleNumber"].ToString();
                            string name = reader["ProductName"].ToString();
                            int count = Convert.ToInt32(reader["Count"]);
                            decimal price = Convert.ToDecimal(reader["ProductCost"]);
                            decimal discountPercent = Convert.ToDecimal(reader["ProductDiscount"]);
                            decimal totalWithDiscount = Convert.ToDecimal(reader["TotalWithDiscount"]);

                            totalOrderAmount += totalWithDiscount;

                            dataGridView1.Rows.Add(
                                article,
                                name,
                                count,
                                price.ToString("C2"),
                                discountPercent > 0 ? $"{discountPercent}%" : "0%",
                                totalWithDiscount.ToString("C2")
                            );
                        }

                        // Обновляем общую сумму (если нужно)
                        label8.Text = $"Итого по товарам: {totalOrderAmount:C2}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке товаров заказа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckChanges();
        }


        private void CheckChanges()
        {
            bool statusChanged = comboBox1.SelectedItem?.ToString() != saveStatus;
            button1.Enabled = statusChanged;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(
               "Сохранить изменения в заказе?",
               "Подтверждение сохранения",
               MessageBoxButtons.YesNo,
               MessageBoxIcon.Question);

            if (dialogResult == DialogResult.No)
                return;

            try
            {
                // Получаем ID статуса из названия
                int statusId = GetStatusId(comboBox1.SelectedItem?.ToString());

                if (statusId == -1)
                {
                    MessageBox.Show("Не удалось определить ID статуса", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Обновляем заказ - только статус, без даты доставки
                string query = @"
                    UPDATE `order` 
                    SET OrderStatus = @StatusId
                    WHERE OrderID = @OrderID";

                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@StatusId", statusId);
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Изменения в заказе успешно сохранены", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Обновляем сохраненные значения
                        saveStatus = comboBox1.SelectedItem?.ToString();

                        // Деактивируем кнопку сохранения
                        button1.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Получение ID статуса по названию
        private int GetStatusId(string statusName)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    string query = "SELECT StatusID FROM status WHERE StatusName = @StatusName";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@StatusName", statusName);

                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1;
                }
            }
            catch
            {
                return -1;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
