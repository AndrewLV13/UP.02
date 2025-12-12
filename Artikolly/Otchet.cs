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
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;

namespace Artikolly
{
    public partial class Otchet : Form
    {
        public Otchet()
        {
            InitializeComponent();
            dateTimePicker2.MinDate = dateTimePicker1.Value;
            //dateTimePicker1.Value = DateTime.Today.AddDays(-30);
            //dateTimePicker2.Value = DateTime.Today;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MenuTovaroved menu = new MenuTovaroved();

            menu.Show();
            this.Hide();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            DateTime currentDate = DateTime.Today;
            DateTime minDate = new DateTime(2025, 1, 1);

            if (dateTimePicker1.Value > currentDate)
            {
                // Если дата больше текущей - устанавливаем текущую дату
                MessageBox.Show("Нельзя выбрать дату больше текущей!", "Предупреждение",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dateTimePicker1.Value = currentDate;
            }
            else if (dateTimePicker1.Value < minDate)
            {
                // Если дата меньше 2020 года - устанавливаем 01.01.2020
                MessageBox.Show("Нельзя выбрать дату раньше 2020 года!", "Предупреждение",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dateTimePicker1.Value = minDate;
            }
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            DateTime currentDate = DateTime.Today;
            DateTime minDate = new DateTime(2020, 1, 1);

            if (dateTimePicker2.Value > currentDate)
            {
                // Если дата больше текущей - устанавливаем текущую дату
                MessageBox.Show("Нельзя выбрать дату больше текущей!", "Предупреждение",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dateTimePicker1.Value = currentDate;
            }
            else if (dateTimePicker2.Value < minDate)
            {
                // Если дата меньше 2020 года - устанавливаем 01.01.2020
                MessageBox.Show("Нельзя выбрать дату раньше 2020 года!", "Предупреждение",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dateTimePicker1.Value = minDate;
            }
        }

        private void Otchet_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                GenerateStockReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void GenerateStockReport()
        {
            // Получаем данные об остатках
            DataTable stockData = GetStockData();

            if (stockData.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для формирования отчета", "Информация",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Создаем Excel приложение
            Excel.Application excelApp = new Excel.Application();
            excelApp.Visible = true;

            // Добавляем рабочую книгу
            Excel.Workbook workbook = excelApp.Workbooks.Add();

            // Первый лист - данные
            Excel.Worksheet dataWorksheet = workbook.ActiveSheet;
            dataWorksheet.Name = "Данные об остатках";

            // Заголовок отчета
            dataWorksheet.Cells[1, 1] = "Отчет об остатках товаров на складе";
            dataWorksheet.Cells[2, 1] = $"Период: с {dateTimePicker1.Value:dd.MM.yyyy} по {dateTimePicker2.Value:dd.MM.yyyy}";
            dataWorksheet.Cells[3, 1] = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}";
            // Заголовок отчета
            dataWorksheet.Cells[1, 1] = "Отчет об остатках товаров на складе";
            dataWorksheet.Cells[2, 1] = $"Период: с {dateTimePicker1.Value:dd.MM.yyyy} по {dateTimePicker2.Value:dd.MM.yyyy}";
            dataWorksheet.Cells[3, 1] = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}";

            // Форматирование заголовка
            Excel.Range titleRange = dataWorksheet.Range["A1:E1"];
            titleRange.Merge(); // Объединяем ячейки для заголовка
            titleRange.Font.Bold = true;
            titleRange.Font.Size = 16;
            titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            titleRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

            // Форматирование подзаголовков
            Excel.Range subtitleRange1 = dataWorksheet.Range["A2:E2"];
            subtitleRange1.Merge();
            subtitleRange1.Font.Bold = true;
            subtitleRange1.Font.Size = 12;
            subtitleRange1.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

            Excel.Range subtitleRange2 = dataWorksheet.Range["A3:E3"];
            subtitleRange2.Merge();
            subtitleRange2.Font.Bold = true;
            subtitleRange2.Font.Size = 12;
            subtitleRange2.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

            // Заголовки таблицы
            dataWorksheet.Cells[5, 1] = "Артикул";
            dataWorksheet.Cells[5, 2] = "Наименование товара";
            dataWorksheet.Cells[5, 3] = "Количество на складе";
            dataWorksheet.Cells[5, 4] = "Категория";
            dataWorksheet.Cells[5, 5] = "Производитель";

            // Заполняем данными
            int row = 6;
            foreach (DataRow dataRow in stockData.Rows)
            {
                dataWorksheet.Cells[row, 1] = dataRow["ProductArticleNumber"].ToString();
                dataWorksheet.Cells[row, 2] = dataRow["ProductName"].ToString();
                dataWorksheet.Cells[row, 3] = Convert.ToInt32(dataRow["ProductQuantilyStock"]);
                dataWorksheet.Cells[row, 4] = dataRow["CategoryName"].ToString();
                dataWorksheet.Cells[row, 5] = dataRow["ManufacturerName"].ToString();
                row++;
            }

            // Форматируем таблицу
            Excel.Range headerRange = dataWorksheet.Range["A5:E5"];
            headerRange.Font.Bold = true;
            headerRange.Interior.Color = Color.LightBlue;

            // Добавляем границы для всей таблицы
            int lastRow = 5 + stockData.Rows.Count;
            Excel.Range tableRange = dataWorksheet.Range[$"A5:E{lastRow}"];
            tableRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            tableRange.Borders.Weight = Excel.XlBorderWeight.xlThin;

            // Авто-подбор ширины столбцов
            dataWorksheet.Columns.AutoFit();

            // Создаем второй лист для диаграммы
            Excel.Worksheet chartWorksheet = workbook.Worksheets.Add();
            chartWorksheet.Name = "Диаграмма остатков";

            // Создаем диаграмму на втором листе
            CreateChart(dataWorksheet, chartWorksheet, stockData.Rows.Count);

            // Активируем лист с данными для удобства пользователя
            dataWorksheet.Activate();

            // Сохраняем файл
            string fileName = $"Остатки_товаров_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

            workbook.SaveAs(filePath);
            MessageBox.Show($"Отчет успешно сохранен: {filePath}", "Успех",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Закрываем Excel
            workbook.Close();
            excelApp.Quit();
        }

        private DataTable GetStockData()
        {
            DataTable dataTable = new DataTable();

            using (MySqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string query = @"
                    SELECT 
                        p.ProductArticleNumber,
                        p.ProductName,
                        p.ProductQuantilyStock,
                        c.CategoryName,
                        m.ManufacturerName
                    FROM product p
                    INNER JOIN category c ON p.ProductCategory = c.CategoryID
                    INNER JOIN manufacturer m ON p.ProductManufacture = m.ManufacturerID
                    WHERE p.ProductQuantilyStock > 0
                    ORDER BY p.ProductQuantilyStock DESC";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }

            return dataTable;
        }

        private void CreateChart(Excel.Worksheet dataWorksheet, Excel.Worksheet chartWorksheet, int dataRowsCount)
        {
            // Определяем диапазон данных для диаграммы
            int lastRow = 5 + dataRowsCount;

            // Создаем объект диаграммы на втором листе
            Excel.ChartObjects chartObjects = (Excel.ChartObjects)chartWorksheet.ChartObjects();
            Excel.ChartObject chartObject = chartObjects.Add(50, 50, 600, 400);
            Excel.Chart chart = chartObject.Chart;

            // Устанавливаем данные для диаграммы (берем с первого листа)
            Excel.Range categoryRange = dataWorksheet.Range[$"B6:B{lastRow}"];
            Excel.Range valueRange = dataWorksheet.Range[$"C6:C{lastRow}"];

            chart.ChartType = Excel.XlChartType.xlColumnClustered;
            chart.SetSourceData(valueRange, Excel.XlRowCol.xlColumns);

            // Настраиваем серии
            Excel.Series series = chart.SeriesCollection(1);
            series.XValues = categoryRange;
            series.Name = "Остатки товаров";

            // Настраиваем заголовок и оси
            chart.HasTitle = true;
            chart.ChartTitle.Text = "Остатки товаров на складе";

            chart.Axes(Excel.XlAxisType.xlCategory).HasTitle = true;
            chart.Axes(Excel.XlAxisType.xlCategory).AxisTitle.Text = "Товары";

            chart.Axes(Excel.XlAxisType.xlValue).HasTitle = true;
            chart.Axes(Excel.XlAxisType.xlValue).AxisTitle.Text = "Количество";

            // Поворачиваем подписи по оси X для лучшей читаемости
            chart.Axes(Excel.XlAxisType.xlCategory).TickLabels.Orientation = 45;

            // Добавляем заголовок на лист с диаграммой
            chartWorksheet.Cells[1, 1] = "Диаграмма остатков товаров на складе";
            chartWorksheet.Cells[1, 1].Font.Bold = true;
            chartWorksheet.Cells[1, 1].Font.Size = 14;

            chartWorksheet.Cells[2, 1] = $"Период: с {dateTimePicker1.Value:dd.MM.yyyy} по {dateTimePicker2.Value:dd.MM.yyyy}";
            chartWorksheet.Cells[3, 1] = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}";

            // Авто-подбор ширины столбцов для заголовков
            chartWorksheet.Columns.AutoFit();
        }
    }

}

