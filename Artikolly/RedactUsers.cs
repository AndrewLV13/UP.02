using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Artikolly
{
    public partial class RedactUsers : Form
    {
        private int userId;
        private string originalSurname;
        private string originalName;
        private string currentPasswordHash;
        public RedactUsers(int selectedUserId)
        {
            InitializeComponent();
            this.userId = selectedUserId; 
            LoadUserData();
        }

        private void LoadUserData()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = @"SELECT UserSurname, UserName, UserPatronomic, DateOfBirth, Phone, Login, Password, Role 
                               FROM user WHERE UserID = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", userId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        textBox1.Text = reader.GetString("UserSurname");
                        textBox2.Text = reader.GetString("UserName");
                        textBox3.Text = reader.IsDBNull(reader.GetOrdinal("UserPatronomic")) ? "" : reader.GetString("UserPatronomic");

                        // Обработка даты рождения - загружаем в dateTimePicker1
                        if (!reader.IsDBNull(reader.GetOrdinal("DateOfBirth")))
                        {
                            DateTime birthDate = reader.GetDateTime(reader.GetOrdinal("DateOfBirth"));
                            dateTimePicker1.Value = birthDate;
                        }

                        maskedTextBox2.Text = reader.GetString("Phone");
                        textBox5.Text = reader.GetString("Login");

                        // Сохраняем текущий хэш пароля, но не показываем его в поле
                        currentPasswordHash = reader.GetString("Password");
                        textBox6.Text = ""; // Оставляем поле пароля пустым

                        // Заполняем список ролей
                        var roles = GetRoles();
                        comboBox1.DataSource = roles;
                        comboBox1.DisplayMember = "RoleName";
                        comboBox1.ValueMember = "RoleID";

                        int roleId = reader.GetInt32("Role");
                        comboBox1.SelectedValue = roleId;

                        originalSurname = textBox1.Text;
                        originalName = textBox2.Text;
                    }
                }
            }
        }

        private List<Role> GetRoles()
        {
            var list = new List<Role>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT RoleID, RoleName FROM role";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Role
                        {
                            RoleID = reader.GetInt32("RoleID"),
                            RoleName = reader.GetString("RoleName")
                        });
                    }
                }
            }
            return list;
        }
        private bool ValidateData()
        {
            // Проверка обязательных полей (пароль теперь не обязателен)
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox2.Text) ||
                string.IsNullOrWhiteSpace(maskedTextBox2.Text) ||
                string.IsNullOrWhiteSpace(textBox5.Text) ||
                comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Необходимо заполнить все обязательные поля!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Проверка даты рождения (теперь через dateTimePicker1)
            DateTime birthDate = dateTimePicker1.Value;

            // Проверка, что дата рождения не в будущем (дополнительная проверка)
            if (birthDate > DateTime.Now)
            {
                MessageBox.Show("Дата рождения не может быть в будущем!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Проверка телефона (должно быть 11 цифр)
            string phoneDigits = new string(maskedTextBox2.Text.Where(char.IsDigit).ToArray());
            if (phoneDigits.Length != 11)
            {
                MessageBox.Show("Телефон должен содержать 11 цифр!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void SaveUserData()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Определяем, нужно ли обновлять пароль
                    string passwordToSave;
                    if (string.IsNullOrWhiteSpace(textBox6.Text))
                    {
                        // Если поле пароля пустое, используем старый хэш
                        passwordToSave = currentPasswordHash;
                    }
                    else
                    {
                        // Если введен новый пароль, хэшируем его
                        passwordToSave = HashPassword(textBox6.Text);
                    }

                    string query = @"UPDATE user 
                                    SET UserSurname = @surname, 
                                        UserName = @name, 
                                        UserPatronomic = @patronomic, 
                                        DateOfBirth = @dob, 
                                        Phone = @phone, 
                                        Login = @login, 
                                        Password = @password, 
                                        Role = @role 
                                    WHERE UserID = @id";

                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    // Получаем дату из dateTimePicker1
                    DateTime birthDate = dateTimePicker1.Value;

                    cmd.Parameters.AddWithValue("@surname", textBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@name", textBox2.Text.Trim());
                    cmd.Parameters.AddWithValue("@patronomic", string.IsNullOrWhiteSpace(textBox3.Text) ?
                        (object)DBNull.Value : textBox3.Text.Trim());
                    cmd.Parameters.AddWithValue("@dob", birthDate);
                    cmd.Parameters.AddWithValue("@phone", maskedTextBox2.Text);
                    cmd.Parameters.AddWithValue("@login", textBox5.Text.Trim());
                    cmd.Parameters.AddWithValue("@password", passwordToSave);
                    cmd.Parameters.AddWithValue("@role", comboBox1.SelectedValue);
                    cmd.Parameters.AddWithValue("@id", userId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Данные пользователя успешно обновлены!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить данные пользователя.", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка базы данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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
        private void pictureBox6_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ValidateData())
            {
                SaveUserData();
            }
        }

        //Фамилия
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем управляющие символы (Backspace, Delete и т.д.)
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Проверяем длину текста
            if (textBox1.Text.Length >= 20)
            {
                e.Handled = true;
                return;
            }

            // Разрешаем пробел и тире
            if (e.KeyChar == ' ' || e.KeyChar == '-')
            {
                return;
            }

            // Разрешаем только русские буквы
            if (!IsRussianLetter(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // Если текстовое поле пустое или курсор находится в начале, делаем букву заглавной
            // Но только если это буква, а не пробел или тире
            if ((textBox1.Text.Length == 0 || textBox1.SelectionStart == 0) && IsRussianLetter(e.KeyChar))
            {
                e.KeyChar = char.ToUpper(e.KeyChar);
            }
        }

        private bool IsRussianLetter(char c)
        {
            // Русские буквы в диапазоне от 'А' до 'я' (включая Ё и ё)
            return (c >= 'А' && c <= 'я') || c == 'Ё' || c == 'ё';
        }


        //Имя
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем управляющие символы (Backspace, Delete и т.д.)
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Проверяем длину текста
            if (textBox2.Text.Length >= 20)
            {
                e.Handled = true;
                return;
            }

            // Разрешаем пробел и тире
            if (e.KeyChar == ' ' || e.KeyChar == '-')
            {
                return;
            }

            // Разрешаем только русские буквы
            if (!IsRussianLetter(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // Если текстовое поле пустое или курсор находится в начале, делаем букву заглавной
            // Но только если это буква, а не пробел или тире
            if ((textBox2.Text.Length == 0 || textBox2.SelectionStart == 0) && IsRussianLetter(e.KeyChar))
            {
                e.KeyChar = char.ToUpper(e.KeyChar);
            }
        }


        //Отчество
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем управляющие символы (Backspace, Delete и т.д.)
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Проверяем длину текста
            if (textBox3.Text.Length >= 20)
            {
                e.Handled = true;
                return;
            }

            // Разрешаем пробел и тире
            if (e.KeyChar == ' ' || e.KeyChar == '-')
            {
                return;
            }

            // Разрешаем только русские буквы
            if (!IsRussianLetter(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // Если текстовое поле пустое или курсор находится в начале, делаем букву заглавной
            // Но только если это буква, а не пробел или тире
            if ((textBox3.Text.Length == 0 || textBox3.SelectionStart == 0) && IsRussianLetter(e.KeyChar))
            {
                e.KeyChar = char.ToUpper(e.KeyChar);
            }
        }


        


        //Телефон
        private void maskedTextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем управляющие символы
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Разрешаем только цифры
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // Проверяем максимальную длину (11 цифр для российского номера)
            string digitsOnly = new string(maskedTextBox2.Text.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length >= 11 && maskedTextBox2.SelectedText.Length == 0)
            {
                e.Handled = true;
            }
        }


        //Логин
        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
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

            // Разрешаем специальные символы для логина
            // Обычно разрешают: подчеркивание _, точку ., дефис -, @
            if (e.KeyChar == '_' || e.KeyChar == '.' || e.KeyChar == '-' || e.KeyChar == '@')
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


        //Пароль
        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }
    }

    public class Role
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
    }
}
