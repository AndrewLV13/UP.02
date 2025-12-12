using System;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Artikolly
{
    public partial class Authorization : Form
    {
        private bool isPasswordVisible = false;
        public Authorization()
        {
            InitializeComponent();
            textBox2.UseSystemPasswordChar = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text.Trim();
            string password = textBox2.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль");
                return;
            }

            string hashedPassword = HashPassword(password);

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = @"
                SELECT u.UserID, u.UserSurname, u.UserName, u.UserPatronomic, u.Role, r.RoleName
                FROM `user` u
                INNER JOIN role r ON u.Role = r.RoleId
                WHERE u.Login = @login AND u.Password = @password";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Сохраняем данные пользователя в статическом классе
                            CurrentUser.UserID = reader["UserID"].ToString();
                            CurrentUser.Surname = reader["UserSurname"].ToString();
                            CurrentUser.Name = reader["UserName"].ToString();
                            CurrentUser.Patronymic = reader["UserPatronomic"].ToString();
                            CurrentUser.Role = reader["Role"].ToString();
                            CurrentUser.RoleName = reader["RoleName"].ToString();

                            MessageBox.Show($"Добро пожаловать, {CurrentUser.Name}!");

                            // В зависимости от роли, открыть соответствующую форму
                            switch (CurrentUser.RoleName.ToLower())
                            {
                                case "администратор":
                                    administrator adminForm = new administrator();
                                    adminForm.Show();
                                    break;
                                case "товаровед":
                                    MenuTovaroved tovarovedForm = new MenuTovaroved();
                                    tovarovedForm.Show();
                                    break;
                                case "продавец":
                                    MenuProdavec prodavecForm = new MenuProdavec();
                                    prodavecForm.Show();
                                    break;
                                default:
                                    MessageBox.Show("Неизвестная роль");
                                    break;
                            }

                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Некорректный логин или пароль");
                            textBox1.Clear();
                            textBox2.Clear();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка подключения к базе данных: " + ex.Message);
                }
            }
        }

        // Метод для хэширования пароля
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hash)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение выхода", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!isPasswordVisible)
            {
                // Показываем пароль
                textBox2.UseSystemPasswordChar = false;
                isPasswordVisible = true;
            }
            else
            {
                // Скрываем пароль
                textBox2.UseSystemPasswordChar = true;
                isPasswordVisible = false;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
