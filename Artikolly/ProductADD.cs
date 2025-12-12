using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Artikolly
{
    public partial class ProductADD : Form
    {
        public event EventHandler ProductAdded;
        private string selectedImagePath = string.Empty;
        private string imageFileName = string.Empty;

        public ProductADD()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {

            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {


            // Проверка заполнения обязательных полей
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox2.Text) ||
                string.IsNullOrEmpty(textBox3.Text) || string.IsNullOrEmpty(textBox4.Text) ||
                string.IsNullOrEmpty(textBox5.Text) || string.IsNullOrEmpty(textBox6.Text) ||
                string.IsNullOrEmpty(textBox7.Text) || comboBox1.SelectedValue == null ||
                comboBox2.SelectedValue == null)
            {
                MessageBox.Show("Необходимо заполнить все обязательные поля!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Некорректное значение стоимости!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!byte.TryParse(productDiscountText, out byte productDiscount))
            {
                MessageBox.Show("Некорректное значение скидки!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(productQuantityText, out int productQuantity))
            {
                MessageBox.Show("Некорректное значение количества!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Получаем ID производителя и категории из комбо-боксов
            int productManufacture = (int)comboBox1.SelectedValue;
            int productCategory = (int)comboBox2.SelectedValue;

            // Обработка изображения
            string productPhoto = "non.jpg"; // Значение по умолчанию
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

                    // Копируем файл
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

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                // Проверка на дублирование по артикулу
                string checkQuery = "SELECT COUNT(*) FROM product WHERE ProductArticleNumber = @articleNumber";
                using (var cmdCheck = new MySqlCommand(checkQuery, connection))
                {
                    cmdCheck.Parameters.AddWithValue("@articleNumber", productArticleNumber);
                    int count = Convert.ToInt32(cmdCheck.ExecuteScalar());
                    if (count > 0)
                    {
                        MessageBox.Show("Товар с таким артикулом уже существует!", "Дубль", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Вставка нового товара
                string insertQuery = @"INSERT INTO product 
            (ProductArticleNumber, ProductName, ProductCost, ProductDiscount, ProductManufacture, ProductCategory, ProductQuantilyStock, ProductDescription, Volume, ProductPhoto)
            VALUES
            (@articleNumber, @name, @cost, @discount, @manufacture, @category, @quantity, @description, @volume, @photo)";

                using (var cmdInsert = new MySqlCommand(insertQuery, connection))
                {
                    cmdInsert.Parameters.AddWithValue("@articleNumber", productArticleNumber);
                    cmdInsert.Parameters.AddWithValue("@name", productName);
                    cmdInsert.Parameters.AddWithValue("@cost", productCost);
                    cmdInsert.Parameters.AddWithValue("@discount", productDiscount);
                    cmdInsert.Parameters.AddWithValue("@manufacture", productManufacture);
                    cmdInsert.Parameters.AddWithValue("@category", productCategory);
                    cmdInsert.Parameters.AddWithValue("@quantity", productQuantity);
                    cmdInsert.Parameters.AddWithValue("@description", productDescription);
                    cmdInsert.Parameters.AddWithValue("@volume", volume);
                    cmdInsert.Parameters.AddWithValue("@photo", productPhoto);

                    try
                    {
                        int rowsAffected = cmdInsert.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Товар успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Вызываем событие
                            ProductAdded?.Invoke(this, EventArgs.Empty);

                            ClearAllFields();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при добавлении товара: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }

        // Метод для очистки всех полей
        private void ClearAllFields()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";

            // Сбрасываем изображение
            selectedImagePath = string.Empty;
            imageFileName = string.Empty;
            pictureBox5.Image = null;
            pictureBox5.BackColor = Color.LightGray;
            label12.Text = "Изображение не выбрано";

            // Сбрасываем комбо-боксы на первый элемент, если он есть
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
            if (comboBox2.Items.Count > 0)
                comboBox2.SelectedIndex = 0;

            // Устанавливаем фокус на первое поле для удобства
            textBox1.Focus();
        }

        private void pictureBox7_Click(object sender, EventArgs e)
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

        private void ProductADD_Load(object sender, EventArgs e)
        {
            FillManufacturers();
            FillCategories();

            
        }

        private void FillManufacturers()
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT ManufacturerID, ManufacturerName FROM manufacturer";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        comboBox1.DataSource = dt;
                        comboBox1.DisplayMember = "ManufacturerName";
                        comboBox1.ValueMember = "ManufacturerID";
                    }
                }
            }
        }

        private void FillCategories()
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT CategoryID, CategoryName FROM category";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        comboBox2.DataSource = dt;
                        comboBox2.DisplayMember = "CategoryName";
                        comboBox2.ValueMember = "CategoryID";
                    }
                }
            }
        }
        //выбор фотографии
        private void button1_Click_1(object sender, EventArgs e)
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
                        Image previewImage = ResizeImage(selectedImage, pictureBox5.Width, pictureBox5.Height);

                        // Устанавливаем изображение в PictureBox
                        pictureBox5.Image = previewImage;
                        pictureBox5.BackColor = Color.White;

                        // Сохраняем путь к оригинальному файлу
                        selectedImagePath = filePath;
                        imageFileName = Path.GetFileName(filePath);

                        // Показываем информацию о выбранном файле
                        label12.Text = $"Выбрано: {imageFileName}\nРазмер: {fileInfo.Length / 1024} КБ";

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



        private void pictureBox5_Click(object sender, EventArgs e)
        {

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

        // Очистка ресурсов изображения при закрытии формы
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            // Освобождаем ресурсы изображения
            if (pictureBox5.Image != null)
            {
                pictureBox5.Image.Dispose();
                pictureBox5.Image = null;
            }
        }
    }
}
