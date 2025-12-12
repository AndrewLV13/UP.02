using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Artikolly.Product;
using Word = Microsoft.Office.Interop.Word;

namespace Artikolly
{
    public partial class Korzina : Form
    {

        private Dictionary<string, int> bucket;
        private Dictionary<string, string> photos;
        private Dictionary<string, Product.ProductInfo> productInfo;

        DateTime NOW;
        DateTime deliverydate;
        int SelectRow; // текущая выбранная строка
        private int lastOrderId; // для хранения ID созданного заказа
        // Добавляем переменные для хранения суммы скидки
        private decimal totalDiscountAmount = 0;
        private decimal totalAmountWithoutDiscount = 0;



        public Korzina(Dictionary<string, int> bucket, Dictionary<string, string> photos, Dictionary<string, Product.ProductInfo> productInfo)
        {
            InitializeComponent();
            this.bucket = bucket;
            this.photos = photos;
            this.productInfo = productInfo;

            NOW = DateTime.Today;
            deliverydate = NOW.AddDays(3);

            label1.Text = "Дата заказа: " + NOW.ToString("dd.MM.yyyy");
            label2.Text = "Дата доставки: " + deliverydate.ToString("dd.MM.yyyy");

            // Инициализация numericUpDown1
            numericUpDown1.Minimum = 1;
            numericUpDown1.Maximum = 1000;
            numericUpDown1.Value = 1;
            numericUpDown1.Enabled = false;

            FillDataGrid();
            SumOrder();
            dataGridView1.ClearSelection();
        }
        // Заполняем данными DataGridView
        void FillDataGrid()
        {
            try
            {
                dataGridView1.Columns.Clear();

                // Добавляем колонку для фото
                DataGridViewImageColumn ImageColumn = new DataGridViewImageColumn();
                ImageColumn.Name = "Фото";
                ImageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
                dataGridView1.Columns.Add(ImageColumn);

                // Добавляем остальные колонки
                dataGridView1.Columns.Add("Article", "Артикул");
                dataGridView1.Columns.Add("ProductName", "Название");
                dataGridView1.Columns.Add("ProductCost", "Цена");
                dataGridView1.Columns.Add("ProductDiscount", "Скидка");
                dataGridView1.Columns.Add("ProductQuantityInStock", "В наличии");
                dataGridView1.Columns.Add("count", "Количество");
                dataGridView1.Columns.Add("Total", "Сумма");

                // Колонка для удаления (крестик)
                DataGridViewImageColumn ImageColumnClose = new DataGridViewImageColumn();
                ImageColumnClose.Name = "Delete";
                ImageColumnClose.HeaderText = "";
                ImageColumnClose.ImageLayout = DataGridViewImageCellLayout.Zoom;
                dataGridView1.Columns.Add(ImageColumnClose);

                // Настраиваем ширину колонок
                dataGridView1.Columns["Фото"].Width = 70;
                dataGridView1.Columns["Article"].Width = 80;
                dataGridView1.Columns["ProductName"].Width = 150;
                dataGridView1.Columns["ProductCost"].Width = 80;
                dataGridView1.Columns["ProductDiscount"].Width = 60;
                dataGridView1.Columns["ProductQuantityInStock"].Width = 70;
                dataGridView1.Columns["count"].Width = 80;
                dataGridView1.Columns["Total"].Width = 80;
                dataGridView1.Columns["Delete"].Width = 30;

                // Заполняем данными
                foreach (var item in bucket)
                {
                    string article = item.Key;
                    int quantity = item.Value;

                    if (productInfo.ContainsKey(article))
                    {
                        var info = productInfo[article];

                        // Загружаем изображение
                        Image productImage = null;
                        if (photos.ContainsKey(article) && File.Exists(photos[article]))
                        {
                            try
                            {
                                productImage = Image.FromFile(photos[article]);
                                productImage = ResizeImage(productImage, 60, 60);
                            }
                            catch
                            {
                                productImage = CreatePlaceholderImage();
                            }
                        }
                        else
                        {
                            productImage = CreatePlaceholderImage();
                        }

                        // Иконка удаления
                        Image deleteIcon = CreateDeleteIcon();

                        // Получаем скидку для товара
                        decimal discount = GetProductDiscount(article);
                        decimal discountedPrice = info.Cost * (1 - discount / 100);
                        decimal itemTotal = discountedPrice * quantity;

                        int rowIndex = dataGridView1.Rows.Add(
                            productImage,
                            article,
                            info.Name,
                            info.Cost.ToString("C2"),
                            discount > 0 ? $"{discount}%" : "0%",
                            info.CurrentStock.ToString(),
                            quantity,
                            itemTotal.ToString("C2"),
                            deleteIcon
                        );

                        // Проверяем наличие и подсвечиваем строку если товара недостаточно
                        if (quantity > info.CurrentStock)
                        {
                            dataGridView1.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightPink;
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

         // Получение скидки для товара из базы данных
        private decimal GetProductDiscount(string article)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    string query = @"
                SELECT 
                    COALESCE(p.ProductDiscount, 0) AS CurrentDiscount
                FROM product p
                WHERE p.ProductArticleNumber = @Article";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Article", article);

                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToDecimal(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении скидки: {ex.Message}");
            }

            return 0; // Если нет скидки или произошла ошибка
        }

        // Создаем изображение-заглушку
        private Image CreatePlaceholderImage()
        {
            Bitmap placeholder = new Bitmap(60, 60);
            using (Graphics g = Graphics.FromImage(placeholder))
            {
                g.Clear(Color.LightGray);
                using (Font font = new Font("Arial", 8))
                using (Brush brush = new SolidBrush(Color.DarkGray))
                {
                    StringFormat format = new StringFormat();
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    g.DrawString("No Image", font, brush, new Rectangle(0, 0, 60, 60), format);
                }
            }
            return placeholder;
        }

        // Создаем иконку удаления
        private Image CreateDeleteIcon()
        {
            Bitmap icon = new Bitmap(20, 20);
            using (Graphics g = Graphics.FromImage(icon))
            {
                g.Clear(Color.Transparent);
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    g.DrawLine(pen, 2, 2, 18, 18);
                    g.DrawLine(pen, 2, 18, 18, 2);
                }
            }
            return icon;
        }

        // Масштабирование изображения
        private Image ResizeImage(Image image, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.DrawImage(image, 0, 0, width, height);
            }
            return result;
        }

        // Расчет суммы заказа
        private void SumOrder()
        {
            try
            {
                decimal totalAmount = 0;
                decimal totalAmountWithoutDiscount = 0;
                decimal totalDiscountAmount = 0;
                int totalItems = 0;

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["Article"].Value != null)
                    {
                        string article = row.Cells["Article"].Value.ToString();
                        if (productInfo.ContainsKey(article))
                        {
                            var info = productInfo[article];
                            int quantity = Convert.ToInt32(row.Cells["count"].Value);

                            // Получаем скидку для товара
                            decimal discount = GetProductDiscount(article);

                            // Сумма без скидки
                            decimal itemTotalWithoutDiscount = info.Cost * quantity;
                            totalAmountWithoutDiscount += itemTotalWithoutDiscount;

                            // Сумма скидки для этого товара
                            decimal itemDiscountAmount = itemTotalWithoutDiscount * (discount / 100);
                            totalDiscountAmount += itemDiscountAmount;

                            // Итоговая сумма с учетом скидки
                            decimal itemTotal = itemTotalWithoutDiscount - itemDiscountAmount;
                            totalAmount += itemTotal;
                            totalItems += quantity;
                        }
                    }
                }

                // Сохраняем значения в полях класса
                this.totalAmountWithoutDiscount = totalAmountWithoutDiscount;
                this.totalDiscountAmount = totalDiscountAmount;

                // Отображаем информацию
                label4.Text = $"Общая стоимость: {totalAmount:C2}";
                if (totalDiscountAmount > 0)
                {
                    label5.Text = $"Скидка: -{totalDiscountAmount:C2}";
                    label5.Visible = true;
                    label6.Text = $"Сумма без скидки: {totalAmountWithoutDiscount:C2}";
                    label6.Visible = true;
                }
                else
                {
                    label5.Visible = false;
                    label6.Visible = false;
                }

                // Обновляем состояние кнопки оформления заказа
                button2.Enabled = CheckProductsAvailability() && dataGridView1.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчете суммы: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Сумма заказа
        private void label4_Click(object sender, EventArgs e)
        {

        }


        //Дата доставки
        private void label2_Click(object sender, EventArgs e)
        {

        }


        //дата заказа
        private void label1_Click_1(object sender, EventArgs e)
        {

        }



        //оформление заказа
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверяем наличие товаров
                if (!CheckProductsAvailability())
                {
                    MessageBox.Show("Некоторые товары недоступны в нужном количестве. Пожалуйста, проверьте корзину.",
                                  "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (dataGridView1.Rows.Count == 0)
                {
                    MessageBox.Show("Корзина пуста!", "Информация",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Создаем заказ в базе данных
                int orderId = CreateOrder();
                if (orderId > 0)
                {
                    MessageBox.Show("Заказ успешно оформлен!", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Обновляем количество товаров на складе
                    UpdateProductStock();

                    // Создаем чек в Word
                    CreateReceiptInWord(orderId);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при оформлении заказа. Попробуйте позже.", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;

                // Если редактировали колонку количества
                if (e.ColumnIndex == dataGridView1.Columns["count"].Index)
                {
                    string article = dataGridView1.Rows[e.RowIndex].Cells["Article"].Value?.ToString();
                    if (string.IsNullOrEmpty(article)) return;

                    if (int.TryParse(dataGridView1.Rows[e.RowIndex].Cells["count"].Value?.ToString(), out int newQuantity))
                    {
                        // Проверяем доступное количество
                        if (productInfo.ContainsKey(article))
                        {
                            int availableStock = productInfo[article].CurrentStock;

                            if (newQuantity <= 0)
                            {
                                // Удаляем товар если количество 0 или меньше
                                bucket.Remove(article);
                                photos.Remove(article);
                                dataGridView1.Rows.RemoveAt(e.RowIndex);
                                MessageBox.Show("Товар удален из корзины", "Информация",
                                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else if (newQuantity > availableStock)
                            {
                                // Превышено доступное количество
                                dataGridView1.Rows[e.RowIndex].Cells["count"].Value = availableStock;
                                bucket[article] = availableStock;
                                MessageBox.Show($"Превышено доступное количество товара. Установлено максимальное значение: {availableStock}",
                                              "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {
                                // Обновляем количество
                                bucket[article] = newQuantity;

                                // Обновляем сумму товара в таблице
                                decimal cost = productInfo[article].Cost;
                                dataGridView1.Rows[e.RowIndex].Cells["Total"].Value = (cost * newQuantity).ToString("C2");
                            }

                            // Обновляем numericUpDown1 если это выбранная строка
                            if (e.RowIndex == SelectRow)
                            {
                                numericUpDown1.Value = newQuantity;
                            }

                            // Пересчитываем общую сумму
                            SumOrder();

                            // Проверяем доступность товаров
                            CheckProductsAvailabilityAndHighlight();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении количества: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Проверка доступности всех товаров
        private bool CheckProductsAvailability()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Article"].Value != null)
                {
                    string article = row.Cells["Article"].Value.ToString();
                    int quantity = Convert.ToInt32(row.Cells["count"].Value);

                    if (productInfo.ContainsKey(article) && quantity > productInfo[article].CurrentStock)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

                SelectRow = e.RowIndex;
                string article = dataGridView1.Rows[e.RowIndex].Cells["Article"].Value?.ToString();
                string productName = dataGridView1.Rows[e.RowIndex].Cells["ProductName"].Value?.ToString();

                // Если кликнули на колонку удаления
                if (e.ColumnIndex == dataGridView1.Columns["Delete"].Index)
                {
                    DialogResult result = MessageBox.Show(
                        $"Удалить товар '{productName}' из корзины?",
                        "Подтверждение удаления",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        bucket.Remove(article);
                        photos.Remove(article);
                        dataGridView1.Rows.RemoveAt(e.RowIndex);
                        SumOrder();

                        if (dataGridView1.Rows.Count == 0)
                        {
                            numericUpDown1.Enabled = false;
                            numericUpDown1.Value = 1;
                        }
                        else
                        {
                            // Если удалили выбранную строку, сбрасываем selection
                            if (e.RowIndex == SelectRow)
                            {
                                numericUpDown1.Enabled = false;
                                numericUpDown1.Value = 1;
                                SelectRow = -1;
                            }
                        }
                    }
                }
                else
                {
                    // Если кликнули на любую другую ячейку - активируем numericUpDown1
                    if (article != null)
                    {
                        numericUpDown1.Enabled = true;
                        int currentQuantity = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["count"].Value);
                        numericUpDown1.Value = currentQuantity;

                        // Проверяем доступность товара
                        if (productInfo.ContainsKey(article))
                        {
                            int quantity = currentQuantity;
                            int availableStock = productInfo[article].CurrentStock;

                            // Устанавливаем максимальное значение для numericUpDown1
                            numericUpDown1.Maximum = Math.Max(availableStock, quantity);

                            // Подсвечиваем строку если товара недостаточно
                            if (quantity > availableStock)
                            {
                                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightPink;
                            }
                            else
                            {
                                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении товара: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private int CreateOrder()
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Пересчитываем суммы перед созданием заказа
                    SumOrder();

                    // 1. Создаем запись в таблице заказов
                    string insertOrderQuery = @"
                        INSERT INTO `order` (OrderDate, OrderUser, OrderStatus, OrderTotalPrice, OrderTotalDiscount) 
                        VALUES (@OrderDate, @OrderUser, @OrderStatus, @OrderTotalPrice, @OrderTotalDiscount);
                        SELECT LAST_INSERT_ID();";

                    MySqlCommand orderCmd = new MySqlCommand(insertOrderQuery, connection, transaction);
                    orderCmd.Parameters.AddWithValue("@OrderDate", NOW);
                    orderCmd.Parameters.AddWithValue("@OrderUser", CurrentUser.UserID);
                    orderCmd.Parameters.AddWithValue("@OrderStatus", 1);
                    orderCmd.Parameters.AddWithValue("@OrderTotalPrice", totalAmountWithoutDiscount);
                    orderCmd.Parameters.AddWithValue("@OrderTotalDiscount", totalDiscountAmount);

                    int orderId = Convert.ToInt32(orderCmd.ExecuteScalar());

                    // 2. Добавляем товары в заказ
                    string insertOrderProductQuery = @"
                        INSERT INTO orderproduct (OrderID, ProductArticleNumber, Count) 
                        VALUES (@OrderID, @Article, @Count)";

                    foreach (var item in bucket)
                    {
                        MySqlCommand orderProductCmd = new MySqlCommand(insertOrderProductQuery, connection, transaction);
                        orderProductCmd.Parameters.AddWithValue("@OrderID", orderId);
                        orderProductCmd.Parameters.AddWithValue("@Article", item.Key);
                        orderProductCmd.Parameters.AddWithValue("@Count", item.Value);
                        orderProductCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    lastOrderId = orderId;
                    return orderId;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"Ошибка при создании заказа: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -1;
                }
            }
        }

        // Получение общей суммы заказа
        private decimal GetTotalAmount()
        {
            decimal total = 0;
            foreach (var item in bucket)
            {
                if (productInfo.ContainsKey(item.Key))
                {
                    total += productInfo[item.Key].Cost * item.Value;
                }
            }
            return total;
        }

        private void UpdateProductStock()
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                try
                {
                    string updateQuery = @"
                        UPDATE product 
                        SET ProductQuantilyStock = ProductQuantilyStock - @Count 
                        WHERE ProductArticleNumber = @Article";

                    foreach (var item in bucket)
                    {
                        MySqlCommand cmd = new MySqlCommand(updateQuery, connection);
                        cmd.Parameters.AddWithValue("@Count", item.Value);
                        cmd.Parameters.AddWithValue("@Article", item.Key);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении остатков: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        // Создание чека в Word
        private void CreateReceiptInWord(int orderId)
        {
            Word.Application wordApp = null;
            Word.Document wordDoc = null;

            try
            {
                // Создаем экземпляр Word
                wordApp = new Word.Application();
                wordApp.Visible = false;

                // Создаем новый документ
                wordDoc = wordApp.Documents.Add();

                // Заполняем документ
                Word.Paragraph header = wordDoc.Paragraphs.Add();
                header.Range.Text = "Чек";
                header.Range.Font.Bold = 1;
                header.Range.Font.Size = 16;
                header.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                header.Range.InsertParagraphAfter();

                // Информация о заказе
                Word.Paragraph orderInfo = wordDoc.Paragraphs.Add();
                orderInfo.Range.Text = $"Заказ {orderId}";
                orderInfo.Range.Font.Size = 14;
                orderInfo.Range.InsertParagraphAfter();
                // Даты
                Word.Paragraph dates = wordDoc.Paragraphs.Add();
                dates.Range.Text = $"Дата заказа: {NOW:dd.MM.yyyy}";
                dates.Range.InsertParagraphAfter();

                // Общая сумма без скидки
                Word.Paragraph total = wordDoc.Paragraphs.Add();
                total.Range.Text = $"ИТОГО: {totalAmountWithoutDiscount:C2}";
                total.Range.Font.Size = 12;
                total.Range.InsertParagraphAfter();

                // Скидка (рассчитанная)
                Word.Paragraph discount = wordDoc.Paragraphs.Add();
                discount.Range.Text = $"СКИДКА: {totalDiscountAmount:C2}";
                discount.Range.Font.Size = 12;
                discount.Range.InsertParagraphAfter();

                // Код (например, ID пользователя)
                Word.Paragraph code = wordDoc.Paragraphs.Add();
                code.Range.Text = $"КОД: {CurrentUser.UserID}";
                code.Range.Font.Size = 12;
                code.Range.InsertParagraphAfter();

                // Разделитель
                Word.Paragraph separator = wordDoc.Paragraphs.Add();
                separator.Range.Text = "----------------------------------------";
                separator.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                separator.Range.InsertParagraphAfter();

                // Информация о компании
                Word.Paragraph companyInfo = wordDoc.Paragraphs.Add();
                companyInfo.Range.Text = "АИС магазин парфюмерии";
                companyInfo.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                companyInfo.Range.InsertParagraphAfter();

                Word.Paragraph fpInfo = wordDoc.Paragraphs.Add();
                fpInfo.Range.Text = "ФП: 01342342342 ФН № 82342353252";
                fpInfo.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                fpInfo.Range.InsertParagraphAfter();

                Word.Paragraph fdInfo = wordDoc.Paragraphs.Add();
                fdInfo.Range.Text = "ФД № 1169 САЙТ ФНС: www.na1o8.ru";
                fdInfo.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                fdInfo.Range.InsertParagraphAfter();

                Word.Paragraph deliveryInfo = wordDoc.Paragraphs.Add();
                deliveryInfo.Range.Text = $"Срок доставки - 3 дня      ИНН: 534534534";
                deliveryInfo.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                deliveryInfo.Range.InsertParagraphAfter();

                // Состав заказа
                Word.Paragraph compositionHeader = wordDoc.Paragraphs.Add();
                compositionHeader.Range.Text = "Состав заказа:";
                compositionHeader.Range.Font.Bold = 1;
                compositionHeader.Range.Font.Size = 12;
                compositionHeader.Range.InsertParagraphAfter();

                // Создаем таблицу для состава заказа
                int rowCount = bucket.Count + 1; // Заголовок + строки товаров
                Word.Table table = wordDoc.Tables.Add(
                    wordDoc.Paragraphs[wordDoc.Paragraphs.Count].Range,
                    rowCount,
                    5); // 5 колонок: Название, Количество, Цена, Скидка, Сумма

                // Заголовки таблицы
                table.Cell(1, 1).Range.Text = "Название";
                table.Cell(1, 2).Range.Text = "Количество";
                table.Cell(1, 3).Range.Text = "Цена";
                table.Cell(1, 4).Range.Text = "Скидка";
                table.Cell(1, 5).Range.Text = "Сумма";

                // Заполняем таблицу товарами
                int row = 2;
                foreach (var item in bucket)
                {
                    string article = item.Key;
                    int quantity = item.Value;

                    if (productInfo.ContainsKey(article))
                    {
                        var info = productInfo[article];
                        decimal discountPercent = GetProductDiscount(article);
                        decimal itemTotalWithoutDiscount = info.Cost * quantity;
                        decimal itemDiscountAmount = itemTotalWithoutDiscount * (discountPercent / 100);
                        decimal itemTotal = itemTotalWithoutDiscount - itemDiscountAmount;

                        table.Cell(row, 1).Range.Text = info.Name;
                        table.Cell(row, 2).Range.Text = quantity.ToString();
                        table.Cell(row, 3).Range.Text = info.Cost.ToString("C2");
                        table.Cell(row, 4).Range.Text = discountPercent > 0 ? $"{discountPercent}%" : "0%";
                        table.Cell(row, 5).Range.Text = itemTotal.ToString("C2");

                        row++;
                    }
                }

                // Форматируем таблицу
                table.Borders.Enable = 1;
                table.Rows[1].Range.Font.Bold = 1;
                table.Columns[1].Width = wordApp.CentimetersToPoints(6);
                table.Columns[2].Width = wordApp.CentimetersToPoints(2);
                table.Columns[3].Width = wordApp.CentimetersToPoints(2);
                table.Columns[4].Width = wordApp.CentimetersToPoints(2);
                table.Columns[5].Width = wordApp.CentimetersToPoints(3);

                // Итоговая сумма к оплате
                decimal totalToPay = totalAmountWithoutDiscount - totalDiscountAmount;
                Word.Paragraph finalTotal = wordDoc.Paragraphs.Add();
                finalTotal.Range.Text = $"ИТОГО К ОПЛАТЕ: {totalToPay:C2}";
                finalTotal.Range.Font.Bold = 1;
                finalTotal.Range.Font.Size = 14;
                finalTotal.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                finalTotal.Range.InsertParagraphAfter();

                // Дата и время
                Word.Paragraph dateTime = wordDoc.Paragraphs.Add();
                dateTime.Range.Text = $"Дата создания чека: {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}";
                dateTime.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                dateTime.Range.InsertParagraphAfter();

                // Сохраняем документ
                string receiptFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Чеки");
                if (!Directory.Exists(receiptFolder))
                {
                    Directory.CreateDirectory(receiptFolder);
                }

                string filePath = Path.Combine(receiptFolder, $"Чек_Заказ_{orderId}_{DateTime.Now:yyyyMMdd_HHmmss}.docx");
                wordDoc.SaveAs2(filePath);

                // Закрываем документ
                wordDoc.Close();
                wordApp.Quit();

                // Освобождаем COM-объекты
                Marshal.ReleaseComObject(wordDoc);
                Marshal.ReleaseComObject(wordApp);

                MessageBox.Show($"Чек успешно создан!\nФайл сохранен: {filePath}", "Чек создан",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании чека: {ex.Message}\nУбедитесь, что Microsoft Word установлен на вашем компьютере.",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Очистка COM-объектов в случае ошибки
                if (wordDoc != null)
                {
                    wordDoc.Close(false);
                    Marshal.ReleaseComObject(wordDoc);
                }
                if (wordApp != null)
                {
                    wordApp.Quit();
                    Marshal.ReleaseComObject(wordApp);
                }
            }
        }



        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (SelectRow >= 0 && SelectRow < dataGridView1.Rows.Count)
                {
                    string article = dataGridView1.Rows[SelectRow].Cells["Article"].Value?.ToString();
                    if (string.IsNullOrEmpty(article)) return;

                    int newQuantity = (int)numericUpDown1.Value;

                    // Проверяем доступное количество
                    if (productInfo.ContainsKey(article))
                    {
                        int availableStock = productInfo[article].CurrentStock;

                        if (newQuantity <= 0)
                        {
                            // Удаляем товар если количество 0 или меньше
                            bucket.Remove(article);
                            photos.Remove(article);
                            dataGridView1.Rows.RemoveAt(SelectRow);

                            // Сбрасываем selection
                            numericUpDown1.Enabled = false;
                            numericUpDown1.Value = 1;
                            SelectRow = -1;

                            MessageBox.Show("Товар удален из корзины", "Информация",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else if (newQuantity > availableStock)
                        {
                            // Превышено доступное количество
                            numericUpDown1.Value = availableStock;
                            dataGridView1.Rows[SelectRow].Cells["count"].Value = availableStock;
                            dataGridView1.Rows[SelectRow].Cells["Total"].Value =
                                (productInfo[article].Cost * availableStock).ToString("C2");
                            bucket[article] = availableStock;
                            MessageBox.Show($"Превышено доступное количество товара. Установлено максимальное значение: {availableStock}",
                                          "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            // Обновляем количество в DataGridView
                            dataGridView1.Rows[SelectRow].Cells["count"].Value = newQuantity;
                            dataGridView1.Rows[SelectRow].Cells["Total"].Value =
                                (productInfo[article].Cost * newQuantity).ToString("C2");
                            bucket[article] = newQuantity;
                        }

                        // Пересчитываем общую сумму
                        SumOrder();

                        // Проверяем доступность товаров и обновляем подсветку
                        CheckProductsAvailabilityAndHighlight();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении количества: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        // Проверка доступности всех товаров и подсветка
        private void CheckProductsAvailabilityAndHighlight()
        {
            bool allAvailable = true;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Article"].Value != null)
                {
                    string article = row.Cells["Article"].Value.ToString();
                    int quantity = Convert.ToInt32(row.Cells["count"].Value);

                    if (productInfo.ContainsKey(article))
                    {
                        if (quantity > productInfo[article].CurrentStock)
                        {
                            row.DefaultCellStyle.BackColor = Color.LightPink;
                            allAvailable = false;
                        }
                        else
                        {
                            row.DefaultCellStyle.BackColor = Color.White;
                        }
                    }
                }
            }

            // Обновляем состояние кнопки оформления заказа
            button2.Enabled = allAvailable && dataGridView1.Rows.Count > 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
