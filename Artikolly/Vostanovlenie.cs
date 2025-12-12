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
    public partial class Vostanovlenie : Form
    {
        public Vostanovlenie()
        {
            InitializeComponent();
            LoadTableNames();
        }
        private void LoadTableNames()
        {
            try
            {
                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "SHOW TABLES";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        comboBox1.Items.Clear();
                        while (reader.Read())
                        {
                            comboBox1.Items.Add(reader[0].ToString());
                        }
                    }
                    if (comboBox1.Items.Count > 0)
                        comboBox1.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке списка таблиц: " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Выбор CSV файла
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.Title = "Выберите файл CSV для импорта";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog.FileName;
                PreviewCSVData(openFileDialog.FileName);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Импорт данных из CSV
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Сначала выберите файл CSV", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Выберите таблицу для импорта", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string tableName = comboBox1.SelectedItem.ToString();
            string filePath = textBox1.Text;

            try
            {
                ImportCSVData(tableName, filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PreviewCSVData(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

                if (lines.Length > 0)
                {
                    string[] headers = lines[0].Split(';');
                    int totalRows = lines.Length - 1;
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при чтении CSV файла: " + ex.Message);
            }
        }

        private void ImportCSVData(string tableName, string filePath)
        {
            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

            if (lines.Length < 2)
            {
                MessageBox.Show("Файл CSV пуст или содержит только заголовки", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string[] headers = lines[0].Split(';');

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // Получаем информацию о колонках таблицы
                MySqlCommand cmd = new MySqlCommand($"DESCRIBE {tableName}", conn);
                var tableColumns = new List<string>();
                var columnTypes = new Dictionary<string, string>();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string columnName = reader["Field"].ToString();
                        string columnType = reader["Type"].ToString();
                        tableColumns.Add(columnName);
                        columnTypes[columnName] = columnType;
                    }
                }

                // Проверяем соответствие колонок
                if (headers.Length != tableColumns.Count)
                {
                    MessageBox.Show($"Несоответствие количества колонок!\n" +
                                  $"В CSV: {headers.Length} колонок\n" +
                                  $"В таблице {tableName}: {tableColumns.Count} колонок",
                                  "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Начинаем транзакцию
                MySqlTransaction transaction = conn.BeginTransaction();
                int importedRows = 0;

                try
                {
                    // Пропускаем заголовок и обрабатываем данные
                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i]))
                            continue;

                        string[] values = lines[i].Split(';');

                        if (values.Length != headers.Length)
                        {
                            throw new Exception($"Строка {i}: несоответствие количества значений");
                        }

                        // Формируем SQL запрос
                        StringBuilder sqlBuilder = new StringBuilder();
                        sqlBuilder.Append($"INSERT INTO {tableName} (");
                        sqlBuilder.Append(string.Join(", ", headers));
                        sqlBuilder.Append(") VALUES (");

                        for (int j = 0; j < values.Length; j++)
                        {
                            sqlBuilder.Append($"@param{j}");
                            if (j < values.Length - 1) sqlBuilder.Append(", ");
                        }
                        sqlBuilder.Append(")");

                        // Создаем команду с параметрами
                        MySqlCommand insertCmd = new MySqlCommand(sqlBuilder.ToString(), conn, transaction);

                        for (int j = 0; j < values.Length; j++)
                        {
                            string paramName = $"@param{j}";
                            string value = values[j];

                            // Обрабатываем NULL значения
                            if (value == "NULL" || string.IsNullOrEmpty(value))
                            {
                                insertCmd.Parameters.AddWithValue(paramName, DBNull.Value);
                            }
                            else
                            {
                                // Преобразуем типы данных
                                string columnType = columnTypes[headers[j]];
                                if (columnType.Contains("int") || columnType.Contains("decimal"))
                                {
                                    if (decimal.TryParse(value, out decimal numericValue))
                                        insertCmd.Parameters.AddWithValue(paramName, numericValue);
                                    else
                                        insertCmd.Parameters.AddWithValue(paramName, value);
                                }
                                else if (columnType.Contains("date"))
                                {
                                    if (DateTime.TryParse(value, out DateTime dateValue))
                                        insertCmd.Parameters.AddWithValue(paramName, dateValue);
                                    else
                                        insertCmd.Parameters.AddWithValue(paramName, value);
                                }
                                else
                                {
                                    insertCmd.Parameters.AddWithValue(paramName, value);
                                }
                            }
                        }

                        insertCmd.ExecuteNonQuery();
                        importedRows++;
                    }

                    transaction.Commit();
                    MessageBox.Show($"Импорт завершен успешно!\nИмпортировано записей: {importedRows}",
                                  "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Очищаем форму
                    textBox1.Clear();
                    LoadTableNames(); 
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception($"Ошибка при импорте строки {importedRows + 1}: {ex.Message}");
                }
            }
        }

        private void Vostanovlenie_Load(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Import imp = new Import();
            this.Hide();
            imp.ShowDialog();
        }
    }
}
