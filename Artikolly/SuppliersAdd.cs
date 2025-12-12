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
    public partial class SuppliersAdd : Form
    {
        public event EventHandler SupplierChanged;
        public SuppliersAdd()
        {
            InitializeComponent();
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string supplierName = textBox5.Text.Trim();
            string contactInfo = textBox6.Text.Trim();

            if (string.IsNullOrEmpty(supplierName))
            {
                MessageBox.Show("Пожалуйста, введите название поставщика.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = "INSERT INTO suppliers (SuppliersName, ContactInformation) VALUES (@name, @contact)";
                    using (var cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", supplierName);
                        cmd.Parameters.AddWithValue("@contact", contactInfo);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Вызов события, чтобы оповестить главную форму о изменениях
                SupplierChanged?.Invoke(this, EventArgs.Empty);
                MessageBox.Show("Поставщик успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close(); // закрываем форму после добавления
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении поставщика: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
           
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void SuppliersAdd_Load(object sender, EventArgs e)
        {

        }
    }
}
