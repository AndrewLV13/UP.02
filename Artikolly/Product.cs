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
    public partial class Product : Form
    {
        // Словарь для хранения путей к фотографиям
        Dictionary<string, string> Path_To_Photo = new Dictionary<string, string>();

        // Корзина товаров
        Dictionary<string, int> bucket = new Dictionary<string, int>();
        Dictionary<string, string> Photo = new Dictionary<string, string>();

        int CurrentRowIndex; // Индекс выбранной строки

        // Класс для хранения информации о товаре (добавлен CurrentStock)
        public class ProductInfo
        {
            public string Name { get; set; }
            public decimal Cost { get; set; }
            public string Description { get; set; }
            public int CurrentStock { get; set; } 
        }
        
        public Product()
        {
           
            InitializeComponent();
            
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.DarkBlue;

            LoadProducts();
            dataGridView1.RowTemplate.Height = 70;

            button1.Enabled = false;
            button3.Enabled = false;
            // Блокируем кнопку удаления для продавцов
            if (CurrentUser.RoleName == "Продавец")
            {
                button3.Enabled = false;
                button3.Visible = false;
                button1.Enabled = false;
                button1.Visible = false;

                button2.Enabled = false;
                button2.Visible = false;
            }
            //Сортировка
            comboBox1.Items.Add("По умолчанию");
            comboBox1.Items.Add("Возр. стоимости");
            comboBox1.Items.Add("Убыв. стоимости");
            comboBox1.SelectedItem = "По умолчанию";
            //Фильтрация
            comboBox2.Items.Add("Мужские");
            comboBox2.Items.Add("Женские");
            comboBox2.Items.Add("Унисекс");
            comboBox2.Items.Add("Все категории");
            comboBox2.SelectedItem = "Все категории";
            UpdateCartButton();
        }
        // Единый метод для загрузки данных
        public void LoadProducts()
        {
            try
            {
                string query = @"SELECT 
                    p.ProductArticleNumber AS Артикул,
                    p.ProductName AS Название,
                    p.ProductCost AS Стоимость,
                    p.ProductDiscount AS Скидка,
                    p.ProductDescription AS Описание,
                    m.ManufacturerName AS Производитель,
                    c.CategoryName AS Категория,
                    p.ProductPhoto
                FROM product p
                LEFT JOIN manufacturer m ON p.ProductManufacture = m.ManufacturerID
                LEFT JOIN category c ON p.ProductCategory = c.CategoryID
                ORDER BY p.ProductArticleNumber ASC";

                FillDataGridWithParameters(query, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке товаров: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void FillDataGrid(string CMD)
        {
            try
            {
                // Очищаем DataGridView правильно
                dataGridView1.DataSource = null;
                dataGridView1.Columns.Clear();
                dataGridView1.Rows.Clear();

                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    MySqlCommand cmd = new MySqlCommand(CMD, connection);
                    using (MySqlDataReader RDR = cmd.ExecuteReader())
                    {
                        // Создаем колонки
                        dataGridView1.Columns.Add("Артикул", "Артикул");
                        dataGridView1.Columns.Add("Название", "Название");
                        dataGridView1.Columns.Add("Стоимость", "Стоимость");
                        dataGridView1.Columns.Add("Скидка", "Скидка");
                        dataGridView1.Columns.Add("Описание", "Описание");
                        dataGridView1.Columns.Add("Производитель", "Производитель");
                        dataGridView1.Columns.Add("Категория", "Категория");
                        dataGridView1.Columns.Add("ProductPhoto", "ProductPhoto");
                        dataGridView1.Columns["Артикул"].Visible = false;
                        // Создаем колонку для фото
                        DataGridViewImageColumn ImageColumn = new DataGridViewImageColumn();
                        ImageColumn.Name = "Фото";
                        ImageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
                        dataGridView1.Columns.Add(ImageColumn);

                        // Скрываем столбец с именем файла
                        dataGridView1.Columns["ProductPhoto"].Visible = false;
                        

                        // Заполняем данными
                        while (RDR.Read())
                        {
                            int rowIndex = dataGridView1.Rows.Add(
                                RDR["Артикул"].ToString(),
                                RDR["Название"].ToString(),
                                RDR["Стоимость"].ToString(),
                                RDR["Скидка"].ToString(),
                                RDR["Описание"].ToString(),
                                RDR["Производитель"].ToString(),
                                RDR["Категория"].ToString(),
                                RDR["ProductPhoto"].ToString()
                            );

                           
                        }
                    }
                }

                // Загружаем фотографии
                PhotoPath();

                // Сбрасываем выбор
                button1.Enabled = false;
                button3.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для загрузки фотографий
        private void PhotoPath()
        {
            Path_To_Photo.Clear(); // Очищаем словарь

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Артикул"].Value == null) continue;

                string article = row.Cells["Артикул"].Value.ToString();
                string photoName = row.Cells["ProductPhoto"].Value?.ToString();

                if (string.IsNullOrEmpty(photoName))
                    photoName = "non.jpg";

                string photoPath = Path.Combine(Application.StartupPath, "photo", photoName);

                try
                {
                    if (File.Exists(photoPath))
                    {
                        // Загружаем и масштабируем изображение
                        Image originalImage = Image.FromFile(photoPath);
                        Image resizedImage = ResizeImage(originalImage, 70, 70);
                        row.Cells["Фото"].Value = resizedImage;
                        originalImage.Dispose(); // Освобождаем оригинальное изображение
                    }
                    else
                    {
                        // Создаем заглушку если файл не найден
                        row.Cells["Фото"].Value = CreatePlaceholderImage();
                    }
                }
                catch (Exception ex)
                {
                    // В случае ошибки используем заглушку
                    row.Cells["Фото"].Value = CreatePlaceholderImage();
                    Console.WriteLine($"Ошибка загрузки изображения {photoName}: {ex.Message}");
                }

                // Заполняем словарь
                Path_To_Photo[article] = photoPath;
            }
        }

        // Метод для масштабирования изображения
        private Image ResizeImage(Image image, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);

            using (Graphics graphics = Graphics.FromImage(result))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                graphics.DrawImage(image, 0, 0, width, height);
            }

            return result;
        }

        // Метод для создания изображения-заглушки
        private Image CreatePlaceholderImage()
        {
            Bitmap placeholder = new Bitmap(70, 70);

            using (Graphics graphics = Graphics.FromImage(placeholder))
            {
                graphics.Clear(Color.LightGray);

                using (Font font = new Font("Arial", 8))
                using (Brush brush = new SolidBrush(Color.DarkGray))
                {
                    StringFormat format = new StringFormat();
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    graphics.DrawString("No Image", font, brush,
                                      new RectangleF(0, 0, 70, 70), format);
                }
            }

            return placeholder;
        }

        // Метод для фильтрации и сортировки
        private void ApplyFiltersAndSort()
        {
            try
            {
                string baseQuery = @"SELECT 
                        p.ProductArticleNumber AS Артикул,
                        p.ProductName AS Название,
                        p.ProductCost AS Стоимость,
                        p.ProductDiscount AS Скидка,
                        p.ProductDescription AS Описание,
                        m.ManufacturerName AS Производитель,
                        c.CategoryName AS Категория,
                        p.ProductPhoto
                    FROM product p
                    LEFT JOIN manufacturer m ON p.ProductManufacture = m.ManufacturerID
                    LEFT JOIN category c ON p.ProductCategory = c.CategoryID
                    WHERE 1=1";

                List<string> conditions = new List<string>();
                List<MySqlParameter> parameters = new List<MySqlParameter>();

                // Добавляем фильтр по категории
                if (comboBox2.SelectedItem?.ToString() != "Все категории" && comboBox2.SelectedItem != null)
                {
                    conditions.Add("c.CategoryName = @CategoryName");
                    parameters.Add(new MySqlParameter("@CategoryName", comboBox2.SelectedItem.ToString()));
                }


                // Добавляем поиск
                if (!string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    conditions.Add("(p.ProductArticleNumber LIKE @Search OR p.ProductName LIKE @Search)");
                    parameters.Add(new MySqlParameter("@Search", $"%{textBox1.Text}%"));
                }

                // Комбинируем условия
                if (conditions.Count > 0)
                {
                    baseQuery += " AND " + string.Join(" AND ", conditions);
                }

                // Добавляем сортировку
                if (comboBox1.SelectedIndex == 1) // Возрастание
                {
                    baseQuery += " ORDER BY p.ProductCost ASC";
                }
                else if (comboBox1.SelectedIndex == 2) // Убывание
                {
                    baseQuery += " ORDER BY p.ProductCost DESC";
                }
                else // По умолчанию (можно добавить сортировку по артикулу или названию)
                {
                    baseQuery += " ORDER BY p.ProductArticleNumber ASC";
                }
                if (dataGridView1.Columns.Contains("Артикул"))
                {
                    dataGridView1.Columns["Артикул"].Visible = false;
                }

                FillDataGridWithParameters(baseQuery, parameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void FillDataGridWithParameters(string query, List<MySqlParameter> parameters)
        {
            try
            {
                // Очищаем DataGridView правильно
                dataGridView1.DataSource = null;
                dataGridView1.Columns.Clear();
                dataGridView1.Rows.Clear();

                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    // Добавляем параметры
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.Add(param);
                        }
                    }

                    using (MySqlDataReader RDR = cmd.ExecuteReader())
                    {
                        // Создаем колонки
                        dataGridView1.Columns.Add("Артикул", "Артикул");
                        dataGridView1.Columns.Add("Название", "Название");
                        dataGridView1.Columns.Add("Стоимость", "Стоимость");
                        dataGridView1.Columns.Add("Скидка", "Скидка");
                        dataGridView1.Columns.Add("Описание", "Описание");
                        dataGridView1.Columns.Add("Производитель", "Производитель");
                        dataGridView1.Columns.Add("Категория", "Категория");
                        dataGridView1.Columns.Add("ProductPhoto", "ProductPhoto");

                        // Создаем колонку для фото
                        DataGridViewImageColumn ImageColumn = new DataGridViewImageColumn();
                        ImageColumn.Name = "Фото";
                        ImageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
                        dataGridView1.Columns.Add(ImageColumn);

                        // Скрываем столбец с именем файла
                        dataGridView1.Columns["ProductPhoto"].Visible = false;
                        dataGridView1.Columns["Артикул"].Visible = false;

                        // Заполняем данными
                        while (RDR.Read())
                        {
                            int rowIndex = dataGridView1.Rows.Add(
                                RDR["Артикул"].ToString(),
                                RDR["Название"].ToString(),
                                RDR["Стоимость"].ToString(),
                                RDR["Скидка"].ToString(),
                                RDR["Описание"].ToString(),
                                RDR["Производитель"].ToString(),
                                RDR["Категория"].ToString(),
                                RDR["ProductPhoto"].ToString()
                            );
                        }
                    }
                }

                // Загружаем фотографии
                PhotoPath();

                // Сбрасываем выбор
                button1.Enabled = false;
                button3.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        // Сортировка
        string SortMod = "ASC";

        private void button2_Click(object sender, EventArgs e)
        {// Скрываем текущую форму товаров
            this.Hide();

            ProductADD addForm = new ProductADD();

            // Подписываемся на событие добавления товара
            addForm.ProductAdded += (s, args) =>
            {
                // Товар добавлен
            };

            // Подписываемся на событие закрытия формы добавления
            addForm.FormClosed += (s, args) =>
            {
                // Показываем форму товаров и обновляем данные
                this.Show();
                this.LoadProducts(); // Используем единый метод
                this.BringToFront(); // Делаем активной
            };

            addForm.Show(); // Открываем немодально
        }

        


            private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Product_Load(object sender, EventArgs e)
        {
            LoadProducts();
        }


        

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    // Активируем кнопки только если пользователь не продавец
                    if (CurrentUser.RoleName != "Продавец")
                    {
                        button1.Enabled = true;
                        button3.Enabled = true;
                    }

                    // Получаем путь к фотографии
                    string article = dataGridView1.Rows[e.RowIndex].Cells["Артикул"].Value?.ToString();
                    if (!string.IsNullOrEmpty(article) && Path_To_Photo.ContainsKey(article))
                    {
                        string path = Path_To_Photo[article];
                        // Можно добавить функционал предпросмотра фотографии
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFiltersAndSort();
        }

       

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Кнопка Редактировать
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.Index < 0)
            {
                MessageBox.Show("Выберите товар для редактирования!", "Информация",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string article = dataGridView1.CurrentRow.Cells["Артикул"].Value?.ToString();

                if (string.IsNullOrEmpty(article))
                {
                    MessageBox.Show("Не удалось получить артикул товара!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                RedactProduct editForm = new RedactProduct(article);

                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadProducts();
                    MessageBox.Show("Данные товара обновлены!", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии формы редактирования: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для проверки существования связей с другими таблицами
        private bool HasOrderReferences(string productArticle)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT COUNT(*) FROM orderproduct 
                                   WHERE ProductArticleNumber = @article";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@article", productArticle);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке связей: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
        }

        // Метод для удаления товара
        private bool DeleteProduct(string productArticle)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "DELETE FROM product WHERE ProductArticleNumber = @article";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@article", productArticle);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    MessageBox.Show("Невозможно удалить товар, так как он используется в заказах.",
                                  "Ошибка удаления",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show($"Ошибка при удалении товара: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        //удаление
        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.Index < 0)
            {
                MessageBox.Show("Выберите товар для удаления!", "Информация",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string article = dataGridView1.CurrentRow.Cells["Артикул"].Value?.ToString();

                if (string.IsNullOrEmpty(article))
                {
                    MessageBox.Show("Не удалось получить артикул товара!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string productName = dataGridView1.CurrentRow.Cells["Название"].Value?.ToString();

                DialogResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить товар \"{productName}\" (артикул: {article})?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    if (HasOrderReferences(article))
                    {
                        MessageBox.Show("Невозможно удалить товар, так как он используется в заказах.",
                                      "Ошибка удаления",
                                      MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (DeleteProduct(article))
                    {
                        MessageBox.Show("Товар успешно удален!", "Успех",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadProducts();
                        button1.Enabled = false;
                        button3.Enabled = false;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить товар.", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении товара: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_Sorted(object sender, EventArgs e)
        {
            PhotoPath();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            // Очищаем изображения в DataGridView
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Фото"]?.Value is Image image)
                {
                    image.Dispose();
                }
            }
        }


        // Нажатие правой кнопкой мыши
        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                var hitTest = dataGridView1.HitTest(e.X, e.Y);
                this.CurrentRowIndex = hitTest.RowIndex;

                // Проверяем, что клик был по строке с данными
                if (CurrentRowIndex >= 0 && CurrentRowIndex < dataGridView1.Rows.Count)
                {
                    ContextMenu m = new ContextMenu();
                    m.MenuItems.Add(new MenuItem("В корзину", contextmenu_click));

                    //// Добавляем пункт для просмотра корзины, если есть товары
                    //if (bucket.Count > 0)
                    //{
                    //    m.MenuItems.Add(new MenuItem("Просмотреть корзину", ViewCart_Click));
                    //}

                    dataGridView1.Rows[CurrentRowIndex].Selected = true;
                    m.Show(dataGridView1, new Point(e.X, e.Y));
                }
            }
        }

        // Обработчик контекстного меню
        void contextmenu_click(object sender, EventArgs e)
        {
            if (CurrentRowIndex < 0 || CurrentRowIndex >= dataGridView1.Rows.Count)
                return;

            string article = dataGridView1.Rows[CurrentRowIndex].Cells["Артикул"].Value?.ToString();

            if (string.IsNullOrEmpty(article))
                return;

            dataGridView1.Rows[CurrentRowIndex].Selected = false;

            try
            {
                if (bucket.ContainsKey(article))
                {
                    // Если товар уже в корзине, увеличиваем количество
                    bucket[article] = bucket[article] + 1;
                }
                else
                {
                    // Добавляем новый товар в корзину
                    bucket.Add(article, 1);

                    // Сохраняем путь к фото товара
                    if (Path_To_Photo.ContainsKey(article))
                    {
                        Photo[article] = Path_To_Photo[article];
                    }
                }

                // Обновляем отображение кнопки корзины
                UpdateCartButton();

                // Показываем сообщение об успешном добавлении
                string productName = dataGridView1.Rows[CurrentRowIndex].Cells["Название"].Value?.ToString();
                MessageBox.Show($"Товар \"{productName}\" добавлен в корзину!", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении в корзину: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Просмотр корзины
        void ViewCart_Click(object sender, EventArgs e)
        {
            ShowCartForm();
        }

        // Обновление отображения кнопки корзины
        private void UpdateCartButton()
        {
            if (bucket.Count > 0)
            {
                button4.Visible = true;
                button4.Text = $"Корзина ({bucket.Count} товар(ов))";

                // Подсчитываем общее количество товаров
                int totalItems = bucket.Values.Sum();
                button4.Text = $"Корзина ({totalItems} шт.)";
            }
            else
            {
                button4.Visible = false;
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
            ShowCartForm();
        }

        // Показ формы корзины
        private void ShowCartForm()
        {
            if (bucket.Count == 0)
            {
                MessageBox.Show("Корзина пуста!", "Информация",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Получаем актуальную информацию о товарах (включая количество на складе)
            var productInfo = GetProductInfoFromDatabase();

            // Создаем и показываем форму корзины
            Korzina cartForm = new Korzina(bucket, Photo, productInfo);

            if (cartForm.ShowDialog() == DialogResult.OK)
            {
                // Если заказ успешно оформлен, очищаем корзину
                bucket.Clear();
                Photo.Clear();
                UpdateCartButton();
            }
            else
            {
                // Обновляем корзину (возможно, пользователь изменил количество)
                UpdateCartButton();
            }
        }

        // Метод для получения информации о товарах из базы данных
        private Dictionary<string, ProductInfo> GetProductInfoFromDatabase()
        {
            var productInfo = new Dictionary<string, ProductInfo>();

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    // Создаем список параметров для безопасного SQL запроса
                    var parameters = new List<MySqlParameter>();
                    var parameterNames = new List<string>();

                    int paramIndex = 0;
                    foreach (var article in bucket.Keys)
                    {
                        string paramName = $"@article{paramIndex}";
                        parameterNames.Add(paramName);
                        parameters.Add(new MySqlParameter(paramName, article));
                        paramIndex++;
                    }

                    string query = $@"SELECT 
                        ProductArticleNumber,
                        ProductName,
                        ProductCost,
                        ProductDescription,
                        ProductQuantilyStock
                    FROM product 
                    WHERE ProductArticleNumber IN ({string.Join(",", parameterNames)})";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        // Добавляем параметры
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.Add(param);
                        }

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string article = reader["ProductArticleNumber"].ToString();
                                productInfo[article] = new ProductInfo
                                {
                                    Name = reader["ProductName"].ToString(),
                                    Cost = Convert.ToDecimal(reader["ProductCost"]),
                                    Description = reader["ProductDescription"].ToString(),
                                    CurrentStock = Convert.ToInt32(reader["ProductQuantilyStock"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке информации о товарах: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return productInfo;
        }


    }
}
  

