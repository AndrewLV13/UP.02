using System;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace Artikolly
{
    public partial class Authorization : Form
    {
        private bool isPasswordVisible = false;
        public Authorization()
        {
            InitializeComponent();
            textBox2.UseSystemPasswordChar = true;
            // Для отладки - показываем стандартные логин/пароль в заголовке
            string adminLogin = ConfigurationManager.AppSettings["AdminLogin"];
            string adminPass = ConfigurationManager.AppSettings["AdminPass"];
            this.Text = $"Авторизация (admin: {adminLogin}/{adminPass})";
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

            // 1. Проверка на стандартный логин/пароль из App.config
            string adminLogin = ConfigurationManager.AppSettings["AdminLogin"];
            string adminPass = ConfigurationManager.AppSettings["AdminPass"];

            if (login == adminLogin && password == adminPass)
            {
                // Устанавливаем данные для админа по умолчанию
                CurrentUser.UserID = "0";
                CurrentUser.Surname = "Администратор";
                CurrentUser.Name = "Системный";
                CurrentUser.Patronymic = "";
                CurrentUser.Role = "1"; // Администратор
                CurrentUser.RoleName = "Администратор";

                MessageBox.Show($"Добро пожаловать, системный администратор!");

                // Открываем форму Import для восстановления/импорта
                Import importForm = new Import();
                importForm.Show();
                this.Hide();
                return; // Выходим из метода, не проверяем БД
            }

            // 2. Проверка через БД (если БД существует)
            try
            {
                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string hashedPassword = HashPassword(password);

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
                                    return;
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
            }
            catch (MySqlException mysqlEx)
            {
                // Обработка ошибок MySQL (например, БД не существует)
                if (mysqlEx.Number == 1049) // Ошибка "Unknown database"
                {
                    MessageBox.Show("База данных не найдена. Пожалуйста, войдите как администратор (admin/admin) для восстановления БД.",
                                  "Ошибка базы данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    // Предлагаем войти как admin
                    textBox1.Text = adminLogin;
                    textBox2.Text = adminPass;
                }
                else
                {
                    MessageBox.Show("Ошибка подключения к базе данных: " + mysqlEx.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
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
