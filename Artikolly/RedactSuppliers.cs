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
    public partial class RedactSuppliers : Form
    {
        public event EventHandler SupplierChanged;
        private int suppliersId;
        public RedactSuppliers(int supplierId)
        {
            InitializeComponent();
            suppliersId = supplierId;
            LoadSupplierData();
        }
        private void LoadSupplierData()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT SuppliersName, ContactInformation FROM suppliers WHERE SuppliersID = @id";
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", suppliersId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                textBox5.Text = reader["SuppliersName"].ToString();
                                textBox6.Text = reader["ContactInformation"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
                this.Close();
            }
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string newName = textBox5.Text.Trim();
            string contactInfo = textBox6.Text.Trim();

            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Название поставщика не может быть пустым.", "Предупреждение");
                return;
            }

            // Проверка на дублирование названия (кроме текущего)
            if (IsDuplicateSuppliersName(newName))
            {
                MessageBox.Show("Поставщик с таким названием уже существует.", "Предупреждение");
                return;
            }

            // Обновление данных
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string updateQuery = "UPDATE suppliers SET SuppliersName = @name, ContactInformation = @contact WHERE SuppliersID = @id";
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", newName);
                        cmd.Parameters.AddWithValue("@contact", contactInfo);
                        cmd.Parameters.AddWithValue("@id", suppliersId);
                        cmd.ExecuteNonQuery();
                    }
                }

                SupplierChanged?.Invoke(this, EventArgs.Empty);
                MessageBox.Show("Данные поставщика успешно обновлены.", "Успех");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении: {ex.Message}", "Ошибка");
            }
        }

        private bool IsDuplicateSuppliersName(string name)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM suppliers WHERE SuppliersName = @name AND SuppliersID != @id";
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@id", suppliersId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch
            {
                // В случае ошибки считаем, что дублирование есть чтобы не позволить ошибочные обновления
                return true;
            }
        }
    

    private void RedactSuppliers_Load(object sender, EventArgs e)
        {

        }
    }
}
