using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;
using System.Data;

namespace Artikolly
{
    public partial class RedactProduct : Form
    {
        public event EventHandler ProductUpdated;
        private string productArticle;
        private string selectedImagePath = string.Empty;
        private string imageFileName = string.Empty;
        private string originalPhotoPath = string.Empty;

        public RedactProduct(string article)
        {
            InitializeComponent();
           
            productArticle = article;
            LoadProductData();
            LoadComboBoxData();
        }

        // Загрузка данных товара
        private void LoadProductData()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT 
                                p.ProductArticleNumber,
                                p.ProductName,
                                p.ProductCost,
                                p.ProductDiscount,
                                p.ProductDescription,
                                p.ProductManufacture,
                                p.ProductCategory,
                                p.ProductQuantilyStock,
                                p.ProductPhoto,
                                p.Volume,
                                m.ManufacturerName,
                                c.CategoryName
                            FROM product p
                            LEFT JOIN manufacturer m ON p.ProductManufacture = m.ManufacturerID
                            LEFT JOIN category c ON p.ProductCategory = c.CategoryID
                            WHERE p.ProductArticleNumber = @Article";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Article", productArticle);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Заполняем поля данными
                                textBox1.Text = reader["ProductArticleNumber"].ToString();
                                textBox2.Text = reader["ProductName"].ToString();
                                textBox3.Text = reader["ProductCost"].ToString();
                                textBox4.Text = reader["ProductDiscount"].ToString();
                                textBox5.Text = reader["ProductQuantilyStock"].ToString();
                                textBox6.Text = reader["ProductDescription"].ToString();
                                textBox7.Text = reader["Volume"].ToString();

                                // Сохраняем оригинальный путь к фото
                                originalPhotoPath = reader["ProductPhoto"]?.ToString();

                                // Загружаем изображение если есть
                                if (!string.IsNullOrEmpty(originalPhotoPath) && originalPhotoPath != "non.jpg")
                                {
                                    string photoFolder = Path.Combine(Application.StartupPath, "photo");
                                    string imagePath = Path.Combine(photoFolder, originalPhotoPath);

                                    if (File.Exists(imagePath))
                                    {
                                        try
                                        {
                                            Image selectedImage = Image.FromFile(imagePath);
                                            Image previewImage = ResizeImage(selectedImage, pictureBox5.Width, pictureBox5.Height);
                                            pictureBox5.Image = previewImage;
                                            pictureBox5.BackColor = Color.White;
                                            selectedImage.Dispose();

                                            
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка",
                                                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            pictureBox5.Image = null;
                                            pictureBox5.BackColor = Color.LightGray;
                                            label12.Text = "Ошибка загрузки изображения";
                                        }
                                    }
                                    else
                                    {
                                        pictureBox5.Image = null;
                                        pictureBox5.BackColor = Color.LightGray;
                                        label12.Text = "Изображение не найдено";
                                    }
                                }
                                else
                                {
                                    pictureBox5.Image = null;
                                    pictureBox5.BackColor = Color.LightGray;
                                    label12.Text = "Изображение не выбрано";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных товара: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Загрузка данных в комбобоксы
        private void LoadComboBoxData()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    // Загрузка производителей
                    string manufacturerQuery = "SELECT ManufacturerID, ManufacturerName FROM manufacturer";
                    using (var cmd = new MySqlCommand(manufacturerQuery, connection))
                    {
                        var dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());
                        comboBox1.DisplayMember = "ManufacturerName";
                        comboBox1.ValueMember = "ManufacturerID";
                        comboBox1.DataSource = dt;
                    }

                    // Загрузка категорий
                    string categoryQuery = "SELECT CategoryID, CategoryName FROM category";
                    using (var cmd = new MySqlCommand(categoryQuery, connection))
                    {
                        var dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());
                        comboBox2.DisplayMember = "CategoryName";
                        comboBox2.ValueMember = "CategoryID";
                        comboBox2.DataSource = dt;
                    }

                    // Устанавливаем текущие значения
                    SetCurrentComboBoxValues();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке справочников: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Установка текущих значений в комбобоксы
        private void SetCurrentComboBoxValues()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT ProductManufacture, ProductCategory 
                                  FROM product 
                                  WHERE ProductArticleNumber = @Article";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Article", productArticle);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                comboBox1.SelectedValue = reader["ProductManufacture"];
                                comboBox2.SelectedValue = reader["ProductCategory"];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при установке значений: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox6_Click(object sender, EventArgs e)
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

            // Разрешаем английские буквы
            if (char.IsLetter(e.KeyChar) && IsEnglishLetter(e.KeyChar))
            {
                return;
            }

            // Разрешаем цифры
            if (char.IsDigit(e.KeyChar))
            {
                return;
            }

            // Если символ не прошел ни одну проверку - запрещаем ввод
            e.Handled = true;
        }

        // Метод для проверки английских букв
        private bool IsEnglishLetter(char c)
        {
            // Английские буквы в диапазонах a-z и A-Z
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Проверка заполнения обязательных полей
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox2.Text) ||
                string.IsNullOrEmpty(textBox3.Text) || string.IsNullOrEmpty(textBox4.Text) ||
                string.IsNullOrEmpty(textBox5.Text) || string.IsNullOrEmpty(textBox6.Text) ||
                string.IsNullOrEmpty(textBox7.Text) || 
                comboBox2.SelectedValue == null)
            {
                MessageBox.Show("Необходимо заполнить все обязательные поля!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string productArticleNumber = textBox1.Text.Trim();
            string productName = textBox2.Text.Trim();
            string productCostText = textBox3.Text.Trim();
            string productDiscountText = textBox4.Text.Trim();
            string productQuantityText = textBox5.Text.Trim();
            string productDescription = textBox6.Text.Trim();
            string volume = textBox7.Text.Trim();

            // Конвертация в нужные типы
            if (!float.TryParse(productCostText, out float productCost))
            {
                MessageBox.Show("Некорректное значение стоимости!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!byte.TryParse(productDiscountText, out byte productDiscount))
            {
                MessageBox.Show("Некорректное значение скидки!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(productQuantityText, out int productQuantity))
            {
                MessageBox.Show("Некорректное значение количества!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Получаем ID производителя и категории из комбо-боксов
            int productManufacture = (int)comboBox1.SelectedValue;
            int productCategory = (int)comboBox2.SelectedValue;

            // Обработка изображения
            string productPhoto = originalPhotoPath; // Сохраняем текущее значение по умолчанию

            if (!string.IsNullOrEmpty(selectedImagePath))
            {
                try
                {
                    // Создаем имя файла на основе артикула
                    string extension = Path.GetExtension(selectedImagePath).ToLower();
                    productPhoto = $"{productArticleNumber}{extension}";

                    string photoFolder = Path.Combine(Application.StartupPath, "photo");

                    // Создаем папку photo, если она не существует
                    if (!Directory.Exists(photoFolder))
                    {
                        Directory.CreateDirectory(photoFolder);
                    }

                    string destinationPath = Path.Combine(photoFolder, productPhoto);

                    // Копируем файл (перезаписываем если существует)
                    File.Copy(selectedImagePath, destinationPath, true);

                    MessageBox.Show($"Изображение успешно сохранено: {productPhoto}", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении изображения: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"UPDATE product SET 
                                ProductArticleNumber = @ArticleNumber,
                                ProductName = @Name,
                                ProductCost = @Cost,
                                ProductDiscount = @Discount,
                                ProductDescription = @Description,
                                ProductManufacture = @Manufacturer,
                                ProductQuantilyStock = @Quantity,
                                Volume = @Volume,
                                ProductCategory = @Category,
                                ProductPhoto = @Photo
                                WHERE ProductArticleNumber = @OriginalArticle";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ArticleNumber", productArticleNumber);
                        cmd.Parameters.AddWithValue("@Name", productName);
                        cmd.Parameters.AddWithValue("@Cost", productCost);
                        cmd.Parameters.AddWithValue("@Discount", productDiscount);
                        cmd.Parameters.AddWithValue("@Description", productDescription);
                        cmd.Parameters.AddWithValue("@Manufacturer", productManufacture);
                        cmd.Parameters.AddWithValue("@Quantity", productQuantity);
                        cmd.Parameters.AddWithValue("@Volume", volume);
                        cmd.Parameters.AddWithValue("@Category", productCategory);
                        cmd.Parameters.AddWithValue("@Photo", productPhoto);
                        cmd.Parameters.AddWithValue("@OriginalArticle", productArticle);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Товар успешно обновлен!", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Вызываем событие обновления
                            ProductUpdated?.Invoke(this, EventArgs.Empty);

                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить товар!", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении товара: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
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

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
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

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
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

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
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

        private void textBox7_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем управляющие символы (Backspace, Delete и т.д.)
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Разрешаем английские буквы
            if (char.IsLetter(e.KeyChar) && IsEnglishLetter(e.KeyChar))
            {
                return;
            }

            // Разрешаем цифры
            if (char.IsDigit(e.KeyChar))
            {
                return;
            }

            // Если символ не прошел ни одну проверку - запрещаем ввод
            e.Handled = true;
        }



        // Метод для масштабирования изображения
        private Image ResizeImage(Image image, int maxWidth, int maxHeight)
        {
            double ratioX = (double)maxWidth / image.Width;
            double ratioY = (double)maxHeight / image.Height;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            Bitmap newImage = new Bitmap(newWidth, newHeight);

            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return newImage;
        }

        // Очистка выбранного изображения
        private void button3_Click(object sender, EventArgs e)
        {
            selectedImagePath = string.Empty;
            imageFileName = string.Empty;
            pictureBox1.Image = null;
            pictureBox1.BackColor = Color.LightGray;
            
        }
        private void RedactProduct_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files (*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = "Выберите изображение товара";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filePath = openFileDialog.FileName;

                        // Проверка размера файла (не более 2 МБ)
                        FileInfo fileInfo = new FileInfo(filePath);
                        if (fileInfo.Length > 2 * 1024 * 1024) // 2 МБ в байтах
                        {
                            MessageBox.Show("Размер изображения не должен превышать 2 МБ!", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Проверка формата файла
                        string extension = Path.GetExtension(filePath).ToLower();
                        if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                        {
                            MessageBox.Show("Разрешены только файлы форматов JPG, JPEG и PNG!", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Загружаем изображение для предпросмотра
                        Image selectedImage = Image.FromFile(filePath);

                        // Масштабируем изображение для предпросмотра
                        Image previewImage = ResizeImage(selectedImage, pictureBox1.Width, pictureBox1.Height);

                        // Устанавливаем изображение в PictureBox
                        pictureBox5.Image = previewImage;
                        pictureBox5.BackColor = Color.White;

                        // Сохраняем путь к оригинальному файлу
                        selectedImagePath = filePath;
                        imageFileName = Path.GetFileName(filePath);

                        // Показываем информацию о выбранном файле
                        label12.Text = $"Новое: {imageFileName}\nРазмер: {fileInfo.Length / 1024} КБ";

                        // Освобождаем ресурсы оригинального изображения
                        selectedImage.Dispose();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        // Очистка ресурсов изображения при закрытии формы
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (pictureBox5.Image != null)
            {
                pictureBox5.Image.Dispose();
                pictureBox5.Image = null;
            }
        }
    }
}
