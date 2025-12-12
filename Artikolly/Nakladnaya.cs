using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using Word = Microsoft.Office.Interop.Word;
using System.IO;
using MySql.Data.MySqlClient;

namespace Artikolly
{
    public partial class Nakladnaya : Form
    {
        private DataGridViewTextBoxColumn ColumnArticle;
        private DataGridViewTextBoxColumn ColumnName;
        private DataGridViewTextBoxColumn ColumnUnit;
        private DataGridViewTextBoxColumn ColumnQuantity;
        private DataGridViewTextBoxColumn ColumnPrice;
        private DataGridViewTextBoxColumn ColumnTotal;


        public Nakladnaya()
        {
            InitializeComponent();
            LoadProducts();
            LoadSuppliers();
            LoadAdmins(); 
            LoadTovarovedi();
            InitializeForm();
        }
        private void InitializeForm()
        {
            // Установка текущей даты
            dateTimePickerDate.Value = DateTime.Now;

            // Генерация номера документа
            textBoxDocNumber.Text = GenerateDocumentNumber();
            
        }
        // Загрузка администраторов из БД
        private void LoadAdmins()
        {
            try
            {
                using (MySqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                SELECT u.UserID, u.UserSurname, u.UserName, u.UserPatronomic 
                FROM user u
                INNER JOIN role r ON u.Role = r.RoleId
                WHERE r.RoleName = 'Администратор' 
                ORDER BY u.UserSurname, u.UserName";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    comboBoxAdmin.Items.Clear();
                    while (reader.Read())
                    {
                        string fullName = $"{reader["UserSurname"]} {reader["UserName"]} {reader["UserPatronomic"]}".Trim();
                        comboBoxAdmin.Items.Add(fullName);
                    }
                    reader.Close();

                    // Выбираем первый элемент, если есть
                    if (comboBoxAdmin.Items.Count > 0)
                        comboBoxAdmin.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке администраторов: {ex.Message}");
            }
        }

        // Загрузка товароведов из БД
        private void LoadTovarovedi()
        {
            try
            {
                using (MySqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                SELECT u.UserSurname, u.UserName, u.UserPatronomic 
                FROM user u
                INNER JOIN role r ON u.Role = r.RoleId
                WHERE r.RoleName = 'Товаровед' 
                ORDER BY u.UserSurname, u.UserName";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    comboBoxTovaroved.Items.Clear();
                    while (reader.Read())
                    {
                        string fullName = $"{reader["UserSurname"]} {reader["UserName"]} {reader["UserPatronomic"]}".Trim();
                        comboBoxTovaroved.Items.Add(fullName);
                    }
                    reader.Close();

                    // Выбираем первый элемент, если есть
                    if (comboBoxTovaroved.Items.Count > 0)
                        comboBoxTovaroved.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке товароведов: {ex.Message}");
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (MySqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT p.ProductArticleNumber, p.ProductName, p.ProductCost, 
                               p.Volume, m.ManufacturerName, c.CategoryName
                        FROM Product p
                        LEFT JOIN Manufacturer m ON p.ProductManufacture = m.ManufacturerID
                        LEFT JOIN Category c ON p.ProductCategory = c.CategoryID
                        WHERE p.ProductQuantilyStock > 0
                        ORDER BY p.ProductName";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    comboBoxProducts.Items.Clear();
                    while (reader.Read())
                    {
                        string productInfo = $"{reader["ProductArticleNumber"]} - {reader["ProductName"]} " +
                                           $"(Цена: {reader["ProductCost"]} руб., {reader["Volume"]})";
                        comboBoxProducts.Items.Add(productInfo);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке товаров: {ex.Message}");
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                using (MySqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT SuppliersID, SuppliersName FROM Suppliers ORDER BY SuppliersName";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    comboBoxSupplier.Items.Clear();
                    while (reader.Read())
                    {
                        string supplierName = $"{reader["SuppliersName"]}";
                        comboBoxSupplier.Items.Add(supplierName);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке поставщиков: {ex.Message}");
            }
        }

        private string GenerateDocumentNumber()
        {
            return $"ТН-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MenuTovaroved menu = new MenuTovaroved();

            menu.Show();
            this.Hide();
        }

        private void buttonGenerateDocument_Click(object sender, EventArgs e)
        {
            if (dataGridViewProducts.Rows.Count == 0)
            {
                MessageBox.Show("Добавьте товары в накладную");
                return;
            }

            if (comboBoxSupplier.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика");
                return;
            }

            if (string.IsNullOrEmpty(textBoxReceiver.Text))
            {
                MessageBox.Show("Укажите грузополучателя");
                return;
            }

            if (comboBoxAdmin.SelectedItem == null)
            {
                MessageBox.Show("Выберите администратора");
                return;
            }

            if (comboBoxTovaroved.SelectedItem == null)
            {
                MessageBox.Show("Выберите товароведа");
                return;
            }

            try
            {
                CreateWordDocument();
                MessageBox.Show("Накладная успешно создана!");
                ClearAllFields(); // Очищаем все поля после успешного создания
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании документа: {ex.Message}");
            }
        }

        private void CreateWordDocument()
        {
            Word.Application wordApp = new Word.Application();
            Word.Document wordDoc = wordApp.Documents.Add();

            try
            {
                wordApp.Visible = true;

                // Установка шрифта Times New Roman 14pt для всего документа
                wordDoc.Content.Font.Name = "Times New Roman";
                wordDoc.Content.Font.Size = 14;

                // Заголовок документа
                Word.Paragraph title = wordDoc.Paragraphs.Add();
                title.Range.Text = "ТОВАРНАЯ НАКЛАДНАЯ (форма № ТОРГ-12)";
                title.Range.Font.Bold = 1;
                title.Range.Font.Size = 12;
                title.Format.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                title.Range.InsertParagraphAfter();

                // Информация о документе
                Word.Paragraph docInfo = wordDoc.Paragraphs.Add();
                docInfo.Range.Text = $"Номер документа: {textBoxDocNumber.Text}\tДата составления: {dateTimePickerDate.Value:dd.MM.yyyy}";
                docInfo.Range.InsertParagraphAfter();

                // Информация о поставщике
                Word.Paragraph supplierInfo = wordDoc.Paragraphs.Add();
                supplierInfo.Range.Text = $"Поставщик: {comboBoxSupplier.SelectedItem}";
                supplierInfo.Range.InsertParagraphAfter();

                // Информация о грузополучателе
                Word.Paragraph receiverInfo = wordDoc.Paragraphs.Add();
                receiverInfo.Range.Text = $"Грузополучатель: {textBoxReceiver.Text}";
                receiverInfo.Range.InsertParagraphAfter();

                // Пустая строка
                wordDoc.Paragraphs.Add().Range.InsertParagraphAfter();

                // Создание таблицы с товарами
                Word.Table table = wordDoc.Tables.Add(
                    wordDoc.Paragraphs.Add().Range,
                    dataGridViewProducts.Rows.Count + 1,
                    6
                );

                // Настройка таблицы - отключаем перенос слов в шапке
                table.AllowAutoFit = true;
                table.AutoFitBehavior(Word.WdAutoFitBehavior.wdAutoFitWindow);

                // Заголовки таблицы (без переносов)
                table.Cell(1, 1).Range.Text = "№";
                table.Cell(1, 2).Range.Text = "Наименование товара";
                table.Cell(1, 3).Range.Text = "Ед. изм.";
                table.Cell(1, 4).Range.Text = "Количество";
                table.Cell(1, 5).Range.Text = "Цена, руб.";
                table.Cell(1, 6).Range.Text = "Сумма, руб.";

                // Форматирование шапки таблицы - отключаем перенос слов
                for (int i = 1; i <= 6; i++)
                {
                    table.Cell(1, i).Range.Font.Bold = 1;
                    table.Cell(1, i).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    table.Cell(1, i).Range.NoProofing = 1; // Отключаем проверку орфографии для шапки
                    table.Cell(1, i).Range.ParagraphFormat.WordWrap = 0; // Отключаем перенос слов
                }

                // Устанавливаем ширину столбцов для предотвращения переноса
                table.Columns[1].Width = wordApp.CentimetersToPoints(1.5f);  // №
                table.Columns[2].Width = wordApp.CentimetersToPoints(6f);    // Наименование
                table.Columns[3].Width = wordApp.CentimetersToPoints(2f);    // Ед. изм.
                table.Columns[4].Width = wordApp.CentimetersToPoints(2f);    // Количество
                table.Columns[5].Width = wordApp.CentimetersToPoints(2.5f);  // Цена
                table.Columns[6].Width = wordApp.CentimetersToPoints(2.5f);  // Сумма

                // Заполнение таблицы данными
                for (int i = 0; i < dataGridViewProducts.Rows.Count; i++)
                {
                    table.Cell(i + 2, 1).Range.Text = (i + 1).ToString();
                    table.Cell(i + 2, 2).Range.Text = dataGridViewProducts.Rows[i].Cells["ColumnName"].Value?.ToString() ?? "";
                    table.Cell(i + 2, 3).Range.Text = dataGridViewProducts.Rows[i].Cells["ColumnUnit"].Value?.ToString() ?? "";
                    table.Cell(i + 2, 4).Range.Text = dataGridViewProducts.Rows[i].Cells["ColumnQuantity"].Value?.ToString() ?? "";
                    table.Cell(i + 2, 5).Range.Text = dataGridViewProducts.Rows[i].Cells["ColumnPrice"].Value?.ToString() ?? "";
                    table.Cell(i + 2, 6).Range.Text = dataGridViewProducts.Rows[i].Cells["ColumnTotal"].Value?.ToString() ?? "";

                    // Выравнивание для числовых колонок
                    table.Cell(i + 2, 1).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    table.Cell(i + 2, 4).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                    table.Cell(i + 2, 5).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                    table.Cell(i + 2, 6).Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                }

                // Форматирование таблицы
                table.Borders.Enable = 1;


                // Пустая строка после таблицы
                wordDoc.Paragraphs.Add().Range.InsertParagraphAfter();

                // Итоговая сумма
                Word.Paragraph totalParagraph = wordDoc.Paragraphs.Add();
                totalParagraph.Range.Text = $"Всего к оплате: {labelTotal.Text}";
                totalParagraph.Range.Font.Bold = 1;
                totalParagraph.Range.Font.Size = 14;

                // Подписи - ИСПРАВЛЕНО: удалены Split('-')[1], используем полное имя
                Word.Paragraph signatures = wordDoc.Paragraphs.Add();

                // Просто берем полное имя без разделения по дефису
                string adminName = comboBoxAdmin.SelectedItem?.ToString() ?? "";
                string tovarovedName = comboBoxTovaroved.SelectedItem?.ToString() ?? "";

                signatures.Range.Text = $"\n\nОтпуск разрешил: _________________ / {adminName} /";
                signatures.Range.InsertParagraphAfter();

                signatures.Range.Text = $"Главный бухгалтер: _________________ / {tovarovedName} /";
                signatures.Range.InsertParagraphAfter();

                signatures.Range.Text = $"\nДата: {DateTime.Now:dd.MM.yyyy}";

                // Сохранение документа
                string fileName = $"Накладная_{textBoxDocNumber.Text}_{DateTime.Now:yyyyMMddHHmmss}.docx";
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

                wordDoc.SaveAs2(filePath);

                MessageBox.Show($"Документ сохранен на рабочем столе: {fileName}", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании Word документа: {ex.Message}\n\nДетали: {ex.StackTrace}",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Не закрываем Word, чтобы пользователь мог увидеть документ
                // wordDoc.Close();
                // wordApp.Quit();
            }
        }

        private void buttonAddProduct_Click(object sender, EventArgs e)
        {
            if (comboBoxProducts.SelectedItem == null || numericUpDownQuantity.Value <= 0)
            {
                MessageBox.Show("Выберите товар и укажите количество");
                return;
            }

            try
            {
                string selectedProduct = comboBoxProducts.SelectedItem.ToString();
                string articleNumber = selectedProduct.Split('-')[0].Trim();
                int quantity = (int)numericUpDownQuantity.Value;

                using (MySqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT p.ProductArticleNumber, p.ProductName, p.ProductCost, 
                               p.Volume, m.ManufacturerName, p.ProductDescription
                        FROM Product p
                        LEFT JOIN Manufacturer m ON p.ProductManufacture = m.ManufacturerID
                        WHERE p.ProductArticleNumber = @ArticleNumber";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ArticleNumber", articleNumber);

                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        decimal price = Convert.ToDecimal(reader["ProductCost"]);
                        decimal total = price * quantity;

                        dataGridViewProducts.Rows.Add(
                            articleNumber,
                            reader["ProductName"],
                            reader["Volume"],
                            quantity,
                            price,
                            total
                        );

                        CalculateTotal();
                        ClearProductSelection();
                    }
                    reader.Close();
                }

                numericUpDownQuantity.Value = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении товара: {ex.Message}");
            }
        }

        private void ClearProductSelection()
        {
            comboBoxProducts.SelectedIndex = -1;
            numericUpDownQuantity.Value = 1;
        }
        private void ClearAllFields()
        {
            // Генерация нового номера документа
            textBoxDocNumber.Text = GenerateDocumentNumber();

            // Сброс даты на текущую
            dateTimePickerDate.Value = DateTime.Now;

            // Очистка текстовых полей
            textBoxReceiver.Text = "";

            // Сброс выбора в комбобоксах
            if (comboBoxSupplier.Items.Count > 0)
                comboBoxSupplier.SelectedIndex = -1;

            if (comboBoxAdmin.Items.Count > 0)
                comboBoxAdmin.SelectedIndex = 0; // Оставляем выбранным первый элемент

            if (comboBoxTovaroved.Items.Count > 0)
                comboBoxTovaroved.SelectedIndex = 0; // Оставляем выбранным первый элемент

            // Очистка выбора товара
            comboBoxProducts.SelectedIndex = -1;
            numericUpDownQuantity.Value = 1;

            // Очистка таблицы с товарами
            dataGridViewProducts.Rows.Clear();

            // Сброс итоговой суммы
            CalculateTotal();

            // Устанавливаем фокус на первое поле для удобства
            textBoxReceiver.Focus();
        }
        private void CalculateTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dataGridViewProducts.Rows)
            {
                if (row.Cells["ColumnTotal"].Value != null)
                {
                    total += Convert.ToDecimal(row.Cells["ColumnTotal"].Value);
                }
            }
            labelTotal.Text = $"Итого: {total:C}";
        }

        private void buttonRemoveProduct_Click(object sender, EventArgs e)
        {
            if (dataGridViewProducts.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridViewProducts.SelectedRows)
                {
                    dataGridViewProducts.Rows.Remove(row);
                }
                CalculateTotal();
            }
        }

        private void dataGridViewProducts_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Пересчет при изменении количества или цены
            if (e.ColumnIndex == 3 || e.ColumnIndex == 4) // ColumnQuantity или ColumnPrice
            {
                DataGridViewRow row = dataGridViewProducts.Rows[e.RowIndex];
                if (row.Cells["ColumnQuantity"].Value != null && row.Cells["ColumnPrice"].Value != null)
                {
                    try
                    {
                        int quantity = Convert.ToInt32(row.Cells["ColumnQuantity"].Value);
                        decimal price = Convert.ToDecimal(row.Cells["ColumnPrice"].Value);
                        row.Cells["ColumnTotal"].Value = quantity * price;
                        CalculateTotal();
                    }
                    catch (FormatException)
                    {
                        MessageBox.Show("Пожалуйста, введите корректные числовые значения");
                    }
                }
            }
        }
    }
}
