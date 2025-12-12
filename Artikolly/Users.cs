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
    public partial class Users : Form
    {
        private const string DefaultQuery = @"SELECT 
                        u.UserID AS ID,
                        u.UserSurname AS Фамилия,
                        u.UserName AS Имя,
                        u.UserPatronomic AS Отчество,
                        u.DateOfBirth AS 'Дата Рождения',
                        u.Phone AS Телефон,
                        r.RoleName AS Роль
                     FROM user u
                     LEFT JOIN role r ON u.Role = r.RoleId";
        public Users()
        {
            InitializeComponent();
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.DarkBlue;
            button1.Enabled = false; // Редактировать
            button3.Enabled = false; // Удалить
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UsersADD formAdd = new UsersADD();
            formAdd.UserChanged += (s, args) => FillDataGrid();
            formAdd.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            administrator authForm = new administrator();

            authForm.Show();
            this.Hide();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Users_Load(object sender, EventArgs e)
        {
            string query = @"SELECT 
                        u.UserID AS ID,
                        u.UserSurname AS Фамилия,
                        u.UserName AS Имя,
                        u.UserPatronomic AS Отчество,
                        u.DateOfBirth AS 'Дата Рождения',
                        u.Phone AS Телефон,
                        r.RoleName AS Роль
                     FROM user u
                     LEFT JOIN role r ON u.Role = r.RoleId";

            FillDataGrid(query);
        }

        void FillDataGrid()
        {
            FillDataGrid(DefaultQuery);
        }
        void FillDataGrid(string CMD)
        {
            try
            {
                dataGridView1.Columns.Clear();
                dataGridView1.Rows.Clear();

                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand(CMD, conn);
                    using (MySqlDataReader RDR = cmd.ExecuteReader())
                    {
                        // Создаем колонки с единообразными именами
                        dataGridView1.Columns.Add("ID", "ID");
                        dataGridView1.Columns.Add("Фамилия", "Фамилия");
                        dataGridView1.Columns.Add("Имя", "Имя");
                        dataGridView1.Columns.Add("Отчество", "Отчество");
                        dataGridView1.Columns.Add("ДатаРождения", "Дата Рождения");
                        dataGridView1.Columns.Add("Телефон", "Телефон");
                        dataGridView1.Columns.Add("Роль", "Роль");
                        dataGridView1.Columns["ID"].Visible = false;

                        // Заполняем данными
                        while (RDR.Read())
                        {
                            dataGridView1.Rows.Add(
                                RDR["ID"].ToString(),
                                RDR["Фамилия"].ToString(),
                                RDR["Имя"].ToString(),
                                RDR["Отчество"].ToString(),
                                RDR["Дата Рождения"].ToString(),
                                RDR["Телефон"].ToString(),
                                RDR["Роль"].ToString()
                            );
                        }
                    }
                }
                selectedUserId = -1;
                button1.Enabled = false;
                button3.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBox1.Text.Trim();

            string query = @"SELECT 
                        u.UserID AS ID,
                        u.UserSurname AS Фамилия,
                        u.UserName AS Имя,
                        u.UserPatronomic AS Отчество,
                        u.DateOfBirth AS 'Дата Рождения',
                        u.Phone AS Телефон,
                        r.RoleName AS Роль
                     FROM user u
                     LEFT JOIN role r ON u.Role = r.RoleId";

            if (!string.IsNullOrEmpty(searchText))
            {
                query += " WHERE u.UserSurname LIKE @search";
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        cmd.Parameters.AddWithValue("@search", "%" + searchText + "%");
                    }

                    using (MySqlDataReader RDR = cmd.ExecuteReader())
                    {
                        // Очищаем данные перед заполнением
                        dataGridView1.Rows.Clear();

                        while (RDR.Read())
                        {
                            dataGridView1.Rows.Add(
                                RDR["ID"].ToString(),
                                RDR["Фамилия"].ToString(),
                                RDR["Имя"].ToString(),
                                RDR["Отчество"].ToString(),
                                RDR["Дата Рождения"].ToString(),
                                RDR["Телефон"].ToString(),
                                RDR["Роль"].ToString()
                            );
                        }
                    }
                }
                selectedUserId = -1;
                button1.Enabled = false;
                button3.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //Редактировать
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (selectedUserId != -1)
            {
                using (var form = new RedactUsers(selectedUserId))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        FillDataGrid();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для редактирования", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private int selectedUserId = -1;
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count)
            {
                // Безопасное получение значения ID
                var idCell = dataGridView1.Rows[e.RowIndex].Cells["ID"];
                if (idCell != null && idCell.Value != null)
                {
                    if (int.TryParse(idCell.Value.ToString(), out int userId))
                    {
                        selectedUserId = userId;
                        button1.Enabled = true; // Редактировать
                        button3.Enabled = true; // Удалить
                    }
                    else
                    {
                        selectedUserId = -1;
                        button1.Enabled = false;
                        button3.Enabled = false;
                    }
                }
                else
                {
                    selectedUserId = -1;
                    button1.Enabled = false;
                    button3.Enabled = false;
                }
            }
        }


        //удалить
        private void button3_Click(object sender, EventArgs e)
        {
            if (selectedUserId == -1)
            {
                MessageBox.Show("Выберите пользователя для удаления", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Получаем информацию о выбранном пользователе
                string selectedUserName = "";
                string selectedUserRole = "";
                bool isCurrentUser = false;

                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Получаем данные выбранного пользователя
                    string userQuery = @"SELECT 
                        CONCAT(u.UserSurname, ' ', u.UserName, ' ', u.UserPatronomic) as FullName,
                        r.RoleName,
                        u.Login
                     FROM user u
                     LEFT JOIN role r ON u.Role = r.RoleId
                     WHERE u.UserID = @userId";

                    MySqlCommand userCmd = new MySqlCommand(userQuery, conn);
                    userCmd.Parameters.AddWithValue("@userId", selectedUserId);

                    using (var reader = userCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            selectedUserName = reader["FullName"].ToString();
                            selectedUserRole = reader["RoleName"].ToString();
                        }
                    }

                    // Проверяем, является ли выбранный пользователь текущим
                    string currentUserQuery = "SELECT Login FROM user WHERE UserID = @userId";
                    MySqlCommand currentUserCmd = new MySqlCommand(currentUserQuery, conn);
                    currentUserCmd.Parameters.AddWithValue("@userId", selectedUserId);

                    string selectedUserLogin = currentUserCmd.ExecuteScalar()?.ToString();
                    isCurrentUser = (selectedUserId.ToString() == CurrentUser.UserID); // Или другое поле, которое идентифицирует текущего пользователя
                }

                // Проверка 1: Нельзя удалить самого себя
                if (isCurrentUser)
                {
                    MessageBox.Show("Нельзя удалить текущего пользователя!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверка 2: Проверяем связи с другими таблицами
                if (HasUserReferences(selectedUserId))
                {
                    MessageBox.Show("Невозможно удалить пользователя, так как он связан с другими данными в системе.",
                        "Ошибка удаления", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Подтверждение удаления
                DialogResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить пользователя:\n{selectedUserName}?\nРоль: {selectedUserRole}",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Удаляем пользователя
                    if (DeleteUser(selectedUserId))
                    {
                        MessageBox.Show("Пользователь успешно удален!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Обновляем список и сбрасываем выбор
                        FillDataGrid();

                        // Деактивируем кнопки
                        button1.Enabled = false;
                        button3.Enabled = false;
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить пользователя.", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Метод для проверки связей пользователя с другими таблицами
        private bool HasUserReferences(int userId)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    // Проверяем наличие пользователя в таблице заказов (если такая есть)
                    // Замените 'order' на актуальное название таблицы заказов в вашей БД
                    string query = @"SELECT COUNT(*) FROM `order` WHERE OrderUser = @userId";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Если таблицы не существует или другая ошибка, считаем что связи есть (безопасный подход)
                MessageBox.Show($"Ошибка при проверке связей: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
        }

        // Метод для удаления пользователя
        private bool DeleteUser(int userId)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    string query = "DELETE FROM user WHERE UserID = @userId";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Если ошибка связана с внешними ключами
                if (ex.Number == 1451) // MySQL error code for foreign key constraint
                {
                    MessageBox.Show("Невозможно удалить пользователя, так как он связан с другими данными в системе.",
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
                MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
