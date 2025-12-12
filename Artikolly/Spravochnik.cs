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

namespace Artikolly
{
    public partial class Spravochnik : Form
    {
        public Spravochnik()
        {
            InitializeComponent();
            LoadCategories();
            LoadRoles();
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.DarkBlue;

            dataGridView2.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            dataGridView2.DefaultCellStyle.SelectionForeColor = Color.DarkBlue;

            // Изначально делаем кнопки удаления неактивными
            pictureBox9.Enabled = false; // Удаление категории
            pictureBox10.Enabled = false; // Удаление роли

        }
        private void LoadCategories()
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT CategoryName FROM category;";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView1.DataSource = dt;

                // Отключить авто-генерацию колонок ID, если она есть
                dataGridView1.Columns["CategoryName"].HeaderText = "Категория товаров";
            }
            pictureBox9.Enabled = false;
        }

        private void LoadRoles()
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT RoleName FROM role;";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView2.DataSource = dt;

                dataGridView2.Columns["RoleName"].HeaderText = "Роли";
            }
            pictureBox10.Enabled = false;
        }
    

    private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Manufacture authForm = new Manufacture();

            authForm.Show();
            this.Hide();
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            CategoryAdd catAdd = new CategoryAdd();
            // Подписываемся на событие закрытия формы
            catAdd.FormClosed += (s, args) => LoadCategories();
            catAdd.ShowDialog();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Пожалуйста, выберите категорию для редактирования.");
                return;
            }

            string selectedCategory = dataGridView1.CurrentRow.Cells["CategoryName"].Value.ToString();

            RedactCategory redactForm = new RedactCategory(selectedCategory);
            redactForm.FormClosed += (s, args) => LoadCategories();
            redactForm.ShowDialog();
        }

       

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            if (dataGridView2.CurrentRow == null)
            {
                MessageBox.Show("Пожалуйста, выберите роль для редактирования.");
                return;
            }

            string selectedRole = dataGridView2.CurrentRow.Cells["RoleName"].Value.ToString();

            RedactRoly redactForm = new RedactRoly(selectedRole);
            redactForm.FormClosed += (s, args) => LoadRoles();
            redactForm.ShowDialog();
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            RolyAdd RolyAd = new RolyAdd();
            RolyAd.FormClosed += (s, args) => LoadRoles();
            RolyAd.ShowDialog();
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Spravochnik_Load(object sender, EventArgs e)
        {

        }


        //удаление категории товаров
        private void pictureBox9_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Пожалуйста, выберите категорию для удаления.");
                return;
            }

            string selectedCategory = dataGridView1.CurrentRow.Cells["CategoryName"].Value?.ToString();

            if (string.IsNullOrEmpty(selectedCategory))
            {
                MessageBox.Show("Не удалось получить название категории.", "Ошибка");
                return;
            }

            try
            {
                // Проверяем наличие связанных товаров
                if (HasCategoryReferences(selectedCategory))
                {
                    MessageBox.Show("Невозможно удалить категорию, так как с ней связаны товары.",
                        "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Подтверждение удаления
                DialogResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить категорию:\n\"{selectedCategory}\"?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Удаляем категорию
                    if (DeleteCategory(selectedCategory))
                    {
                        MessageBox.Show("Категория успешно удалена!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Обновляем список и сбрасываем выбор
                        LoadCategories();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить категорию.", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении категории: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для проверки связей категории с товарами
        private bool HasCategoryReferences(string categoryName)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    // Получаем ID категории по имени
                    string categoryIdQuery = "SELECT CategoryID FROM category WHERE CategoryName = @categoryName";
                    int categoryId = -1;

                    using (var cmd = new MySqlCommand(categoryIdQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@categoryName", categoryName);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            categoryId = Convert.ToInt32(result);
                        }
                    }

                    if (categoryId == -1) return false;

                    // Проверяем наличие товаров в этой категории
                    string checkQuery = @"SELECT COUNT(*) FROM product 
                                        WHERE ProductCategory = @categoryId";

                    using (var cmd = new MySqlCommand(checkQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@categoryId", categoryId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке связей категории: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true; // В случае ошибки считаем что связи есть
            }
        }

        // Метод для удаления категории
        private bool DeleteCategory(string categoryName)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    string query = "DELETE FROM category WHERE CategoryName = @categoryName";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@categoryName", categoryName);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Если ошибка связана с внешними ключами
                if (ex.Number == 1451)
                {
                    MessageBox.Show("Невозможно удалить категорию, так как с ней связаны товары.",
                        "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show($"Ошибка при удалении категории: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        //удаление ролей
        private void pictureBox10_Click(object sender, EventArgs e)
        {
            if (dataGridView2.CurrentRow == null)
            {
                MessageBox.Show("Пожалуйста, выберите роль для удаления.");
                return;
            }

            string selectedRole = dataGridView2.CurrentRow.Cells["RoleName"].Value?.ToString();

            if (string.IsNullOrEmpty(selectedRole))
            {
                MessageBox.Show("Не удалось получить название роли.", "Ошибка");
                return;
            }

            try
            {
                // Проверяем наличие связанных пользователей
                if (HasRoleReferences(selectedRole))
                {
                    MessageBox.Show("Невозможно удалить роль, так как с ней связаны пользователи.",
                        "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Подтверждение удаления
                DialogResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить роль:\n\"{selectedRole}\"?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Удаляем роль
                    if (DeleteRole(selectedRole))
                    {
                        MessageBox.Show("Роль успешно удалена!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Обновляем список и сбрасываем выбор
                        LoadRoles();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить роль.", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении роли: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для проверки связей роли с пользователями
        private bool HasRoleReferences(string roleName)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    // Получаем ID роли по имени
                    string roleIdQuery = "SELECT RoleId FROM role WHERE RoleName = @roleName";
                    int roleId = -1;

                    using (var cmd = new MySqlCommand(roleIdQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@roleName", roleName);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            roleId = Convert.ToInt32(result);
                        }
                    }

                    if (roleId == -1) return false;

                    // Проверяем наличие пользователей с этой ролью
                    string checkQuery = @"SELECT COUNT(*) FROM user 
                                        WHERE Role = @roleId";

                    using (var cmd = new MySqlCommand(checkQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@roleId", roleId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке связей роли: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true; // В случае ошибки считаем что связи есть
            }
        }

        // Метод для удаления роли
        private bool DeleteRole(string roleName)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    string query = "DELETE FROM role WHERE RoleName = @roleName";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@roleName", roleName);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Если ошибка связана с внешними ключами
                if (ex.Number == 1451)
                {
                    MessageBox.Show("Невозможно удалить роль, так как с ней связаны пользователи.",
                        "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show($"Ошибка при удалении роли: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    

    private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count)
            {
                // Активируем кнопку удаления категории при выборе
                pictureBox9.Enabled = true;
            }
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView2.Rows.Count)
            {
                // Активируем кнопку удаления роли при выборе
                pictureBox10.Enabled = true;
            }
        }
    }
}
